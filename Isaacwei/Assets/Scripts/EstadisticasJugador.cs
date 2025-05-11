using System;
using System.Collections;
using UnityEngine;
using static ObjetoControlador;

public class EstadisticasJugador : MonoBehaviour
{
    public static EstadisticasJugador instancia; // Singleton

    [Header("Movimiento")]
    public float velocidad = 10f;
    public float suavizado = 0.5f;

    [Header("Disparo")]
    public float cooldownDisparo = 0.2f;
    public float velocidadDisparo = 5f;
    public float distanciaMaximaDisparo = 10f;
    public float damage = 3.5f;

    [Header("Tamaño del proyectil")]
    public Vector3 escalaProyectil = Vector3.one;

    private Vector3 escalaInicial;
    private float velocidadInicial;
    private float cooldownInicial;
    private float velocidadDisparoInicial;
    private float distanciaMaximaInicial;
    private float damageInicial;

    [Header("Vida")]
    public float vida=3f;
    public float tiempoEntreDanos = 0.5f;
    private float tiempoSiguienteDano = 0f;
    public SpriteRenderer spriteRenderer;
    public float duracionParpadeo = 0.5f; // Duración total del parpadeo
    public float intervaloParpadeo = 0.2f;

    private float vidaInicial;
    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // No se destruye al cambiar de escena
        }
        else
        {
            Destroy(gameObject); // Evita duplicados
            return;
        }

        GuardarValoresIniciales();
    }

    private void GuardarValoresIniciales()
    {
        velocidadInicial = velocidad;
        cooldownInicial = cooldownDisparo;
        velocidadDisparoInicial = velocidadDisparo;
        distanciaMaximaInicial = distanciaMaximaDisparo;
        damageInicial = damage;
        vidaInicial = vida;
        escalaInicial = escalaProyectil; 
    }

    public void RestaurarValoresIniciales()
    {
        velocidad = velocidadInicial;
        cooldownDisparo = cooldownInicial;
        velocidadDisparo = velocidadDisparoInicial;
        distanciaMaximaDisparo = distanciaMaximaInicial;
        damage = damageInicial;
        vida = vidaInicial;
        escalaProyectil = escalaInicial; 
    }

    public void PerderVida(float v)
    {
        if (Time.time < tiempoSiguienteDano)
            return; // Todavía no puede recibir daño

        vida -= v;
        tiempoSiguienteDano = Time.time + tiempoEntreDanos;

        Debug.Log($"💥 Daño recibido, Vida restante: {vida}");
        StartCoroutine(Parpadear());

        if (vida <= 0)
        {
            DungeonManager.Instance.GameOver();
        }
    }
    private IEnumerator Parpadear()
    {
        float tiempoPasado = 0f;

        while (tiempoPasado < duracionParpadeo)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(intervaloParpadeo);
            tiempoPasado += intervaloParpadeo;
        }

        // Asegurar que el sprite se quede visible al final
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    public void AplicarModificador(ObjetoControlador.TipoEfecto tipo, ObjetoControlador.TipoOperacion operacion, float cantidad)
    {
        float AplicarOperacion(float valorBase)
        {
            return operacion switch
            {
                ObjetoControlador.TipoOperacion.Sumar => valorBase + cantidad,
                ObjetoControlador.TipoOperacion.Restar => valorBase - cantidad,
                ObjetoControlador.TipoOperacion.Multiplicar => valorBase * cantidad,
                ObjetoControlador.TipoOperacion.Dividir => valorBase / cantidad,
                _ => valorBase
            };
        }

        switch (tipo)
        {
            case ObjetoControlador.TipoEfecto.Vida:
                vida = AplicarOperacion(vida);
                break;
            case ObjetoControlador.TipoEfecto.Dano:
                damage = AplicarOperacion(damage);
                break;
            case ObjetoControlador.TipoEfecto.VelocidadAtaque:
                cooldownDisparo = AplicarOperacion(cooldownDisparo);
                break;
            case ObjetoControlador.TipoEfecto.VelocidadDisparo:
                velocidadDisparo = AplicarOperacion(velocidadDisparo);
                break;
            case ObjetoControlador.TipoEfecto.Tamano:
                if (operacion == ObjetoControlador.TipoOperacion.Multiplicar || operacion == ObjetoControlador.TipoOperacion.Dividir)
                {
                    escalaProyectil = escalaProyectil * cantidad;
                }
                else
                {
                    escalaProyectil += Vector3.one * cantidad;
                }
                break;
        }

        Debug.Log($"✅ Aplicado: {tipo} con {operacion} {cantidad}");
    }

}
