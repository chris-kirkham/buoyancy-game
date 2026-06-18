using UnityEngine;

public class Sea : MonoBehaviour
{
    [SerializeField] private WaveData waveData;
    [SerializeField] private Renderer waveRenderer;

    private void Awake()
    {
        UpdateWaveShaderData();
    }

    private void OnValidate()
    {
        UpdateWaveShaderData();
    }

    [ContextMenu("Update wave shader data")]
    public void UpdateWaveShaderData()
    {
        if (waveData && waveRenderer)
        {
            var waveMat = waveRenderer.sharedMaterial;
            waveMat.SetFloat("_WaveHeight", waveData.WaveHeight);
            waveMat.SetVector("_WaveSpeed", waveData.Speed);
            waveMat.SetVector("_WaveFrequency", waveData.Frequency);
        }
    }

    public float GetWaveHeight_WS(Vector3 position)
    {
        if(!waveData)
        {
            return 0f;
        }

        return transform.position.y + waveData.GetWaveHeightOffset(position);
    }

    public bool IsPointUnderwater(Vector3 position_WS)
    {
        if(!waveData)
        {
            return false;
        }

        return GetWaveHeight_WS(position_WS) > position_WS.y;
    }

    private void OnDrawGizmosSelected()
    {
        if(!waveData)
        {
            return;
        }

        float posMult = 1f;
        for(float x = 0f; x < 50f; x++)
        {
            for(float z = 0f; z < 50f; z++)
            {
                var pos = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z) * posMult;
                var height = waveData.GetWaveHeightOffset(pos);
                var heightNormalised = ((height / waveData.WaveHeight) + 1f) / 2f; //from [-waveHeight, waveHeight] to [-1, 1] to [0, 1]
                //Gizmos.color = Color.Lerp(Color.blue, Color.white, heightNormalised);
                Gizmos.color = Color.HSVToRGB(heightNormalised, 1f, 1f);
                Gizmos.DrawSphere(pos + (Vector3.up * height), 0.1f);
            }
        }
    }
}
