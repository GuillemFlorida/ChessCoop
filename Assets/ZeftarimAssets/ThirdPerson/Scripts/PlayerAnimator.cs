using UnityEngine;

namespace Zeftarim.ThirdPerson
{
    /// <summary>
    /// Receives locomotion state and forwards it to the Animator component.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField, Tooltip("Animator on the character model child object.")]
        private Animator animator;

        [Header("Parameters")]
        [SerializeField, Tooltip("Float parameter used by the standing blend tree: 0 idle, 1 walk, 2 run.")]
        private string locomotionBlendParameter = "LocomotionBlend";

        [SerializeField, Tooltip("Bool parameter that selects the crouch branch. Suggested name: Crouch.")]
        private string crouchParameter = "Crouch";

        [SerializeField, Tooltip("Float parameter used by the crouch blend tree: 0 crouch idle, 1 crouch walk.")]
        private string crouchBlendParameter = "CrouchBlend";

        [SerializeField, Tooltip("Seconds used to smooth blend tree parameters.")]
        private float blendDampTime = 0.12f;

        private int locomotionBlendHash;
        private int crouchParameterHash;
        private int crouchBlendHash;

        /// <summary>
        /// Resolves the animator reference, caches parameter hashes, and disables root motion.
        /// </summary>
        private void Awake()
        {
            ResolveAnimator();
            CacheParameterHashes();
            ApplyRootMotionSetting();
        }

        /// <summary>
        /// Keeps the component auto-wired while editing in the inspector.
        /// </summary>
        private void OnValidate()
        {
            ResolveAnimator();
            CacheParameterHashes();
        }

        /// <summary>
        /// Applies locomotion state to the configured Animator.
        /// </summary>
        /// <param name="locomotionBlend">Standing locomotion blend: 0 idle, 1 walk, 2 run.</param>
        /// <param name="crouching">Whether the crouch tree should be active.</param>
        /// <param name="crouchBlend">Crouch locomotion blend: 0 crouch idle, 1 crouch walk.</param>
        public void SetMovementState(float locomotionBlend, bool crouching, float crouchBlend)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(locomotionBlendHash, locomotionBlend, blendDampTime, Time.deltaTime);
            animator.SetBool(crouchParameterHash, crouching);
            animator.SetFloat(crouchBlendHash, crouchBlend, blendDampTime, Time.deltaTime);
        }

        /// <summary>
        /// Forces root motion off so all locomotion is driven by code.
        /// </summary>
        public void ApplyRootMotionSetting()
        {
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        private void ResolveAnimator()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }
        }

        private void CacheParameterHashes()
        {
            locomotionBlendHash = Animator.StringToHash(locomotionBlendParameter);
            crouchParameterHash = Animator.StringToHash(crouchParameter);
            crouchBlendHash = Animator.StringToHash(crouchBlendParameter);
        }
    }
}
