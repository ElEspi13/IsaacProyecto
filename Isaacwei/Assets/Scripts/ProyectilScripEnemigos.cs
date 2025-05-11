using NUnit.Framework;
using UnityEngine;

public class ProyectilScriptEnemigos : MonoBehaviour
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
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            EstadisticasJugador.instancia.PerderVida(0.5f);
        }
        else if(other.CompareTag("Muro"))
        {
            Destroy(gameObject);
        }
        
    }
}

