using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance; // Singleton para acceso global

    public event Action OnSalaLimpia; // Evento que se dispara cuando una sala está limpia

    private Dictionary<Vector2Int, GameObject> instantiatedRooms; // Referencia a las salas generadas
    private GameObject player; // Referencia al jugador
    public Transform Player { get; private set; }
    private int enemigosRestantes; // Número de enemigos restantes en la sala actual
    private Vector2Int salaActual; // Posición de la sala actual

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
    }

    public void MoverJugadorADireccion(ROOM_DIRECTIONS direccion)
    {
        if (instantiatedRooms == null || instantiatedRooms.Count == 0)
        {
            Debug.LogError("❌ No hay salas instanciadas en el DungeonManager.");
            return;
        }

        // Calcular la posición de la sala conectada
        Vector2Int nuevaSala = salaActual + DireccionARelativa(direccion);

        if (instantiatedRooms.ContainsKey(nuevaSala))
        {
            // Desactivar la sala actual
            if (instantiatedRooms.ContainsKey(salaActual) && instantiatedRooms[salaActual] != null)
            {
                instantiatedRooms[salaActual].SetActive(false);
                Debug.Log($"🔴 Sala desactivada: {salaActual}");
            }

            // Activar la sala conectada
            GameObject salaConectada = instantiatedRooms[nuevaSala];
            if (salaConectada != null)
            {
                salaConectada.SetActive(true);
                Debug.Log($"🟢 Sala activada: {nuevaSala}");

                // Mover al jugador frente a la puerta opuesta
                if (player != null)
                {
                    Transform puertaOpuesta = ObtenerPuertaOpuesta(salaConectada, direccion);
                    if (puertaOpuesta != null)
                    {
                        // Calcular la posición frente a la puerta opuesta
                        Vector3 posicionFrentePuerta = CalcularPosicionFrentePuerta(puertaOpuesta, direccion);
                        player.transform.position = posicionFrentePuerta;
                        Debug.Log($"🚶 Jugador movido a: {posicionFrentePuerta}");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ No se encontró la puerta opuesta en la nueva sala.");
                    }
                }

                // Mover la cámara al centro de la nueva sala
                MoverCamaraAlCentro(salaConectada);

                // Actualizar la sala actual
                salaActual = nuevaSala;
            }
            else
            {
                Debug.LogError($"❌ La sala en la posición {nuevaSala} es nula.");
            }
        }
        else
        {
            Debug.LogError($"❌ No se encontró la sala en la posición {nuevaSala}");
        }
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
        // Verificar si la sala actual está limpia de enemigos
        return enemigosRestantes == 0;
    }

    public void EliminarEnemigo()
    {
        // Reducir el número de enemigos restantes
        enemigosRestantes--;

        if (enemigosRestantes <= 0)
        {
            // Disparar el evento de sala limpia
            OnSalaLimpia?.Invoke();
        }
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
}
