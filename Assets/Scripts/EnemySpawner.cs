using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Opcional E: generador temporizado de enemigos (drones con NavMeshAgent)
public class EnemySpawner : MonoBehaviour
{
    // Prefab del dron: se arrastra desde el Project en el Inspector
    public GameObject enemyPrefab;

    private float startDelay;   // segundos antes del primer dron
    private float interval;     // segundos entre drones
    private int maxAlive;       // tope de drones vivos a la vez
    private List<GameObject> spawned;

    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: falta asignar el prefab del dron.");
            return;
        }

        startDelay = 5f;
        interval = 10f;
        maxAlive = 3;
        spawned = new List<GameObject>();

        InvokeRepeating(nameof(SpawnEnemy), startDelay, interval);
    }

    private void SpawnEnemy()
    {
        // Limpia de la lista los drones que ya no existen
        spawned.RemoveAll(enemy => enemy == null);
        if (spawned.Count >= maxAlive) return;

        // El dron tiene que nacer sobre el NavMesh: busca el punto caminable más cercano
        Vector3 position = transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 3f, NavMesh.AllAreas))
            position = hit.position;

        GameObject enemy = Instantiate(enemyPrefab, position, transform.rotation);
        spawned.Add(enemy);
    }
}