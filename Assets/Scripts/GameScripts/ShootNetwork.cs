using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;



public class ShootNetwork : NetworkBehaviour
{
    InputSystem_Actions inputActions;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform playerArmature;
    [SerializeField] private float shootRange = 100.0f;
    [SerializeField] private int shootDamage = 40;   

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
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
        if(Physics.Raycast(shootPoint.position, playerArmature.forward,out RaycastHit hit, shootRange))
        {
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
    }

    [Rpc(SendTo.Server)]
     void PlayerHitServerRPC(ulong id)
    {
        Debug.Log("Has disparado a " + id);
    }
}