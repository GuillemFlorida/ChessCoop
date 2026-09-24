using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Zeftarim.ThirdPerson
{
    /// <summary>
    /// Reads the New Input System and exposes the current player input state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Asset")]
        [SerializeField, Tooltip("Assign the local CharacterController.inputactions asset from Assets/Zeftarim/ThirdPerson.")]
        private InputActionAsset controllerInputActions;

        private const string ActionMapName = "Player";
        private const string MoveActionName = "Move";
        private const string RunActionName = "Run";
        private const string CrouchActionName = "Crouch";

        private InputActionMap playerActionMap;
        private InputAction moveAction;
        private InputAction runAction;
        private InputAction crouchAction;

        /// <summary>
        /// Current movement input in local player space.
        /// </summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>
        /// True while run is held.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// True while crouch is held.
        /// </summary>
        public bool IsCrouching { get; private set; }

        /// <summary>
        /// Fired when movement input changes.
        /// </summary>
        public event Action<Vector2> MoveInputChanged;

        /// <summary>
        /// Fired when the run state changes.
        /// </summary>
        public event Action<bool> RunStateChanged;

        /// <summary>
        /// Fired when the crouch state changes.
        /// </summary>
        public event Action<bool> CrouchStateChanged;

        private void OnEnable()
        {
            CacheActions();
            BindActions();
        }

        private void OnDisable()
        {
            UnbindActions();

            if (playerActionMap != null)
            {
                playerActionMap.Disable();
            }

            MoveInput = Vector2.zero;
            IsRunning = false;
            IsCrouching = false;
        }

        /// <summary>
        /// Returns the current movement vector.
        /// </summary>
        public Vector2 GetMoveInput()
        {
            return MoveInput;
        }

        private void CacheActions()
        {
            if (controllerInputActions == null)
            {
                Debug.LogError($"{nameof(PlayerInputHandler)} on {name} requires the local CharacterController.inputactions asset.", this);
                return;
            }

            playerActionMap = controllerInputActions.FindActionMap(ActionMapName, true);
            moveAction = playerActionMap.FindAction(MoveActionName, true);
            runAction = playerActionMap.FindAction(RunActionName, false);
            crouchAction = playerActionMap.FindAction(CrouchActionName, false);
        }

        private void BindActions()
        {
            if (playerActionMap == null)
            {
                return;
            }

            playerActionMap.Enable();

            moveAction.performed += HandleMoveChanged;
            moveAction.canceled += HandleMoveChanged;

            if (runAction != null)
            {
                runAction.performed += HandleRunPerformed;
                runAction.canceled += HandleRunCanceled;
            }

            if (crouchAction != null)
            {
                crouchAction.performed += HandleCrouchPerformed;
            }
        }

        private void UnbindActions()
        {
            if (moveAction != null)
            {
                moveAction.performed -= HandleMoveChanged;
                moveAction.canceled -= HandleMoveChanged;
            }

            if (runAction != null)
            {
                runAction.performed -= HandleRunPerformed;
                runAction.canceled -= HandleRunCanceled;
            }

            if (crouchAction != null)
            {
                crouchAction.performed -= HandleCrouchPerformed;
            }
        }

        private void HandleMoveChanged(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
            MoveInputChanged?.Invoke(MoveInput);
        }

        private void HandleRunPerformed(InputAction.CallbackContext context)
        {
            IsRunning = true;
            RunStateChanged?.Invoke(true);
        }

        private void HandleRunCanceled(InputAction.CallbackContext context)
        {
            IsRunning = false;
            RunStateChanged?.Invoke(false);
        }

        private void HandleCrouchPerformed(InputAction.CallbackContext context)
        {
            IsCrouching = !IsCrouching;
            CrouchStateChanged?.Invoke(IsCrouching);
        }
    }
}
