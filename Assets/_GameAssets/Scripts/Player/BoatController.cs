using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform propSteeringPivot;
    [SerializeField] private Transform propUnderwaterTestPoint;
    [SerializeField] private Transform driveForceOrigin;
    [SerializeField] private Sea sea;
    [SerializeField] private float forwardSpeed = 1f;
    [SerializeField] private ForceMode driveForceMode;
    [SerializeField] private float steeringSpeed = 1f;
    [SerializeField] private float maxSteeringAngle = 90f;
    [SerializeField] private RotateObject propRotateVFX; //simple prototype VFX!

    [SerializeField] private Vector2 moveInput;

    private void OnEnable()
    {
        if(!rb)
        {
            TryGetComponent<Rigidbody>(out rb);
        }
    }

    private void Update()
    {
        SteerProp(moveInput.x);
    }

    private void FixedUpdate()
    {
        DriveProp(moveInput.y);
    }

    private void DriveProp(float forwardInput)
    {
        if(!rb || !sea || !propUnderwaterTestPoint || !driveForceOrigin)
        {
            return;
        }

        if(sea.IsPointUnderwater(propUnderwaterTestPoint.position))
        {
            rb.AddForceAtPosition(propSteeringPivot.forward * GetDriveForce(forwardInput), driveForceOrigin.position, driveForceMode);
        }
    }

    private float GetDriveForce(float forwardInput)
    {
        return forwardInput * forwardSpeed;
    }

    private void SteerProp(float steeringInput)
    {
        if(steeringInput == 0f || !propSteeringPivot)
        {
            return;
        }

        var currentY = propSteeringPivot.localEulerAngles.y;
        var newY = currentY + (steeringInput * steeringSpeed * Time.deltaTime);

        propSteeringPivot.localRotation = Quaternion.Euler(0f, newY, 0f);
    }

    private void OnDrawGizmos()
    {
        //draw prop pivot transform
        if(propSteeringPivot)
        {
            Gizmos.matrix = propSteeringPivot.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawCube(Vector3.right * 0.5f, new Vector3(1f, 0.1f, 0.1f));
            Gizmos.color = Color.green;
            Gizmos.DrawCube(Vector3.up * 0.5f, new Vector3(0.1f, 1f, 0.1f));
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(Vector3.forward * 0.5f, new Vector3(0.1f, 0.1f, 1f));
        }

        //draw forward force
        Gizmos.color = Color.yellow;
        var driveForce = GetDriveForce(moveInput.y);
        Gizmos.DrawCube(Vector3.forward * driveForce * 0.5f, new Vector3(0.1f, 0.1f, driveForce));
    }

    private void UpdatePropRotationVFX(float forwardInput)
    {
        if (!propRotateVFX)
        {
            return;
        }

        if (moveInput.y == 0f)
        {
            propRotateVFX.enabled = false;
        }
        else
        {
            propRotateVFX.enabled = true;
            propRotateVFX.SetRotationAxis((Vector3.forward * moveInput.y).normalized);
        }
    }

#region Input
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        UpdatePropRotationVFX(moveInput.y);
    }

#endregion
}
