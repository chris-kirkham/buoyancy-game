using UnityEngine;

//simple follow camera script
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform followTarget;
    [SerializeField] private FollowCameraStats stats;

    private void LateUpdate()
    {
        if(!followTarget)
        {
            return;
        }

        //position
        var flatTargetForward = Vector3.ProjectOnPlane(followTarget.forward, Vector3.up).normalized;
        var desiredPos = followTarget.transform.position - (flatTargetForward * stats.DesiredDistance);
        desiredPos += Vector3.up * stats.DesiredHeightAboveTarget;
        transform.position = Vector3.Slerp(transform.position, desiredPos, Time.deltaTime * stats.PositionFollowSpeed);

        //look
        var desiredLookPos = followTarget.position + followTarget.transform.TransformDirection(stats.TargetLookOffset_LS);
        var desiredLookRotation = Quaternion.LookRotation(desiredLookPos - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredLookRotation, Time.deltaTime * stats.LookAtSpeed);
    }

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    public void SetStats(FollowCameraStats stats)
    {
        if(stats)
        {
            this.stats = stats;
        }
        else
        {
            Debug.LogError($"Tried to set null {nameof(FollowCameraStats)}! Stats will not be changed.");
        }
    }
}
