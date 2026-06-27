using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VolumeVoxels
{
    [System.Serializable]   
    public struct Voxel
    {
        public float3 position_LS;
        public float3 normal_LS;

        public Voxel(float3 position_LS, float3 normal_LS)
        {
            this.position_LS = position_LS;
            this.normal_LS = normal_LS;
        }
    }

    public static List<Voxel> GenerateVoxels(Rigidbody rb, List<Collider> colliders, float voxelDensity)
    {
        if(colliders == null || colliders.Count == 0)
        {
            Debug.LogError($"No colliders given for {rb.gameObject.name}! Cannot generate voxels.");
            return null;
        }

        Debug.Log($"Generating volume voxels for {rb.gameObject.name}...");

        var prevRotation = rb.transform.rotation;
        rb.transform.rotation = Quaternion.identity;

        var voxels_LS = new List<Voxel>();
        var com = rb.worldCenterOfMass;
        var inc = 1f / voxelDensity;

        //create AABB which encloses all colliders
        var allBounds = new Bounds(com, Vector3.zero);
        foreach(var coll in colliders)
        {
            allBounds.Encapsulate(coll.bounds);
        }

        const float paddingMult = 2f;
        var paddedExtents = allBounds.extents * paddingMult; 
        var min = allBounds.center - paddedExtents;
        var max = allBounds.center + paddedExtents;

        for (var x = min.x; x <= max.x; x += inc)
        {
            for (var y = min.y; y <= max.y; y += inc)
            {
                for (var z = min.z; z <= max.z; z += inc)
                {
                    foreach(var coll in colliders)
                    {
                        if (!coll.enabled)
                        {
                            //Collider.ClosestPoint always returns the input point if the collider is disabled
                            Debug.LogError($"Collider {coll.gameObject.name} must be enabled to generate voxel data!");
                            continue;
                        }

                        if (coll is MeshCollider && !((MeshCollider)coll).convex)
                        {
                            Debug.LogError($"MeshCollider {coll.gameObject.name} must be convex to generate voxel data!");
                            continue;
                        }

                        var pos_WS = new Vector3(x, y, z);
                        if (IsInsideCollider(pos_WS, coll))
                        {
                            var voxel = new Voxel();
                            voxel.position_LS = rb.transform.InverseTransformPoint(pos_WS);
                            voxel.normal_LS = rb.transform.InverseTransformDirection(GetVoxelNormal(pos_WS, coll));
                            voxels_LS.Add(voxel);

                            //if this point is inside one collider, skip the others so we don't duplicate voxels if colliders overlap
                            break;
                        }
                    }
                }
            }
        }

        rb.transform.rotation = prevRotation;

        Debug.Log($"Generated {voxels_LS.Count} volume voxels for {rb.gameObject.name}!");

        return voxels_LS;
    }

    public static async Task<NativeList<Voxel>> GenerateVoxelsJobVersion(Rigidbody rb, List<Collider> colliders, float voxelDensity)
    {
        if(colliders == null || colliders.Count == 0)
        {
            Debug.LogError($"No colliders given for {rb.gameObject.name}! Cannot generate voxels.");
            return new NativeList<Voxel>();
        }

        Debug.Log($"Generating volume voxels for {rb.gameObject.name}...");

        var collBounds = new NativeList<Bounds>(colliders.Count, Allocator.TempJob);
        foreach(var coll in colliders)
        {
            collBounds.Add(coll.bounds);
        }

        var testPoints = new NativeList<float3>(Allocator.TempJob);
        //var generatedVoxels = new NativeList<Voxel>(Allocator.Persistent);

        var prevRotation = rb.transform.rotation;
        rb.transform.rotation = Quaternion.identity;
        
        var job = new GenerateVoxelsJob()
        {
            l2w = rb.transform.localToWorldMatrix,
            w2l = rb.transform.worldToLocalMatrix,
            rbCoM_WS = rb.worldCenterOfMass,
            voxelDensity = voxelDensity,
            collBounds = collBounds,
            testPoints = testPoints,
            generatedVoxels = new NativeList<Voxel>(Allocator.TempJob)
        };

        var handle = job.Schedule();
        await Task.Yield();
        handle.Complete();

        rb.transform.rotation = prevRotation;

        var generatedVoxels = new NativeList<Voxel>(Allocator.Persistent);
        for(int i = 0; i < testPoints.Length; i++)
        {
            var pos = testPoints[i];
            foreach(var coll in colliders)
            {
                if (IsInsideCollider(pos, coll))
                {
                    generatedVoxels.Add(new Voxel(
                        rb.transform.InverseTransformPoint(pos),
                        rb.transform.InverseTransformDirection(GetVoxelNormal(pos, coll))));

                    break; //no need to check other colliders if one found for this point? Or should we use average normal of all enclosing colliders?
                }
            }
        }

        Debug.Log($"Generated volume voxels for {rb.gameObject.name}!");

        return generatedVoxels;
    }

    [BurstCompile]
    private struct GenerateVoxelsJob : IJob
    {
        public float4x4 l2w;
        public float4x4 w2l;
        public float3 rbCoM_WS;
        public float voxelDensity;
        [ReadOnly] public NativeList<Bounds> collBounds;
        public NativeList<float3> testPoints;
        public NativeList<Voxel> generatedVoxels;

        public void Execute()
        {
            //create AABB which encloses all colliders
            var objBounds = new Bounds(rbCoM_WS, Vector3.zero);
            for(int i = 0; i < collBounds.Length; i++)
            {
                objBounds.Encapsulate(collBounds[i]);
            }

            //TODO: this should cover everything (and be symmetrical from the RB's CoM) but can find a more efficient bound? Think about when I'm not tired
            //Find closest multiple of voxel increment that covers bounds
            float inc = 1f / voxelDensity;
            var mult = math.ceil(objBounds.extents / inc);
            var extent = mult * inc;
            float3 min = rbCoM_WS - extent;
            float3 max = rbCoM_WS + extent;

            for (var x = min.x; x <= max.x; x += inc)
            {
                for (var y = min.y; y <= max.y; y += inc)
                {
                    for (var z = min.z; z <= max.z; z += inc)
                    {
                        for (int i = 0; i < collBounds.Length; i++)
                        {
                            //var pos_WS = math.transform(l2w, new float3(x, y, z));
                            float3 pos_WS = new float3(x, y, z);
                            if (collBounds[i].Contains(pos_WS))
                            {
                                /*
                                var voxel = new Voxel();
                                voxel.position_LS = math.transform(w2l, pos_WS);
                                voxel.normal_LS = math.normalizesafe(math.transform(w2l, GetVoxelNormal(pos_WS, objBounds)));
                                generatedVoxels.Add(voxel);
                                */

                                testPoints.Add(pos_WS);

                                //if this point is inside one collider, skip the others so we don't duplicate voxels if colliders overlap
                                break;
                            }
                        }
                    }
                }
            }
        }

        private float3 GetVoxelNormal(float3 voxelPos_WS, Bounds objBounds)
        {
            float3 closestPoint = objBounds.ClosestPoint(voxelPos_WS);
            return math.normalizesafe(closestPoint - voxelPos_WS);
        }
    }

    private static bool IsInsideCollider(Vector3 point, Collider coll)
    {
        Debug.DrawRay(point, Vector3.up * 0.1f, Color.white, 10f);
        return Vector3.Distance(coll.ClosestPoint(point), point) <= Mathf.Epsilon;
    }

    private static Vector3 GetVoxelNormal(Vector3 point, Collider coll)
    {
        //TODO: maybe a more accurate way to get the "normals" of each voxel would be to do
        //(Collider.ClosestPoint(point) - point).normalized? Though this way we'd have to ensure the point is actually
        //inside the collider, not just near it w/ radius (if using radius/box check)
        //return (coll.ClosestPointOnBounds(point) - point).normalized;

        var closestPointOnBounds = coll.ClosestPointOnBounds(point);
        var centre = coll.attachedRigidbody ? coll.attachedRigidbody.worldCenterOfMass : coll.transform.position;
        var toCentre = centre - closestPointOnBounds;
        if (toCentre.sqrMagnitude <= Mathf.Epsilon)
        {
            return Vector3.zero;
        }

        var rayOrigin = centre - (toCentre * 10f);
        if (coll.Raycast(new Ray(rayOrigin, toCentre.normalized), out var hit, Mathf.Infinity))
        {
            return hit.normal;
        }
        else
        {
            Debug.Log($"Raycast failed on origin: {rayOrigin}, centre: {centre}");
            Debug.DrawLine(rayOrigin, centre, Color.red, 10f);
            return Vector3.zero;

            if (coll.attachedRigidbody)
            {
                return (point - coll.attachedRigidbody.worldCenterOfMass).normalized;
            }
            else
            {
                return (point - coll.transform.position).normalized;
            }
        }
    }

    public static List<Voxel> GenerateSurfacePoints(Rigidbody rb, List<Collider> colliders, float pointDensity)
    {
        Debug.Log($"Generating surface points for {rb.gameObject.name}...");

        if(colliders == null || colliders.Count == 0)
        {
            Debug.LogError($"No colliders for {rb.gameObject.name}! Cannot generate surface points.");
            return null;
        }

        var voxels_LS = new List<Voxel>();
        var com = rb.worldCenterOfMass;
        const float tau = Mathf.PI * 2f;
        var inc = tau * (1f / pointDensity);

        //create AABB which encloses all colliders
        var allBounds = new Bounds(com, Vector3.zero);
        foreach (var coll in colliders)
        {
            allBounds.Encapsulate(coll.bounds);
        }

        //spread points around a sphere and raycast in to get an approximately evenly-spaced 
        //array of surface points and normals - from https://discussions.unity.com/t/how-do-i-calculate-a-point-on-sphere-given-angles-and-radius/868529
        const float radiusPadding = 10f;
        var r = allBounds.size.magnitude * radiusPadding;
        for(float lon = 0f; lon < tau; lon += inc)
        {
            var equatorX = Mathf.Sin(lon);
            var equatorZ = Mathf.Cos(lon);

            for (float lat = 0f; lat < tau; lat += inc)
            {
                var y = Mathf.Sin(lat);
                var mult = Mathf.Cos(lat);
                var x = mult * equatorX;
                var z = mult * equatorZ;

                var rayOrigin = com + (new Vector3(x, y, z) * r);
                var rayDir = (com - rayOrigin).normalized;
                var ray = new Ray(rayOrigin, rayDir);
                foreach(var coll in colliders)
                {
                    if(coll.Raycast(ray, out var hit, Mathf.Infinity))
                    {
                        voxels_LS.Add(new Voxel(
                            rb.transform.InverseTransformPoint(hit.point),
                            rb.transform.InverseTransformDirection(hit.normal)));
                    }
                }
            }
        }

        Debug.Log($"Surface points generated for {rb.gameObject.name}");

        return voxels_LS;
    }
}
