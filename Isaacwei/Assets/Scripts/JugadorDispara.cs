using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public EstadisticasJugador estadisticas; // Referencia compartida
    public GameObject prefabDisparo;
    public GameObject puntoPartida;
    private float tiempoUltimoDisparo = 0f;
    public Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (estadisticas == null)
            estadisticas = Object.FindAnyObjectByType<EstadisticasJugador>(); // Autoasignar si está en escena
    }

    void Update()
    {
        float inputHorizontal2 = Input.GetAxis("Horizontal");
        float inputVertical2 = Input.GetAxis("Vertical");

        if (Time.time - tiempoUltimoDisparo >= estadisticas.cooldownDisparo)
        {
            float inputHorizontal = Input.GetAxis("HorizontalButton");
            float inputVertical = Input.GetAxis("VerticalButton");

            if (Mathf.Abs(inputHorizontal) > 0.1f || Mathf.Abs(inputVertical) > 0.1f)
            {
                Vector2 direccionDisparo = new Vector2(inputHorizontal, inputVertical).normalized;

                if (puntoPartida != null && prefabDisparo != null)
                {
                    GameObject disparo = Instantiate(prefabDisparo, puntoPartida.transform.position, Quaternion.identity);

                    // 👇 Aplicar tamaño desde estadísticas
                    disparo.transform.localScale = estadisticas.escalaProyectil;

                    ProyectilScript movimientoDisparo = disparo.GetComponent<ProyectilScript>();

                    if (movimientoDisparo != null)
                    {
                        movimientoDisparo.DireccionDisparo = direccionDisparo;
                        movimientoDisparo.velocidad = estadisticas.velocidadDisparo;
                        movimientoDisparo.distanciaMaxima = estadisticas.distanciaMaximaDisparo;
                    }
                    else
                    {
                        Debug.LogError("El prefabDisparo no tiene el script ProyectilScript.");
                    }

                    tiempoUltimoDisparo = Time.time;
                }
                else
                {
                    Debug.LogError("PrefabDisparo o PuntoPartida no están asignados.");
                }
            }
        }

        ActualizarAnimaciones(inputHorizontal2, inputVertical2);
    }

    void ActualizarAnimaciones(float inputHorizontal2, float inputVertical2)
    {
        animator.SetFloat("Horizontal", inputHorizontal2);
        animator.SetFloat("Vertical", inputVertical2);
    }
}
