using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class AimCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera followCamera;
    private float targetFOV = 50f;

    void Update()
    {
        targetFOV = Mouse.current.rightButton.isPressed ? 25f : 50f;

        var lens = followCamera.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, 10f * Time.deltaTime);
        followCamera.Lens = lens;
    }
}