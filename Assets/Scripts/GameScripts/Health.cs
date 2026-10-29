using UnityEngine;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    public int vida = 100;   

    public void QuitarVida(int damage)
    {
        if (IsServer)
        {
            vida = vida - damage;
            Debug.Log("Has hecho daño a " + gameObject.name + ", le quedan " + vida + " de vida");

            if (vida <= 0)
            {
                Morir();
            }
        }
        else
        {
            QuitarVidaRpc(damage);
        }

    }

    void Morir()
    {
        Debug.Log(gameObject.name + " ha muerto");
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (IsServer && networkObject.IsSpawned)
        {
            networkObject.Despawn(true);
        }
    }

    [Rpc(SendTo.Server)]
    void QuitarVidaRpc(int damage)
    {
        QuitarVida(damage);
    }
}