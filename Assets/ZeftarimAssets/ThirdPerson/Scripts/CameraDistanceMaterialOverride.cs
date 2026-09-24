using UnityEngine;

namespace Zeftarim.ThirdPerson
{
    public class CameraDistanceFadeOverride : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkinnedMeshRenderer playerRenderer;
        [SerializeField, Tooltip("El Camera_Target que usa Cinemachine como pivote")] 
        private Transform cameraTarget;

        [Header("Settings")]
        [SerializeField] private float startFadeDistance = 1.05f;
        [SerializeField] private float fullTransparentDistance = 0.3f;

        private Material playerMaterialInstance;

        private void Start()
        {
            if (playerRenderer != null)
            {
                // Al usar .material, Unity crea automáticamente un clon en memoria.
                // Esto asegura que no modifiquemos el asset original del usuario en el editor.
                playerMaterialInstance = playerRenderer.material;
            }
        }

        private void LateUpdate()
        {
            if (playerMaterialInstance != null && cameraTarget != null)
            {
                // Inyectamos las variables al material actual en cada frame
                playerMaterialInstance.SetVector("_TargetPosition", cameraTarget.position);
                playerMaterialInstance.SetFloat("_MaxDistance", startFadeDistance);
                playerMaterialInstance.SetFloat("_MinDistance", fullTransparentDistance);
            }
        }
    }
}