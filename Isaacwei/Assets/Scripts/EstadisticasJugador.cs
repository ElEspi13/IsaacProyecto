using UnityEngine;

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

    private float velocidadInicial;
    private float cooldownInicial;
    private float velocidadDisparoInicial;
    private float distanciaMaximaInicial;
    private float damageInicial;

    [Header("Vida")]
    public float vida=3f;

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
    }

    public void RestaurarValoresIniciales()
    {
        velocidad = velocidadInicial;
        cooldownDisparo = cooldownInicial;
        velocidadDisparo = velocidadDisparoInicial;
        distanciaMaximaDisparo = distanciaMaximaInicial;
        damage = damageInicial;
        vida = vidaInicial;
    }
}
