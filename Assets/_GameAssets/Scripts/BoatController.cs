using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform propPivot;
    [SerializeField] private float forwardSpeed = 1f;
    [SerializeField] private float steeringSpeed = 1f;
    [SerializeField] private float maxSteeringAngle = 90f;

    [SerializeField] private Vector2 moveInput;

    private void Update()
    {
        SteerProp(moveInput.x * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        DriveProp(moveInput.y);
    }

    private void DriveProp(float forwardInput)
    {
        //TODO: only add force if prop is underwater
        rb.AddForceAtPosition(propPivot.forward * GetDriveForce(forwardInput), propPivot.position);
    }

    private float GetDriveForce(float forwardInput)
    {
        return forwardInput * forwardSpeed;
    }

    private void SteerProp(float steeringInput)
    {
        var currentY = propPivot.localEulerAngles.y;
        var newY = Mathf.Clamp(currentY + (steeringInput * steeringSpeed), -maxSteeringAngle, maxSteeringAngle);
        propPivot.localRotation = Quaternion.Euler(0f, newY, 0f);
    }


    private void OnDrawGizmos()
    {
        //draw prop pivot transform
        Gizmos.matrix = propPivot.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawCube(Vector3.right * 0.5f, new Vector3(1f, 0.1f, 0.1f));
        Gizmos.color = Color.green;
        Gizmos.DrawCube(Vector3.up * 0.5f, new Vector3(0.1f, 1f, 0.1f));
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(Vector3.forward * 0.5f, new Vector3(0.1f, 0.1f, 1f));

        //draw forward force
        Gizmos.color = Color.yellow;
        var driveForce = GetDriveForce(moveInput.y);
        Gizmos.DrawCube(Vector3.forward * driveForce * 0.5f, new Vector3(0.1f, 0.1f, driveForce));
    }

#region Input
    private void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

#endregion
}
