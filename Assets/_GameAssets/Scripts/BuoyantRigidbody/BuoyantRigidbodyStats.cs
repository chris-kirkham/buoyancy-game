using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BuoyantRigidbodyStats", menuName = "Buoyant Rigidbody/Buoyant Rigidbody Stats")]
public class BuoyantRigidbodyStats : ScriptableObject
{
    [Header("Buoyancy forces")]
    [SerializeField] private float buoyancyUpMultiplier = 1f;
    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private float maxDownForce = -1f;
    [SerializeField] private float maxUpForce = 10f;
    [SerializeField] private float maxForceMagnitude = 10f;
    [Header("Water drag simulation")]
    [SerializeField] private float minLinearDamping;
    [SerializeField] private float minAngularDamping;
    [SerializeField] private float maxLinearDamping = 2f;
    [SerializeField] private float maxAngularDamping = 2f;
    [Header("Self-righting forces")]
    [SerializeField] private float selfRightingStrength;
    [SerializeField] private float selfRightActivationXAngle = 45f; //degrees either side of upright (where 0 is straigth up)
    [SerializeField] private float selfRightActivationZAngle = 45f;

    public float UpMultiplier => buoyancyUpMultiplier;
    public float ForceMultiplier => forceMultiplier;
    public float MaxDownForce => maxDownForce;
    public float MaxUpForce => maxUpForce;
    public float MaxForceMagnitude => maxForceMagnitude;
    
    public float MinLinearDamping => minLinearDamping;
    public float MinAngularDamping => minAngularDamping;
    public float MaxLinearDamping => maxLinearDamping;
    public float MaxAngularDamping => maxAngularDamping;

    public float SelfRightingStrength => SelfRightingStrength;
    public float SelfRightActivationXAngle => selfRightActivationXAngle;
    public float SelfRightActivationZAngle => selfRightActivationZAngle;

}
