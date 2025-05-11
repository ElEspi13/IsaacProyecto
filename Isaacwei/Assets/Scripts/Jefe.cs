using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

public class Jefe : EnemigoBase
{
    [Header("Movimiento")]
    public float velocidad = 3.5f;
    private float velocidadOriginal;
    public Transform jugador;
    private NavMeshAgent agente;
    private bool enKnockback = false;

    [Header("Knockback")]
    public float fuerzaKnockback = 2f;
    public float duracionKnockback = 0.3f;

    [Header("Disparo")]
    public GameObject proyectilPrefab;
    public Transform puntoDisparo;
    public float rangoDisparo = 10f;
    public float tiempoEntreDisparos = 1.5f;
    private float temporizadorDisparo;

    private void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        velocidadOriginal = velocidad;

        // Configurar NavMeshAgent
        agente.speed = velocidad;
        agente.updateRotation = false;
        agente.updateUpAxis = false;

        if (DungeonManager.Instance != null && DungeonManager.Instance.Player != null)
        {
            jugador = DungeonManager.Instance.Player;
        }
        else
        {
            Debug.LogError("? No se encontr� el jugador en el DungeonManager.");
        }

        temporizadorDisparo = 0f;
    }

    private void Update()
    {
        if (jugador == null) return;

        // Movimiento hacia el jugador si no est� en knockback
        if (!enKnockback)
        {
            agente.SetDestination(jugador.position);
        }

        // Direcci�n del movimiento para invertir el sprite
        if (agente.velocity.x > 0.5f)
            transform.localScale = new Vector3(0.4f, 0.4f, 4);
        else if (agente.velocity.x < -0.5f)
            transform.localScale = new Vector3(-0.4f, 0.4f, 4);

        // Disparo si el jugador est� en rango
        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);
        if (distanciaAlJugador <= rangoDisparo)
        {
            temporizadorDisparo -= Time.deltaTime;
            if (temporizadorDisparo <= 0f)
            {
                Disparar();
                temporizadorDisparo = tiempoEntreDisparos;
            }
        }
    }

    private void Disparar()
    {
        if (proyectilPrefab != null && puntoDisparo != null)
        {
            Vector3 direccion = (jugador.position - puntoDisparo.position).normalized;
            GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);
            proyectil.GetComponent<Rigidbody2D>().linearVelocity = direccion * 10f;
        }
        else
        {
            Debug.LogError("? Faltan el prefab o el punto de disparo.");
        }
    }

    public override void RecibirDano(float cantidad)
    {
        vida -= cantidad;
        Debug.Log("Jefe recibi� da�o: " + cantidad + ". Vida restante: " + vida);
        StartCoroutine(AplicarKnockback());

        if (vida <= 0)
        {
            Morir();
            DungeonManager.Instance.Victoria();
        }
    }

    private IEnumerator AplicarKnockback()
    {
        enKnockback = true;
        agente.isStopped = true;

        Vector3 direccionKnockback = (transform.position - jugador.position).normalized;
        agente.velocity = direccionKnockback * fuerzaKnockback;

        yield return new WaitForSeconds(duracionKnockback);

        agente.isStopped = false;
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
