using UnityEngine;

namespace Zeftarim.ThirdPerson
{
    /// <summary>
    /// Drives camera-relative third-person movement, rotation, gravity, and animation state.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("CharacterController used for physics-aware movement.")]
        private CharacterController characterController;

        [SerializeField, Tooltip("Input provider that exposes player movement and actions.")]
        private PlayerInputHandler inputHandler;

        [SerializeField, Tooltip("Animation bridge used to forward locomotion state to the model Animator.")]
        private PlayerAnimator playerAnimator;

        [SerializeField, Tooltip("Optional camera transform. If left empty, Camera.main will be used.")]
        private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField, Tooltip("Base walking speed in meters per second.")]
        private float walkSpeed = 3.5f;

        [SerializeField, Tooltip("Running speed in meters per second.")]
        private float runSpeed = 5f;

        [SerializeField, Tooltip("Crouched walking speed in meters per second.")]
        private float crouchSpeed = 1.5f;

        [SerializeField, Tooltip("Seconds used to interpolate toward the target facing direction.")]
        private float rotationLerpSpeed = 12f;

        [SerializeField, Tooltip("Dead zone used to ignore tiny stick input values.")]
        private float inputDeadZone = 0.01f;

        [SerializeField, Tooltip("Time to reach the target speed. Smaller is faster.")]
        private float accelerationSmoothTime = 0.05f;

        [SerializeField, Tooltip("Time to reach a full stop when input is released. Higher values create a more gradual deceleration.")]
        private float decelerationSmoothTime = 0.12f;

        [SerializeField, Tooltip("Time in seconds to reach the target rotation. Smaller is faster.")]
        private float rotationSmoothTime = 0.12f;

        private float speedVelocity;
        private float currentSpeed;
        private float rotationVelocity;

        [Header("Crouch")]
        [SerializeField, Tooltip("CharacterController height while crouching.")]
        private float crouchHeight = 1.1f;

        [SerializeField, Tooltip("Time in seconds to smoothly transition the stance. Smaller is faster.")]
        private float stanceSmoothTime = 0.15f;

        private float heightVelocity;
        private Vector3 centerVelocity;

        private float standingHeight;
        private Vector3 standingCenter;

        [Header("Gravity")]
        [SerializeField, Tooltip("Gravity applied while the controller is airborne.")]
        private float gravity = -24f;

        [SerializeField, Tooltip("Small downward force used to keep the controller grounded.")]
        private float groundedStickForce = -2f;

        [Header("Ground Check")]
        [SerializeField, Tooltip("Optional layers considered solid ground for the sphere cast.")]
        private LayerMask groundLayers = ~0;

        [SerializeField, Tooltip("Extra distance added to the ground sphere cast.")]
        private float groundCheckDistance = 0.15f;

        [SerializeField, Tooltip("Small radius multiplier applied to the CharacterController radius.")]
        private float groundCheckRadiusMultiplier = 0.95f;

        private float verticalVelocity;
        private Transform cachedCameraTransform;
        private bool isGrounded;

        /// <summary>
        /// Gets whether the character is grounded.
        /// </summary>
        public bool IsGrounded => isGrounded;

        /// <summary>
        /// Resolves dependencies before the controller starts updating.
        /// </summary>
        private void Awake()
        {
            ResolveDependencies();
            CacheStandingStance();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Keeps references aligned when values change in the inspector.
        /// </summary>
        private void OnValidate()
        {
            ResolveDependencies();
            if (Application.isPlaying)
            {
                CacheStandingStance();
            }
        }

        /// <summary>
        /// Updates movement, rotation, gravity, and animation once per frame.
        /// </summary>
        private void Update()
        {
            if (!ResolveCameraTransform() || characterController == null || inputHandler == null)
                return;

            float deltaTime = Time.deltaTime;
            Vector2 input = inputHandler.MoveInput;
            Vector3 targetDirection = CalculateCameraRelativeDirection(input);
            bool hasMoveInput = targetDirection.sqrMagnitude > 0f;
            bool isCrouching = inputHandler.IsCrouching;
            bool isRunning = inputHandler.IsRunning && !isCrouching;

            if (hasMoveInput)
            {
                float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            UpdateStance(isCrouching);


            float targetSpeed = 0f;

            if (hasMoveInput)
            {
                if (isCrouching)
                {
                    targetSpeed = crouchSpeed;
                }
                else if (isRunning)
                {
                    targetSpeed = runSpeed;
                }
                else
                {
                    targetSpeed = walkSpeed;
                }
            }


            float currentSmoothTime = hasMoveInput ? accelerationSmoothTime : decelerationSmoothTime;

            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, currentSmoothTime);

            if (currentSpeed < 0.01f) currentSpeed = 0f;

            Vector3 horizontalVelocity = transform.forward * currentSpeed;

            isGrounded = characterController.isGrounded || ProbeGrounded();

            if (isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
            }
            else
            {
                verticalVelocity += gravity * deltaTime;
            }

            Vector3 displacement = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(displacement * deltaTime);

            if (playerAnimator != null)
            {
                float locomotionBlend = 0f;
                float crouchBlend = 0f;

                if (hasMoveInput)
                {
                    locomotionBlend = isRunning ? 2f : 1f;
                    crouchBlend = 1f;
                }

                playerAnimator.SetMovementState(locomotionBlend, isCrouching, crouchBlend);
            }
        }

        /// <summary>
        /// Resolves all dependencies from the current GameObject hierarchy.
        /// </summary>
        private void ResolveDependencies()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (inputHandler == null)
            {
                inputHandler = GetComponent<PlayerInputHandler>();
            }

            if (playerAnimator == null)
            {
                playerAnimator = GetComponentInChildren<PlayerAnimator>(true);
            }

            if (cameraTransform != null)
            {
                cachedCameraTransform = cameraTransform;
            }

        }

        /// <summary>
        /// Stores the standing stance values so crouch can be blended back to the original state.
        /// </summary>
        private void CacheStandingStance()
        {
            if (characterController != null)
            {
                standingHeight = characterController.height;
                standingCenter = characterController.center;
            }
        }

        /// <summary>
        /// Smoothly changes the character controller height and dynamically adjusts the camera target.
        /// </summary>
        /// <param name="isCrouching">True when crouch is active.</param>
        private void UpdateStance(bool isCrouching)
        {
            if (characterController == null || standingHeight <= 0f) return;

            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            characterController.height = Mathf.SmoothDamp(characterController.height, targetHeight, ref heightVelocity, stanceSmoothTime);

            float standingBottom = standingCenter.y - standingHeight * 0.5f;
            Vector3 targetCenter = standingCenter;
            targetCenter.y = standingBottom + characterController.height * 0.5f;
            characterController.center = Vector3.SmoothDamp(characterController.center, targetCenter, ref centerVelocity, stanceSmoothTime);

        }

        /// <summary>
        /// Resolves the transform used for camera-relative motion.
        /// </summary>
        /// <returns>True when a valid camera transform is available.</returns>
        private bool ResolveCameraTransform()
        {
            if (cachedCameraTransform != null)
            {
                return true;
            }

            if (cameraTransform != null)
            {
                cachedCameraTransform = cameraTransform;
                return true;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cachedCameraTransform = mainCamera.transform;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts the raw input into a flattened world-space movement direction.
        /// </summary>
        /// <param name="input">Raw movement input from the New Input System.</param>
        /// <returns>A normalized world-space direction, or zero when there is no input.</returns>
        private Vector3 CalculateCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude <= inputDeadZone * inputDeadZone)
            {
                return Vector3.zero;
            }

            Vector3 forward = cachedCameraTransform.forward;
            Vector3 right = cachedCameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 direction = forward * input.y + right * input.x;
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            return direction;
        }

        /// <summary>
        /// Smoothly rotates the character toward the provided movement direction.
        /// </summary>
        /// <param name="direction">Target world-space movement direction.</param>
        /// <param name="deltaTime">Frame delta time.</param>
        private void RotateTowards(Vector3 direction, float deltaTime)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * deltaTime);
        }

        /// <summary>
        /// Performs a sphere cast to validate ground contact beyond the CharacterController's built-in flag.
        /// </summary>
        /// <returns>True if the character is touching the ground.</returns>
        private bool ProbeGrounded()
        {
            float radius = characterController.radius * groundCheckRadiusMultiplier;
            Vector3 origin = transform.position + characterController.center + Vector3.up * Mathf.Max(0f, characterController.height * 0.5f - radius);
            float castDistance = groundCheckDistance + 0.05f;

            return Physics.SphereCast(origin, radius, Vector3.down, out _, castDistance, groundLayers, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Automatically adjusts the CharacterController's capsule based on the character's mesh.
        /// </summary>
        [ContextMenu("Auto-Fit Capsule To Mesh")]
        private void AutoFitCapsule()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            if (renderers.Length == 0)
            {
                Debug.LogWarning("No skinned mesh renderers found in children to calculate size.");
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);

            characterController.height = bounds.size.y;

            characterController.center = new Vector3(0f, localCenter.y, 0f);

            characterController.radius = Mathf.Max(bounds.size.x, bounds.size.z) / 3.5f;

            Debug.Log("CharacterController capsule auto-fitted to mesh bounds. Height: " + characterController.height + ", Center: " + characterController.center + ", Radius: " + characterController.radius);
        }
    }
}
