using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform boatSpawnParent;
    [SerializeField] private FollowCamera followCam;
    [SerializeField] private BoatData boatData;
    [SerializeField] private BoatControllerInputBase input;

    private BoatController spawnedBoat;

    public void SpawnBoat()
    {
        if(!boatData)
        {
            Debug.LogError($"No {nameof(BoatData)} set!");
            return;
        }

        spawnedBoat = Instantiate<BoatController>(boatData.BoatPrefab, boatSpawnParent);
        spawnedBoat.SetInput(input);

        if(followCam)
        {
            followCam.SetStats(spawnedBoat.CamStats);
            followCam.SetFollowTarget(spawnedBoat.CamFollowTarget);
        }
    }

    public void SetBoatData(BoatData boatData)
    {
        this.boatData = boatData;
    }
}
