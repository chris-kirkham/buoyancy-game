using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class BuoyantRigidbody : MonoBehaviour
{
    [SerializeField] private List<Collider> colliders;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform manualCentreOfMass;
    [SerializeField] private Sea sea;
    [SerializeField] private float buoyancyMultiplier = 1f;
    [SerializeField, Min(0.1f)] private float buoyancyVoxelDensity = 1f; //voxels per metre on each axis

    private List<VolumeVoxels.Voxel> volumeVoxels;

    private const float maxDownForce = -1f;
    private const float maxUpForce = 10f;
    private const float maxForceMagnitude = 10f;

    private float buoyancyMultByVoxelDensity => buoyancyMultiplier / buoyancyVoxelDensity;

    private void Awake()
    {
        if(manualCentreOfMass)
        {
            rb.centerOfMass = rb.transform.InverseTransformPoint(manualCentreOfMass.position);
        }

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
        volumeVoxels = VolumeVoxels.GenerateVolumeVoxelsForColliders(colliders, buoyancyVoxelDensity);
    }

    private void UpdateBuoyancyForce()
    {
        if(!sea)
        {
            return;
        }

        if(volumeVoxels != null && volumeVoxels.Count > 0)
        {
            var totalForce_WS = Vector3.zero;
            var avgForcePosition_WS = Vector3.zero;
            foreach (var voxel in volumeVoxels)
            {
                //totalForce_WS += GetBuoyancy(voxel);
                //avgForcePosition_WS += voxel.coll.transform.TransformPoint(voxel.position_LS);
                rb.AddForceAtPosition(GetBuoyancy(voxel), voxel.coll.transform.TransformPoint(voxel.position_LS));
            }

            //avgForcePosition_WS /= volumeVoxels.Count;
            //rb.AddForceAtPosition(totalForce_WS, avgForcePosition_WS);
        }
        else //no volume voxels - add downward force at Rigidbody origin
        {
            var voxel = new VolumeVoxels.Voxel(colliders[0], Vector3.zero, Vector3.down);
            rb.AddForce(GetBuoyancy(voxel));
        }
    }

    private Vector3 GetBuoyancy(VolumeVoxels.Voxel voxel)
    {
        if(!sea || colliders == null || colliders.Count == 0)
        {
            return Vector3.zero;
        }
        
        var position_WS = voxel.coll.transform.TransformPoint(voxel.position_LS);
        var normal_WS = voxel.coll.transform.TransformDirection(voxel.normal_LS);
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
                var pos_WS = voxel.coll.transform.TransformPoint(voxel.position_LS);
                var normal_WS = voxel.coll.transform.TransformDirection(voxel.normal_LS);
                    
                var usingBuoyancy = force.sqrMagnitude > Mathf.Epsilon;
                var col = usingBuoyancy ? force.normalized : normal_WS;
                Gizmos.color = new Color(Mathf.Abs(col.x), Mathf.Abs(col.y), Mathf.Abs(col.z));
                Gizmos.DrawRay(pos_WS, -force);
                Gizmos.DrawSphere(pos_WS, 0.1f / buoyancyVoxelDensity);
                if(!usingBuoyancy)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawRay(pos_WS, normal_WS);
                }
            }
        }

    }
}
