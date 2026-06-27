using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using static VolumeVoxels;

public class BuoyantRigidbody : MonoBehaviour
{
    [SerializeField] private List<Collider> colliders;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Sea sea;
    [SerializeField, Min(0.1f)] private float buoyancyVoxelDensity = 1f; //voxels per metre on each axis
    [SerializeField, Min(0.1f)] private float surfacePointDensity = 1f;
    [SerializeField] private BuoyantRigidbodyStats stats;

    [SerializeField] private List<Voxel> volumeVoxels;
    [SerializeField] private List<VolumeVoxels.Voxel> surfacePoints;

    private void Awake()
    {
        GenerateVolumeVoxels();

        if (!sea)
        {
            var seas = FindObjectsByType<Sea>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
            if (seas.Length == 1)
            {
                sea = seas[0];
            }
            else if (seas.Length > 1)
            {
                Debug.LogWarning($">1 {nameof(Sea)} found in scene! Using first one.");
                sea = seas[0];
            }
            else
            {
                Debug.LogError($"No {nameof(Sea)} set and none found in scene!");
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateBuoyancyForces();
    }

    [ContextMenu("Generate volume voxels")]
    private void GenerateVolumeVoxels()
    {
        volumeVoxels = VolumeVoxels.GenerateVoxels(rb, colliders, buoyancyVoxelDensity);
    }

    private void UpdateBuoyancyForces()
    {
        if(!sea)
        {
            return;
        }

        //if(volumeVoxels == null || volumeVoxels.Count == 0 || surfacePoints == null || surfacePoints.Count == 0)
        if(volumeVoxels == null || volumeVoxels.Count == 0)
        {
            var dummyVoxel = new Voxel(Vector3.zero, Vector3.down);
            rb.AddForce(GetBuoyancyForce(dummyVoxel));
            return;
        }

        int numVoxelsUnderwater = 0;
        foreach (var voxel in volumeVoxels)
        {
            if(sea.IsPointUnderwater(rb.transform.TransformPoint(voxel.position_LS)))
            {
                numVoxelsUnderwater++;
            }
        }
            
        //increase damping based on fraction of voxels underwater to simulate water drag (TODO: necessary?/better way to do this?)
        if (numVoxelsUnderwater > 0f)
        {
            var totalForce = Vector3.zero;
            var forcePos = Vector3.zero;
            var forceVoxelsCount = 0;
            foreach(var voxel in volumeVoxels)
            {
                var buoyancyForce = GetBuoyancyForce(voxel);
                if(buoyancyForce.sqrMagnitude > Mathf.Epsilon)
                {
                    forceVoxelsCount++;
                    totalForce += buoyancyForce;
                    forcePos += rb.transform.TransformPoint(voxel.position_LS);
                    //rb.AddForceAtPosition(buoyancyForce, rb.transform.TransformPoint(voxel.position_LS));
                }
            }

            var fractionVoxelsUnderwater = numVoxelsUnderwater / (float)volumeVoxels.Count;
            if(forceVoxelsCount > 0)
            {
                var clampedForce = totalForce / forceVoxelsCount;
                clampedForce = clampedForce.normalized * Mathf.Min(clampedForce.magnitude, stats.MaxForceMagnitude);
                rb.AddForceAtPosition(clampedForce, forcePos / forceVoxelsCount);
            }

            rb.linearDamping = Mathf.Lerp(stats.MinLinearDamping, stats.MaxLinearDamping, fractionVoxelsUnderwater);
            rb.angularDamping = Mathf.Lerp(stats.MinAngularDamping, stats.MaxAngularDamping, fractionVoxelsUnderwater);
        }
    }

    private struct GetBuoyancyForceJob : IJobFor
    {
        [ReadOnly] public NativeList<Voxel> voxels;
        [ReadOnly] public NativeList<float> seaHeightDiffs; //TODO: make it so we can calculate this within the job?
        public NativeList<float3> forces;
        public float4x4 l2w;
        public float upMultiplier;
        public float forceMultiplier;
        public float maxDownForce;
        public float maxUpForce;

        [WriteOnly] public NativeList<float4> buoyancyForces; //hack! (x = voxel idx, yzw = force)

        public void Execute(int i)
        {
            var heightDiff = seaHeightDiffs[i];

            //if this voxel isn't underwater, don't give it a buoyancy force
            if (heightDiff < 0f)
            {
                return;
            }

            var normal_WS = math.transform(l2w, voxels[i].normal_LS);

            //adjust y component of normal according to depth below surface (pressure and upward buoyant force increases with water depth)
            var yForce = -normal_WS.y * heightDiff;
            if (yForce > 0f)
            {
                //artificial up multiplier 
                yForce *= upMultiplier;
            }

            var totalForce = new Vector3(normal_WS.x, yForce, normal_WS.z) * forceMultiplier;
            totalForce.y = Mathf.Clamp(totalForce.y, maxDownForce, maxUpForce);

            buoyancyForces.Add(new float4(i, totalForce));
        }
    }

    private Vector3 GetBuoyancyForce(Voxel voxel)
    {
        if (!sea || colliders == null || colliders.Count == 0)
        {
            return Vector3.zero;
        }

        var position_WS = rb.transform.TransformPoint(voxel.position_LS);
        var waveHeight = sea.GetWaveHeight_WS(position_WS);
        var heightDiff = waveHeight - position_WS.y;
        if(heightDiff < 0f)
        {
            return Vector3.zero;
        }

        var normal_WS = rb.transform.TransformDirection(voxel.normal_LS);
        
        //adjust y component of normal according to depth below surface (pressure and upward buoyant force increases with water depth)
        var yForce = -normal_WS.y * heightDiff;
        if(yForce > 0f)
        {
            //artificial up multiplier 
            yForce *= stats.UpMultiplier;
        }

        //var totalForce = new Vector3(normal_WS.x, yForce, normal_WS.z) * stats.ForceMultiplier;
        var totalForce = Vector3.up * yForce * stats.ForceMultiplier;
        //totalForce.y = Mathf.Clamp(totalForce.y, stats.MaxDownForce, stats.MaxUpForce);

        if (totalForce.magnitude > stats.MaxForceMagnitude)
        {
            totalForce = totalForce.normalized * stats.MaxForceMagnitude;
        }

        return totalForce;
    }

    private void DoSelfRighting()
    {
        var rollSelfRightAmount = GetSelfRightForce(transform.eulerAngles.x, stats.SelfRightActivationXAngle);
        var pitchSelfRightAmount = GetSelfRightForce(transform.eulerAngles.z, stats.SelfRightActivationZAngle);

        float GetSelfRightForce(float angle, float activationAngle)
        {
            angle = MathsUtils.Mod(angle, 360f);
            if(angle > activationAngle && angle < 360f - activationAngle)
            {
                var direction = angle > 180f ? 1f : -1f;
                return direction * stats.SelfRightingStrength; 
            }

            return 0f;
        }
    }

    private void DoSelfRightingPID(float angle, float targetAngle)
    {
        var error = targetAngle - angle;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.identity;

        //rb centre of mass
        if (rb)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(rb.worldCenterOfMass, 0.25f);
        }

        if(volumeVoxels != null)
        {
            foreach(var voxel in volumeVoxels)
            {
                var pos_WS = rb.transform.TransformPoint(voxel.position_LS);
                var normal_WS = rb.transform.TransformDirection(voxel.normal_LS);

                var force = GetBuoyancyForce(voxel);
                var usingBuoyancy = force.sqrMagnitude > Mathf.Epsilon;
                if(usingBuoyancy)
                {
                    var col = force.normalized;
                    Gizmos.color = new Color(Mathf.Abs(col.x), Mathf.Abs(col.y), Mathf.Abs(col.z));
                    Gizmos.DrawRay(pos_WS, -force);
                    Gizmos.DrawSphere(pos_WS, 0.1f / buoyancyVoxelDensity);
                }
                else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawRay(pos_WS, normal_WS);
                }

                if(sea && sea.IsPointUnderwater(pos_WS))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(pos_WS, 0.2f / buoyancyVoxelDensity);
                }
                else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(pos_WS, 0.1f / buoyancyVoxelDensity);
                }
            }
        }

        if (surfacePoints != null)
        {
            foreach (var point in surfacePoints)
            {
                var force = GetBuoyancyForce(point);
                var pos_WS = rb.transform.TransformPoint(point.position_LS);
                var normal_WS = rb.transform.TransformDirection(point.normal_LS);

                var usingBuoyancy = force.sqrMagnitude > Mathf.Epsilon;
                var col = usingBuoyancy ? force.normalized : normal_WS;
                Gizmos.color = new Color(Mathf.Abs(col.x), Mathf.Abs(col.y), Mathf.Abs(col.z));
                Gizmos.DrawRay(pos_WS, -force);
                Gizmos.DrawSphere(pos_WS, 1f / surfacePointDensity);
                if (!usingBuoyancy)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawRay(pos_WS, normal_WS);
                }
            }
        }
    }
}
