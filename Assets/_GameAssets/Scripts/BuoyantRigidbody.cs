using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class BuoyantRigidbody : MonoBehaviour
{
    [SerializeField] private Collider coll;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Sea sea;
    [SerializeField] private float buoyancyMultiplier = 1f;
    [SerializeField] private List<Transform> buoyancySamplePoints;

    private List<VolumeVoxels.Voxel> volumeVoxels;

    private const float maxDownForce = -1f;
    private const float maxUpForce = 10f;
    private const float maxForceMagnitude = 10f;

    private float buoyancyMultByVoxelDensity => buoyancyMultiplier / VolumeVoxels.VoxelDensity;

    private void Awake()
    {
        GenerateVolumeVoxels();
    }

    private void OnValidate()
    {
        GenerateVolumeVoxels();
    }

    private void FixedUpdate()
    {
        UpdateBuoyancyForce();
    }

    private void GenerateVolumeVoxels()
    {
        if(!coll)
        {
            return;
        }

        volumeVoxels = VolumeVoxels.GenerateVolumeVoxelsForCollider(coll);
    }

    private void UpdateBuoyancyForce()
    {
        if(!sea)
        {
            return;
        }

        if(volumeVoxels != null && volumeVoxels.Count > 0)
        {
            foreach (var voxel in volumeVoxels)
            {
                rb.AddForceAtPosition(GetBuoyancy(voxel), coll.transform.TransformPoint(voxel.position_LS));
            }
        }
        else //no volume voxels - add downward force at Rigidbody origin
        {
            var voxel = new VolumeVoxels.Voxel
            {
                position_LS = Vector3.zero,
                normal_LS = Vector3.down
            };

            rb.AddForce(GetBuoyancy(voxel));
        }

    }

    private Vector3 GetBuoyancy(VolumeVoxels.Voxel voxel)
    {
        if(!coll)
        {
            return Vector3.zero;
        }
        
        var position_WS = coll.transform.TransformPoint(voxel.position_LS);
        var normal_WS = coll.transform.TransformDirection(voxel.normal_LS);
        var waveHeight = sea.GetWaveHeight_WS(position_WS);
        var heightDiff = waveHeight - position_WS.y;
        if (heightDiff > 0f)
        {
            //adjust y component of normal according to depth below surface (pressure and upward buoyant force increases with water depth)
            var yForce = -normal_WS.y * heightDiff;
            yForce = Mathf.Clamp(yForce, maxDownForce, maxUpForce);
            var force = new Vector3(normal_WS.x, yForce, normal_WS.z) * buoyancyMultByVoxelDensity;
            if(force.magnitude > maxForceMagnitude)
            {
                force = force.normalized * maxForceMagnitude;
            }
            
            //DEBUG
            //var col = force.normalized;
            //Debug.DrawRay(position_WS, -force, new Color(Mathf.Abs(col.x), Mathf.Abs(col.y), Mathf.Abs(col.z)));
            
            return force;
        }

        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if(volumeVoxels != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            foreach(var voxel in volumeVoxels)
            {
                var force = GetBuoyancy(voxel);
                var pos_WS = coll.transform.TransformPoint(voxel.position_LS);
                var normal_WS = coll.transform.TransformDirection(voxel.normal_LS);
                    
                var usingBuoyancy = force.sqrMagnitude > Mathf.Epsilon;
                var col = usingBuoyancy ? force.normalized : normal_WS;
                Gizmos.color = new Color(Mathf.Abs(col.x), Mathf.Abs(col.y), Mathf.Abs(col.z));
                Gizmos.DrawRay(pos_WS, -force);
                Gizmos.DrawSphere(pos_WS, 0.1f / VolumeVoxels.VoxelDensity);
                if(!usingBuoyancy)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawRay(pos_WS, normal_WS);
                }
            }
        }

    }
}
