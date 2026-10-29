using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public class ShootNetwork : NetworkBehaviour
{
    InputSystem_Actions inputActions;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform playerArmature;
    [SerializeField] private float shootRange = 100.0f;
    [SerializeField] private int shootDamage = 40;

    [Header("Visuales de Disparo")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float laserDuration = 0.05f;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();


        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        inputActions.Player.Attack.performed += OnAttack;
        inputActions.Enable();
    }

    public override void OnNetworkDespawn()
    {
        inputActions.Player.Attack.performed -= OnAttack;
        inputActions.Disable();
    }

    private void FixedUpdate()
    {

    }

    void OnAttack(InputAction.CallbackContext context)
    {
        Vector3 rayOrigin = shootPoint.position;
        Vector3 rayDirection = playerArmature.forward;
        Vector3 rayEndPoint = rayOrigin + (rayDirection * shootRange);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, shootRange))
        {

            rayEndPoint = hit.point;

            Health vida = hit.collider.GetComponent<Health>();
            if (vida != null)
            {
                vida.QuitarVida(shootDamage);
            }

            NetworkObject networkObject = hit.collider.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                PlayerHitServerRPC(networkObject.NetworkObjectId);
            }
        }


        if (lineRenderer != null)
        {
            ShowLaserRpc(rayOrigin, rayEndPoint);
        }
    }


    private IEnumerator ShowLaser(Vector3 start, Vector3 end)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        yield return new WaitForSeconds(laserDuration);

        lineRenderer.enabled = false;
    }

    [Rpc(SendTo.Server)]
    void PlayerHitServerRPC(ulong id)
    {
        Debug.Log("Has disparado a " + id);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ShowLaserRpc(Vector3 start, Vector3 end)
    {
        StartCoroutine(ShowLaser(start, end));
    }
}