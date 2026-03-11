using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis;
    [SerializeField] private float speed; 
    
    private void LateUpdate()
    {
        transform.Rotate(rotationAxis * speed * Time.deltaTime);
    }

    public void SetRotationAxis(Vector3 axis)
    {
        rotationAxis = axis;
    }
}
