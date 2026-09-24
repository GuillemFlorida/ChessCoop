using UnityEngine;
using UnityEngine.InputSystem;

namespace Zeftarim.ThirdPerson
{
    public class CameraTargetRig : MonoBehaviour
    {
        [Header("Tracking")]
        [SerializeField, Tooltip("The root object of the player to follow.")]
        private Transform playerToFollow;

        [SerializeField, Tooltip("Percentage of the character's total height to use as the pivot.")]
        [Range(0.1f, 1f)]
        private float heightRatio = 0.85f;

        [Header("Rotation Input")]
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private float sensitivityX = 0.2f;
        [SerializeField] private float sensitivityY = 0.2f;

        [Header("Limits (Clamp)")]
        [SerializeField] private float minPitch = -50f;
        [SerializeField] private float maxPitch = 60f;

        private CharacterController playerController;
        private float yaw;
        private float pitch;

        private void Awake()
        {
            transform.SetParent(null);
        }

        private void Start()
        {
            if (playerToFollow != null)
            {
                playerController = playerToFollow.GetComponent<CharacterController>();
            }
        }

        private void LateUpdate()
        {
            if (playerToFollow == null)
            {
                Destroy(gameObject);
                return;
            }

            float currentHeightOffset = 1.4f;
            if (playerController != null)
            {
                currentHeightOffset = playerController.height * heightRatio;
            }

            transform.position = playerToFollow.position + (Vector3.up * currentHeightOffset);

            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

            yaw += lookInput.x * sensitivityX;
            pitch -= lookInput.y * sensitivityY;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}