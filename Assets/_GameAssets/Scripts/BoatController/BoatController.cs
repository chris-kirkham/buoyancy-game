using UnityEngine;
using UnityEngine.InputSystem;
public class BoatController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform propSteeringPivot;
    [SerializeField] private Transform propUnderwaterTestPoint;
    [SerializeField] private Transform driveForceOrigin;
    [SerializeField] private Sea sea;

    [Header("Boat stats")]
    [SerializeField] private float forwardSpeed = 1f;
    [SerializeField] private float propAccelTime = 0.5f;
    [SerializeField] private AnimationCurve propAccelCurve;
    [SerializeField] private ForceMode driveForceMode;
    [SerializeField] private float steeringReturnToCentreSpeed = 1f;
    [SerializeField] private float steeringSpeed = 1f;
    [SerializeField] private float maxSteeringAngle = 90f;
    [SerializeField] private RotateObject propRotateVFX; //simple prototype VFX!

    [SerializeField] private FollowCameraStats cameraStats; //camera stats for this boat

    [Header("Input")]
    [SerializeField] private BoatControllerInputBase input;

    public FollowCameraStats CamStats => cameraStats;
    public Transform CamFollowTarget => rb.transform;

    private float fwdInputTime; //time the

    //cached input
    private Vector2 moveInput;

    private void OnEnable()
    {
        if(!rb)
        {
            TryGetComponent<Rigidbody>(out rb);
        }

        if(!sea)
        {
            var seas = FindObjectsByType<Sea>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
            if(seas.Length == 1)
            {
                sea = seas[0];
            }
            else if(seas.Length > 1)
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

    private void Update()
    {
        if(input)
        {
            moveInput = input.GetMoveInput();
        }

        SteerProp(moveInput.x, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        DriveProp(moveInput.y);
        UpdatePropRotationVFX(moveInput.y);
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

    private void SteerProp(float steeringInput, float deltaTime)
    {
        if(steeringInput == 0f || !propSteeringPivot)
        {
            ReturnSteeringToCentre(deltaTime);
            return;
        }

        var newY = GetSteeringAngle_KBM(steeringInput, deltaTime);
        
        if(newY > maxSteeringAngle)
        {
            if(newY < 180f)
            {
                newY = maxSteeringAngle;
            }
            else if (newY < 360f - maxSteeringAngle)
            {
                newY = 180f + maxSteeringAngle;
            }
        }

        propSteeringPivot.localRotation = Quaternion.Euler(0f, newY, 0f);
    }

    private float GetSteeringAngle_KBM(float steeringInput, float deltaTime)
    {
        var currentY = propSteeringPivot.localEulerAngles.y;
        var newY = currentY + (steeringInput * steeringSpeed * deltaTime);

        return newY;
    }

    private float GetSteeringAngle_Gamepad(float steeringInput, float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    private void ReturnSteeringToCentre(float deltaTime)
    {
        propSteeringPivot.localRotation = Quaternion.Lerp(propSteeringPivot.localRotation, Quaternion.identity, deltaTime * steeringReturnToCentreSpeed);
    }

    private void UpdatePropRotationVFX(float forwardInput)
    {
        if (!propRotateVFX)
        {
            return;
        }

        if (forwardInput == 0f)
        {
            propRotateVFX.enabled = false;
        }
        else
        {
            propRotateVFX.enabled = true;
            propRotateVFX.SetRotationAxis((Vector3.forward * forwardInput).normalized);
        }
    }

    public void SetInput(BoatControllerInputBase input)
    {
        if(input)
        {
            this.input = input;
        }
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
}
