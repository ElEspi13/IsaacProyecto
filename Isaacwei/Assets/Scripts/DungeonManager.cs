using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance; // Singleton para acceso global
    public event Action OnSalaLimpia;

    private Dictionary<Vector2Int, GameObject> instantiatedRooms; // Referencia a las salas generadas
    private GameObject player; // Referencia al jugador
    public Transform Player { get; private set; }
    private int enemigosRestantes=0; // Número de enemigos restantes en la sala actual
    private Vector2Int salaActual; // Posición de la sala actual

    public GameObject gameOverCanvas; // Canvas de Game Over
    public GameObject victoryCanvas; // Canvas de Victoria
    private void Awake()
    {
        // Configurar el Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Dictionary<Vector2Int, GameObject> rooms, GameObject playerObject)
    {
        instantiatedRooms = rooms;
        player = playerObject;
        Player= playerObject.transform;

        DesactivarTodasLasSalasMenosInicial();
    }

    public void DesactivarTodasLasSalasMenosInicial()
    {
        if (instantiatedRooms == null || instantiatedRooms.Count == 0)
        {
            Debug.LogError("❌ No hay salas instanciadas en el DungeonManager.");
            return;
        }

        Vector2Int salaInicial = new Vector2Int(0, 0); // Posición de la sala inicial

        foreach (var sala in instantiatedRooms)
        {
            if (sala.Key == salaInicial)
            {
                sala.Value.SetActive(true); // Activar la sala inicial
                Debug.Log($"🟢 Sala inicial activada: {sala.Key}");
            }
            else
            {
                sala.Value.SetActive(false); // Desactivar las demás salas
                Debug.Log($"🔴 Sala desactivada: {sala.Key}");
            }
        }

        // Configurar la sala inicial como la sala actual
        salaActual = salaInicial;
    }

    public void MoverJugadorADireccion(ROOM_DIRECTIONS direccion)
    {
        StartCoroutine(CambiarDeSalaConPausa(direccion));
    }
    private IEnumerator CambiarDeSalaConPausa(ROOM_DIRECTIONS direccion)
    {
        if (instantiatedRooms == null || instantiatedRooms.Count == 0)
        {
            Debug.LogError("❌ No hay salas instanciadas.");
            yield break;
        }

        Vector2Int nuevaSala = salaActual + DireccionARelativa(direccion);

        if (!instantiatedRooms.ContainsKey(nuevaSala))
        {
            Debug.LogError($"❌ No se encontró la sala en la posición {nuevaSala}");
            yield break;
        }

        // Desactivar sala actual
        if (instantiatedRooms.ContainsKey(salaActual) && instantiatedRooms[salaActual] != null)
        {
            instantiatedRooms[salaActual].SetActive(false);
            Debug.Log($"🔴 Sala desactivada: {salaActual}");
        }

        GameObject salaConectada = instantiatedRooms[nuevaSala];
        if (salaConectada == null)
        {
            Debug.LogError($"❌ La sala en la posición {nuevaSala} es nula.");
            yield break;
        }

        salaConectada.SetActive(true);
        Debug.Log($"🟢 Sala activada: {nuevaSala}");

        if (player != null)
        {
            Transform puertaOpuesta = ObtenerPuertaOpuesta(salaConectada, direccion);
            if (puertaOpuesta != null)
            {
                Vector3 posicionFrentePuerta = CalcularPosicionFrentePuerta(puertaOpuesta, direccion);
                player.transform.position = posicionFrentePuerta;
                Debug.Log($"🚶 Jugador movido a: {posicionFrentePuerta}");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró la puerta opuesta en la nueva sala.");
            }
        }

        MoverCamaraAlCentro(salaConectada);
        salaActual = nuevaSala;

        // 🔸 Pausa del juego durante 0.6 segundos de tiempo real
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 1f;
    }

    private Vector3 CalcularPosicionFrentePuerta(Transform puerta, ROOM_DIRECTIONS entrada)
    {
        // Distancia para posicionar al jugador dentro de la sala conectada
        float desplazamientoX = 2.0f; // Desplazamiento en el eje X
        float desplazamientoY = 2.0f; // Desplazamiento en el eje Y

        // Calcular la posición frente a la puerta según la dirección de entrada
        return entrada switch
        {
            ROOM_DIRECTIONS.Up => puerta.position + new Vector3(0, desplazamientoY, 0), // Dentro de la sala superior
            ROOM_DIRECTIONS.Down => puerta.position + new Vector3(0, -desplazamientoY, 0), // Dentro de la sala inferior
            ROOM_DIRECTIONS.Left => puerta.position + new Vector3(-desplazamientoX, 0, 0), // Dentro de la sala izquierda
            ROOM_DIRECTIONS.Right => puerta.position + new Vector3(desplazamientoX, 0, 0), // Dentro de la sala derecha
            _ => puerta.position // Por defecto, la posición de la puerta
        };
    }
    private Transform ObtenerPuertaOpuesta(GameObject sala, ROOM_DIRECTIONS entrada)
    {
        // Determinar el nombre de la puerta opuesta
        string nombrePuertaOpuesta = entrada switch
        {
            ROOM_DIRECTIONS.Up => "PuertaAbajo",
            ROOM_DIRECTIONS.Down => "PuertaArriba",
            ROOM_DIRECTIONS.Left => "PuertaDerecha",
            ROOM_DIRECTIONS.Right => "PuertaIzquierda",
            _ => null
        };

        if (nombrePuertaOpuesta != null)
        {
            // Buscar el GameObject "Puertas" dentro de la sala
            Transform contenedorPuertas = sala.transform.Find("Puertas");
            if (contenedorPuertas != null)
            {
                // Buscar la puerta dentro del contenedor "Puertas"
                Transform puerta = contenedorPuertas.Find(nombrePuertaOpuesta);
                if (puerta != null)
                {
                    Debug.Log($"✅ Puerta opuesta encontrada: {puerta.name} en la sala {sala.name}");
                    return puerta;
                }
            }
        }

        Debug.LogWarning($"⚠️ No se encontró la puerta opuesta: {nombrePuertaOpuesta} en la sala {sala.name}");
        return null;
    }
    private Vector2Int DireccionARelativa(ROOM_DIRECTIONS direccion)
    {
        return direccion switch
        {
            ROOM_DIRECTIONS.Up => Vector2Int.up,
            ROOM_DIRECTIONS.Down => Vector2Int.down,
            ROOM_DIRECTIONS.Left => Vector2Int.left,
            ROOM_DIRECTIONS.Right => Vector2Int.right,
            _ => throw new ArgumentException("Dirección no válida")
        };
    }

    public bool SalaActualLimpia()
    {
        if (!instantiatedRooms.ContainsKey(salaActual))
        {
            Debug.LogWarning($"No se encontró una sala en la posición {salaActual}");
            return true;
        }

        GameObject salaGO = instantiatedRooms[salaActual];
        Transform suelo = salaGO.transform.Find("Suelo");

        if (suelo == null)
        {
            Debug.LogWarning("No se encontró el objeto 'suelo' en la sala.");
            return true;
        }

        int enemigosActivos = 0;

        foreach (Transform spawnPoint in suelo)
        {
            foreach (Transform posibleEnemigo in spawnPoint)
            {
                if (posibleEnemigo.CompareTag("Enemigo") && posibleEnemigo.gameObject.activeSelf)
                {
                    enemigosActivos++;
                    Debug.Log($"🛑 Enemigo activo: {posibleEnemigo.name}");
                }
            }
        }

        enemigosRestantes = enemigosActivos; // actualiza el contador con el número real

        return enemigosActivos == 0;
        
    }


    public void EliminarEnemigo()
    {
        // Reducir el número de enemigos restantes
        enemigosRestantes--;
        Debug.Log($"Cheviene:{enemigosRestantes}");

        if (enemigosRestantes <= 0)
        {
            // Disparar el evento de sala limpia
            OnSalaLimpia?.Invoke();

            Debug.Log($"No enemigos quedan");
        }
        Debug.Log($"Enemigos:{enemigosRestantes}");


    }

    public void ConfigurarSalaActual(Vector2Int sala, int enemigos)
    {
        // Configurar la sala actual y el número de enemigos
        salaActual = sala;
        enemigosRestantes = enemigos;
    }
    private void MoverCamaraAlCentro(GameObject sala)
    {
        Camera mainCamera = Camera.main; // Obtener la cámara principal
        if (mainCamera != null)
        {
            // Mover la cámara al centro de la sala
            mainCamera.transform.position = new Vector3(
                sala.transform.position.x,
                sala.transform.position.y,
                mainCamera.transform.position.z // Mantener la posición Z de la cámara
            );
            Debug.Log($"📷 Cámara movida al centro de la sala: {sala.transform.position}");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró la cámara principal.");
        }
    }

    internal void GameOver()
    {
        Time.timeScale = 0f;

        Debug.Log("Game Over");

        // Mostrar el Canvas de Game Over
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true); // Activar el Canvas de Game Over
        }
        player.transform.position = new Vector3(0, 0, 0); // Reiniciar la posición del jugador

        


    }

    internal void Victoria()
    {
        Time.timeScale = 0f;
        // Mostrar el Canvas de Game Over
        if (victoryCanvas != null)
        {
            victoryCanvas.SetActive(true); // Activar el Canvas de Game Over
        }
        player.transform.position = new Vector3(0, 0, 0); // Reiniciar la posición del jugador


    }
}
