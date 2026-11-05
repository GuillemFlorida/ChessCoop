using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    public List<NetworkObject> listaPrefabsEnemigos;

    public override void OnNetworkSpawn()
    {
        EventManager.onbuttonpressed += PedirSpawn;
    }

    public override void OnNetworkDespawn()
    {
        EventManager.onbuttonpressed -= PedirSpawn;
    }
    public void PedirSpawn()
    {
        SpawnEnServidorRpc(0, transform.position);
    }

    [Rpc(SendTo.Server)]
    void SpawnEnServidorRpc(int indice, Vector3 posicion)
    {
        NetworkObject prefabElegido = listaPrefabsEnemigos[indice];

        NetworkObject enemigoInstanciado = Instantiate(prefabElegido, posicion, Quaternion.identity);
        enemigoInstanciado.Spawn();
    }
}