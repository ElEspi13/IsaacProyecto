using UnityEngine;
using UnityEngine.AI;

public class SpawnEnemigos : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab del enemigo a generar
    public Transform enemigosSpawnPoints; // Contenedor para los enemigos de la sala

    public void SpawnEnemy()
    {
        if (enemyPrefab != null && enemigosSpawnPoints != null)
        {
            // Usar la posición de enemigosSpawnPoints para generar al enemigo
            Vector3 spawnPosition = enemigosSpawnPoints.position;

            // Verificar si la posición está en el NavMesh
            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
            {
                // Instanciar el enemigo en la posición válida del NavMesh
                GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);

                // Asegurarse de que el enemigo tenga un NavMeshAgent
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent == null)
                {
                    Debug.LogError($"❌ El prefab del enemigo {enemy.name} no tiene un componente NavMeshAgent.");
                }

                // Introducir al enemigo como hijo del contenedor enemigosSpawnPoints
                enemy.transform.SetParent(enemigosSpawnPoints);
            }
            else
            {
                Debug.LogError($"❌ No se encontró una posición válida en el NavMesh para el punto de spawn: {spawnPosition}");
            }
        }
        else
        {
            Debug.LogError("❌ No se asignó un prefab de enemigo o el contenedor enemigosSpawnPoints es nulo.");
        }
    }
}
