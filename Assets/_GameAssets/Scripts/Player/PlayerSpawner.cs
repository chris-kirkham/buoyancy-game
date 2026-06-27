using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private int team; //TODO: make a TeamPlayerSpawner inherit from this?
    [SerializeField] private PlayerSpawnData spawnData;

    [ContextMenu("Spawn player")]
    public void Spawn()
    {
        if(!spawnData)
        {
            Debug.Log($"No {nameof(PlayerSpawnData)} set!");
            return;
        }

        //TODO: spawn player prefab
        var player = Instantiate<Player>(spawnData.PlayerPrefab, transform.position, transform.rotation);
        player.SetBoatData(spawnData.BoatData);
        player.SpawnBoat();
    }

    private void OnDrawGizmos()
    {
        //draw spawn point
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(Vector3.zero, 0.5f);
        Gizmos.DrawRay(Vector3.zero, Vector3.up * 2f);
        Gizmos.DrawRay(Vector3.left, Vector3.right * 2f);
        Gizmos.DrawRay(Vector3.zero, Vector3.forward);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
