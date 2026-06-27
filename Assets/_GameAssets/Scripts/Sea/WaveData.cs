using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Game/WaveData")]
public class WaveData : ScriptableObject
{
    [SerializeField] private float waveHeight;
    [SerializeField] private Vector2 waveSpeedXZ;
    [SerializeField] private Vector2 waveFrequencyXZ;

    public float WaveHeight => waveHeight;
    public Vector2 Speed => waveSpeedXZ;
    public Vector2 Frequency => waveFrequencyXZ;

    private void OnValidate()
    {
        foreach(var sea in FindObjectsByType<Sea>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            sea.UpdateWaveShaderData();
        }
    }

    public float GetWaveHeightOffset(Vector3 position)
    {
        var heightX = Mathf.Sin((position.x + (Time.time * Speed.x)) * Frequency.x);
        var heightZ = Mathf.Sin((position.z + (Time.time * Speed.y)) * Frequency.y);
        return ((heightX + heightZ) / 2f) * WaveHeight;
    }
}
