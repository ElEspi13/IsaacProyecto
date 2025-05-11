using UnityEngine;

public class SpawnObjetos : MonoBehaviour
{
    public Transform objetosSpawnPoints; // Puntos de spawn donde se colocarán los objetos
    public int objetosRestantes; // Número de objetos restantes a generar (puedes usarlo para un contador)

    // Lista de objetos que se cargarán desde Resources
    private GameObject[] objetosLista;

    void Start()
    {
        

    }

    public void SpawnObject()
    {
        // Cargar los objetos desde la carpeta "Resources/objetos"
        objetosLista = Resources.LoadAll<GameObject>("objetos");
        if (objetosLista.Length > 0 && objetosSpawnPoints != null && objetosRestantes > 0)
        {
            // Elegir un objeto aleatorio de la lista cargada
            GameObject objetoElegido = objetosLista[Random.Range(0, objetosLista.Length)];

            // Usar la posición de objetosSpawnPoints para generar el objeto
            Vector3 spawnPosition = objetosSpawnPoints.position;

            // Instanciar el objeto en la posición deseada
            GameObject objeto = Instantiate(objetoElegido, spawnPosition, Quaternion.identity);

            // Decrementar el contador de objetos restantes
            objetosRestantes--;

            // Si quieres agregar alguna lógica adicional (por ejemplo, padres, animaciones, etc.), lo puedes hacer aquí
            // Ejemplo: asignar el objeto como hijo de objetosSpawnPoints
            objeto.transform.SetParent(objetosSpawnPoints);

            Debug.Log($"Objeto generado: {objeto.name}. Objetos restantes: {objetosRestantes}");
        }
        else
        {
            Debug.LogError("❌ No hay objetos en la lista o el contenedor objetosSpawnPoints es nulo.");
        }
    }
}
