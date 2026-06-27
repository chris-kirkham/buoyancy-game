using System.Collections.Generic;
using UnityEngine;

public class Sea : MonoBehaviour
{
    [SerializeField] private WaveData waveData;
    [SerializeField] private Renderer wavePlanePrefab;
    [SerializeField] private float wavePlaneScale = 1f;
    [SerializeField] private Vector2Int wavePlaneGridSize;

    private List<Renderer> wavePlanes;

    private void OnEnable()
    {
        UpdateWavePlanes();
        UpdateWaveShaderData();
    }

    private void OnValidate()
    {
        UpdateWaveShaderData();
    }

    public void UpdateWaveShaderData()
    {
        if (!waveData || wavePlanes == null || wavePlanes.Count == 0)
        {
            return;
        }
     
        var waveMat = wavePlanes[0].sharedMaterial;
        waveMat.SetFloat("_WaveHeight", waveData.WaveHeight);
        waveMat.SetVector("_WaveSpeed", waveData.Speed);
        waveMat.SetVector("_WaveFrequency", waveData.Frequency);
    }

    private void UpdateWavePlanes()
    {
        if(!wavePlanePrefab)
        {
            Debug.LogError($"Wave plane prefab not set!");
            return;
        }

        if(wavePlanes == null)
        {
            wavePlanes = new List<Renderer>(wavePlaneGridSize.x * wavePlaneGridSize.y);
        }
        else //remove existing planes
        {
            foreach(var wavePlane in wavePlanes)
            {
                if(wavePlane)
                {
                    Destroy(wavePlane.gameObject);
                }
            }

            wavePlanes.Clear();
        }

        for(int x = 0; x < wavePlaneGridSize.x; x++)
        {
            for(int z = 0; z < wavePlaneGridSize.y; z++)
            {
                var newWavePlane = Instantiate(wavePlanePrefab, transform);
                newWavePlane.transform.localScale *= wavePlaneScale;
                newWavePlane.transform.localPosition = new Vector3(x, 0f, z) * wavePlaneScale;
                wavePlanes.Add(newWavePlane);
            }
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

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.white;
        for(int x = 0; x < wavePlaneGridSize.x; x++)
        {
            for(int z = 0; z < wavePlaneGridSize.y; z++)
            {
                var pos = new Vector3(
                    transform.position.x + (x * wavePlaneScale),
                    transform.position.y,
                    transform.position.z + (z * wavePlaneScale));

                if (waveData)
                {
                    //Draw wave height preview
                    var height = waveData.GetWaveHeightOffset(pos);
                    var heightNormalised = ((height / waveData.WaveHeight) + 1f) / 2f; //from [-waveHeight, waveHeight] to [-1, 1] to [0, 1]

                    Gizmos.color = Color.HSVToRGB(heightNormalised, 1f, 1f);
                    
                    var offsetPos = pos + (Vector3.up * height);
                    Gizmos.DrawLine(pos, offsetPos);
                    Gizmos.DrawSphere(offsetPos, 0.1f * wavePlaneScale);

                    //Gizmos.color = Color.Lerp(Color.blue, Color.white, heightNormalised);
                }

                //Draw wave plane preview
                Gizmos.DrawWireCube(pos, new Vector3(1f, 0f, 1f) * wavePlaneScale);
            }
        }
    }
}
