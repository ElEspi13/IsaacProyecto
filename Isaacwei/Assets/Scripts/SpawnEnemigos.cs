using UnityEngine;
using UnityEngine.AI;

public class SpawnEnemigos : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab del enemigo a generar

    public void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            // Verificar si la posición está en el NavMesh
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
            {
                // Instanciar el enemigo en la posición válida del NavMesh
                GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);

                // Asegurarse de que el enemigo tenga un NavMeshAgent
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent == null)
                {
                    Debug.LogError($"❌ El prefab del enemigo {enemy.name} no tiene un componente NavMeshAgent.");
                }
            }
            else
            {
                Debug.LogError($"❌ No se encontró una posición válida en el NavMesh para el punto de spawn: {transform.position}");
            }
        }
    }
}
