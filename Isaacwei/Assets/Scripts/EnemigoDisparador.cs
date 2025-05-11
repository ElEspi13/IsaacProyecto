using UnityEngine;
using UnityEngine.AI;

public class EnemigoDisparador : EnemigoBase {


    [Header("Disparo")]
    public GameObject proyectilPrefab; // Prefab del proyectil
    public Transform puntoDisparo; // Punto desde donde se dispara
    public float rangoDisparo = 10f; // Rango en el que el enemigo puede disparar
    public float tiempoEntreDisparos = 1.5f; // Tiempo entre disparos
    private float temporizadorDisparo;

    [Header("Jugador")]
    public Transform jugador;

    private NavMeshAgent agente;
    private void Start()
    {

        agente = GetComponent<NavMeshAgent>(); // Obtener el NavMeshAgent

        // Configurar NavMeshAgent
        agente.speed = 0; // No moverse
        agente.updateRotation = false; // No girar automáticamente
        agente.updateUpAxis = false; // Mantener eje Z fijo (para 2D)
        if (DungeonManager.Instance != null && DungeonManager.Instance.Player != null)
        {
            jugador = DungeonManager.Instance.Player;
        }
        else
        {
            Debug.LogError("❌ No se encontró el jugador en el DungeonManager.");
        }

        temporizadorDisparo = 0f; // Inicializar el temporizador
    }

    private void Update()
    {
        if (jugador == null) return;

        // Calcular la distancia al jugador
        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);

        // Si el jugador está dentro del rango de disparo
        if (distanciaAlJugador <= rangoDisparo)
        {
            // Mirar hacia el jugador (solo rotación en el eje X)
            Vector3 direccion = jugador.position - transform.position;

            // Si el jugador está a la derecha del enemigo
            if (direccion.x > 0)
            {
                transform.localScale = new Vector3((float)-0.4, (float)0.4, 4); // Míralo a la derecha
            }
            // Si el jugador está a la izquierda del enemigo
            else
            {
                transform.localScale = new Vector3((float)-0.4, (float)0.4, 4); // Míralo a la izquierda
            }

            // Disparar si el temporizador lo permite
            temporizadorDisparo -= Time.deltaTime;
            if (temporizadorDisparo <= 0f)
            {
                Disparar();
                Debug.Log("💥 Disparando proyectil...");
                temporizadorDisparo = tiempoEntreDisparos; // Reiniciar el temporizador
            }
        }
    }

    private void Disparar()
    {
        if (proyectilPrefab != null && puntoDisparo != null)
        {
            // Instanciar el proyectil
            GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);

            // Configurar la dirección del proyectil hacia el jugador
            Vector3 direccion = (jugador.position - puntoDisparo.position).normalized;
            proyectil.GetComponent<Rigidbody2D>().linearVelocity = direccion * 10f; // Ajusta la velocidad del proyectil

            Debug.Log("💥 Enemigo disparó un proyectil.");
        }
        else
        {
            Debug.LogError("❌ No se asignó un prefab de proyectil o un punto de disparo.");
        }
    }

    
}