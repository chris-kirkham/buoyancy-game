using System.Collections.Generic;
using UnityEngine;

public class VolumeVoxels
{
    public struct Voxel
    {
        public Collider coll;
        public Vector3 position_LS;
        public Vector3 normal_LS;

        public Voxel(Collider coll, Vector3 position_LS, Vector3 normal_LS)
        {
            this.coll = coll;
            this.position_LS = position_LS;
            this.normal_LS = normal_LS;
        }
    }

    public static List<Voxel> GenerateVolumeVoxelsForColliders(List<Collider> colliders, float voxelDensity)
    {
        if(colliders == null || colliders.Count == 0)
        {
            return null;
        }

        var voxels_LS = new List<Voxel>();

        foreach(var coll in colliders)
        {
            var min = coll.bounds.min;
            var max = coll.bounds.max;
            var inc = 1f / voxelDensity;

            for (var x = min.x; x <= max.x; x += inc)
            {
                for (var y = min.y; y <= max.y; y += inc)
                {
                    for (var z = min.z; z <= max.z; z += inc)
                    {
                        var pos_WS = new Vector3(x, y, z);
                        if (IsInsideCollider(pos_WS, coll))
                        {
                            var voxel = new Voxel();
                            voxel.coll = coll;
                            voxel.position_LS = coll.transform.InverseTransformPoint(pos_WS);
                            voxel.normal_LS = coll.transform.InverseTransformDirection(GetVoxelNormal(pos_WS, coll));
                            voxels_LS.Add(voxel);
                        }
                    }
                }
            }
        }

        return voxels_LS;

        bool IsInsideCollider(Vector3 point, Collider coll)
        {
            if (!coll.enabled)
            {
                //Collider.ClosestPoint always returns the input point if the collider is disabled lmao
                Debug.LogError($"Collider must be enabled for this function to work!");
                return false;
            }

            return Vector3.Distance(coll.ClosestPoint(point), point) <= Mathf.Epsilon;
        }

        Vector3 GetVoxelNormal(Vector3 point, Collider coll)
        {
            //TODO: maybe a more accurate way to get the "normals" of each voxel would be to do
            //(Collider.ClosestPoint(point) - point).normalized? Though this way we'd have to ensure the point is actually
            //inside the collider, not just near it w/ radius (if using radius/box check)
            //return (coll.ClosestPointOnBounds(point) - point).normalized;
            
            var closestPointOnBounds = coll.ClosestPointOnBounds(point);
            var centre = coll.attachedRigidbody ? coll.attachedRigidbody.worldCenterOfMass : coll.transform.position;
            var toCentre = centre - closestPointOnBounds;
            var rayOrigin = centre - (toCentre * 10f);
            if(coll.Raycast(new Ray(rayOrigin, toCentre.normalized), out var hit, Mathf.Infinity))
            {
                return hit.normal;
            }
            else
            {
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
    }
}
