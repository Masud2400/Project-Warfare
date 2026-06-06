using System.Collections;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject modelToSpawn; 
    [SerializeField] private Transform[] spawnPoints; 
    [SerializeField] private float respawnDelay = 3.0f;

    void Start()
    {
        foreach (Transform t in spawnPoints)
        {
            SpawnModelAtPoint(t);
        }
    }

    private void SpawnModelAtPoint(Transform t)
    {
        GameObject spawned = Instantiate(modelToSpawn, t.position, t.rotation);

		Enemy enemy = spawned.GetComponent<Enemy>();
		if (enemy != null)
		{
			enemy.SetupSpawner(this, t);
		}
    }

    public void TrackRespawn(Transform t)
    {
        StartCoroutine(RespawnRoutine(t));
    }

    private IEnumerator RespawnRoutine(Transform t)
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnModelAtPoint(t);
    }
}
