using UnityEngine;
using UnityEngine.AI; // Importar IA de navegación
using System.Collections;

public class Enemigo : EnemigoBase
{
    [Header("Estadísticas del Enemigo")]
    public float velocidad = 3.5f;
    private float velocidadOriginal;

    [Header("Movimiento")]
    public Transform jugador;
    private NavMeshAgent agente; // NavMeshAgent para el movimiento

    [Header("Knockback")]
    public float fuerzaKnockback = 2f;
    public float duracionKnockback = 0.3f;

    private bool enKnockback = false;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>(); // Obtener el NavMeshAgent
        velocidadOriginal = velocidad;

        // Configurar NavMeshAgent
        agente.speed = velocidad;
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
    }

    void Update()
    {
        if (enKnockback) return; // Si está en knockback, no moverse normalmente

        // Seguir al jugador
        if (jugador != null)
        {
            agente.SetDestination(jugador.position);
        }

        // 🔄 Invertir sprite según dirección
        if (agente.velocity.x > 0.5)
        {
            transform.localScale = new Vector3(3, 3, 4);
        }
        else if (agente.velocity.x < 0.5)
        {
            transform.localScale = new Vector3(-3, 3, 4);
        }
    }

    public override void RecibirDano(float cantidad)
    {
        vida -= cantidad;
        Debug.Log("Enemigo recibió daño: " + cantidad + ". Vida restante: " + vida);

        // Aplicar knockback y ralentización
        StartCoroutine(AplicarKnockback());

        if (vida <= 0)
        {
            Morir();
        }
    }

    private IEnumerator AplicarKnockback()
    {
        enKnockback = true; // Detener movimiento normal
        agente.isStopped = true; // Pausar el NavMeshAgent

        // Calcular dirección de retroceso
        Vector3 direccionKnockback = (transform.position - jugador.position).normalized;
        agente.velocity = direccionKnockback * fuerzaKnockback; // Aplicar knockback

        yield return new WaitForSeconds(duracionKnockback); // Esperar el knockback

        agente.isStopped = false; // Reactivar movimiento
        enKnockback = false;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EstadisticasJugador.instancia.PerderVida(0.5f);

        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EstadisticasJugador.instancia.PerderVida(0.5f);

        }
    }


}
