using UnityEngine;

[CreateAssetMenu(fileName = "FollowCameraStats", menuName = "Player/FollowCameraStats")]
public class FollowCameraStats : ScriptableObject
{
    [field: SerializeField] public float DesiredDistance { get; private set; }
    [field: SerializeField] public float MinDistance { get; private set; }
    [field: SerializeField] public float MaxDistance { get; private set; }
    [field: SerializeField] public float DesiredHeightAboveTarget { get; private set; }
    [field: SerializeField] public float PositionFollowSpeed { get; private set; }
    [field: SerializeField] public float LookAtSpeed { get; private set; }
    [field: SerializeField] public Vector3 TargetLookOffset_LS { get; private set; }
}
