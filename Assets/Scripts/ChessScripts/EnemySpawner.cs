using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    public List<NetworkObject> listaPrefabsEnemigos;
    public LayerMask capaGround;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return; 
        EventManager.onbuttonpressed += PedirSpawn;
    }

    public override void OnNetworkDespawn()
    {
        EventManager.onbuttonpressed -= PedirSpawn;
    }

    public void PedirSpawn()
    {
        Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(rayo, out RaycastHit golpe, 1000f, capaGround))
        {
            ulong playerid = 0;

            foreach (var player in NetworkManager.Singleton.ConnectedClients)
            {
               playerid = player.Key;

                if(playerid != NetworkManager.Singleton.LocalClientId)
                {
                    break;
                }
            }

            SpawnEnServidorRpc(0, golpe.point,playerid);
        }
    }

    [Rpc(SendTo.Server)]
    void SpawnEnServidorRpc(int indice, Vector3 posicion, ulong playerid)
    {
        NetworkObject prefabElegido = listaPrefabsEnemigos[indice];

        NetworkObject enemigoInstanciado = Instantiate(prefabElegido, posicion, Quaternion.identity);

        enemigoInstanciado.GetComponent<EnemyBehaviour>().targetPlayerId = playerid;

        Transform pies = enemigoInstanciado.transform.Find("Pies");
        enemigoInstanciado.transform.position += posicion - pies.position;

        enemigoInstanciado.Spawn();
    }
}