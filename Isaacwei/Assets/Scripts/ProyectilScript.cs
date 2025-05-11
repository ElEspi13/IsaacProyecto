using NUnit.Framework;
using UnityEngine;

public class ProyectilScript : MonoBehaviour
{
    public float velocidad = 5f;
    public float distanciaMaxima = 10f;
    public Vector2 DireccionDisparo;
    public float damage;

    private float distanciaRecorrida;

    void Start()
    {
        EstadisticasJugador estadisticas = Object.FindAnyObjectByType<EstadisticasJugador>();

        if (estadisticas != null)
        {
            damage = estadisticas.damage;
        }
    }

    void Update()
    {
        transform.Translate(DireccionDisparo * velocidad * Time.deltaTime);
        distanciaRecorrida += velocidad * Time.deltaTime;

        if (distanciaRecorrida >= distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si el objeto tiene el componente EnemigoBase
        EnemigoBase enemigo = other.GetComponent<EnemigoBase>();

        // Si el objeto tiene el componente EnemigoBase (es un enemigo)
        if (enemigo != null)
        {
            enemigo.RecibirDano(damage); // Aplicar daño al enemigo
            Destroy(gameObject); // Destruir el proyectil después de golpear
        }
        else if (other.CompareTag("Muro"))
        {
            Destroy(gameObject); // Destruir el proyectil al golpear un muro
        }
    }
}

