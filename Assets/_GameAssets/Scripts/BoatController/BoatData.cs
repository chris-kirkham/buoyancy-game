using UnityEngine;

[CreateAssetMenu(fileName = "BoatData", menuName = "Game/Boat Data")]
public class BoatData : ScriptableObject
{
    [field: SerializeField] public BoatController BoatPrefab { get; private set; }
}
