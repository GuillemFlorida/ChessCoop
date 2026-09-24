using UnityEngine;

namespace Zeftarim.ThirdPerson
{
    /// <summary>
    /// Controls the position of the AIM target based on the camera's forward direction, with limits and smoothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAimController : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField, Tooltip("The camera used to determine the aim direction. If left empty, the main camera will be used.")]
        private Transform mainCamera;

        [SerializeField, Tooltip("The AIM target that the character's bones point towards in the rig.")]
        private Transform aimTarget;

        [Header("Aim Configuration")]
        [SerializeField, Tooltip("Limit for how far left and right the character can look. Higher values allow looking more behind, but can cause unnatural twisting.")]
        [Range(45f, 150f)]
        private float maxLookAngle = 115f;

        [SerializeField, Tooltip("How fast the head sweeps across the screen to reach the other side.")]
        private float sweepSmoothTime = 0.15f;

        [SerializeField, Tooltip("Approximate height of the character's head.")]
        private float headHeightOffset = 1.5f;

        [SerializeField, Tooltip("Distance from the character's face where the AIM object floats.")]
        private float aimDistance = 3f;

        private float currentYaw;
        private float currentPitch;
        private float yawVelocity;
        private float pitchVelocity;

        private void LateUpdate()
        {
            if (mainCamera == null || aimTarget == null)
                return;

            Vector3 realAimDirection = mainCamera.forward;
            Vector3 localAimDir = transform.InverseTransformDirection(realAimDirection);

            float targetYaw = Mathf.Atan2(localAimDir.x, localAimDir.z) * Mathf.Rad2Deg;

            float horizontalDist = Mathf.Sqrt(localAimDir.x * localAimDir.x + localAimDir.z * localAimDir.z);
            float targetPitch = Mathf.Atan2(localAimDir.y, horizontalDist) * Mathf.Rad2Deg;

            float clampedYaw = Mathf.Clamp(targetYaw, -maxLookAngle, maxLookAngle);

            float clampedPitch = Mathf.Clamp(targetPitch, -60f, 60f);

            currentYaw = Mathf.SmoothDamp(currentYaw, clampedYaw, ref yawVelocity, sweepSmoothTime);
            currentPitch = Mathf.SmoothDamp(currentPitch, clampedPitch, ref pitchVelocity, sweepSmoothTime);

            Quaternion localRotation = Quaternion.Euler(-currentPitch, currentYaw, 0f);
            Vector3 worldSweepDirection = transform.TransformDirection(localRotation * Vector3.forward);

            Vector3 headPosition = transform.position + (Vector3.up * headHeightOffset);
            aimTarget.position = headPosition + (worldSweepDirection * aimDistance);
        }
    }
}