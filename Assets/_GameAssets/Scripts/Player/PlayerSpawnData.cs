using UnityEngine;

/// <summary>
/// Data necessary to spawn a player and their boat!
/// </summary>
[CreateAssetMenu(fileName = "PlayerSpawnData", menuName = "Player/PlayerSpawnData")]
public class PlayerSpawnData : ScriptableObject
{
    [field: SerializeField] public Player PlayerPrefab { get; private set; }
    [field: SerializeField] public BoatData BoatData { get; private set; }
}
