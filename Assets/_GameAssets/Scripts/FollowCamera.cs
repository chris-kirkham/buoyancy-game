using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float desiredDistance;
    [SerializeField] private float desiredHeightAboveTarget;
    [SerializeField] private float positionFollowSpeed;
    [SerializeField] private float lookAtSpeed;
    [SerializeField] private float desiredLookHeightAboveTarget;

    private void LateUpdate()
    {
        if(!followTarget)
        {
            return;
        }

        //position
        var flatTargetForward = Vector3.ProjectOnPlane(followTarget.forward, Vector3.up).normalized;
        var desiredPos = followTarget.transform.position - (flatTargetForward * desiredDistance);
        desiredPos += Vector3.up * desiredHeightAboveTarget;
        transform.position = Vector3.Slerp(transform.position, desiredPos, Time.deltaTime * positionFollowSpeed);

        //look
        var lookPos = (followTarget.position + (Vector3.up * desiredLookHeightAboveTarget)) - transform.position;
        var desiredLook = Quaternion.LookRotation(lookPos, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredLook, Time.deltaTime * lookAtSpeed);
    }
}
