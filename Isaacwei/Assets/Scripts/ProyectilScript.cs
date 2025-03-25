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
        if (other.CompareTag("Enemigo"))
        {
            Enemigo enemigo = other.GetComponent<Enemigo>();
            if (enemigo != null)
            {
                enemigo.RecibirDaño(damage);
            }
            Destroy(gameObject);
        }
    }
}

