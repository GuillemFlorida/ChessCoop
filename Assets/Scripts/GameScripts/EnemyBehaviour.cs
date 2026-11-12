using Unity.Netcode;
using UnityEngine;

public class EnemyBehaviour : NetworkBehaviour
{
    private Transform player;
    Rigidbody rb;
    public float speed = 3f;
    public float damage = 25f;
    public float range = 3f;
    public float cooldown = 1f;
    private float nextAttackTime = 0f;
    public float cost = 10f;
    public ulong targetPlayerId;



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()   
    {
        
        if (player == null)
        {
            NetworkObject playerObj = NetworkManager.Singleton.ConnectedClients[targetPlayerId].PlayerObject;//GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            return;
        }



        if (Time.time < nextAttackTime)
            return;

        if (Vector3.Distance(transform.position, player.position) <= range)
        {
            Attack();
            nextAttackTime = Time.time + cooldown;
            rb.constraints = RigidbodyConstraints.FreezePosition;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );
        }

    }

    void Attack()
    {
        Health health = player.GetComponent<Health>();

        if (health != null)
        {
            health.QuitarVida((int)damage);
        }
        rb.constraints = RigidbodyConstraints.FreezePosition;

    }
}