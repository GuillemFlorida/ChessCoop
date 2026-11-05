using UnityEngine;
using Unity.Netcode; 

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Chess/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Información Básica")]
    public string enemyName;
    public int cost = 10;

    [Header("Referencia de Red")]
    public NetworkObject prefab;
}