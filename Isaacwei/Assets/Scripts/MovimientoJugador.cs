using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public EstadisticasJugador estadisticas; // Referencia compartida
    public Animator animator;
    private Rigidbody2D rb;
    private Vector2 direccionInput;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Bloquear rotación y desactivar gravedad en Rigidbody2D
        rb.freezeRotation = true;
        rb.gravityScale = 0;

        if (estadisticas == null)
        {
            estadisticas = Object.FindAnyObjectByType<EstadisticasJugador>();
            if (estadisticas == null)
            {
                Debug.LogError("❌ ERROR: No se encontró EstadisticasJugador en la escena.");
                return;
            }
        }
    }

    void Update()
    {
        // Capturar la entrada del jugador
        direccionInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Actualizar animaciones
        ActualizarAnimaciones(direccionInput);
    }

    void FixedUpdate()
    {
        // Aplicar la velocidad al Rigidbody2D
        rb.linearVelocity = direccionInput * estadisticas.velocidad;
    }

    void ActualizarAnimaciones(Vector2 input)
    {
        animator.SetBool("IsRunning", input.magnitude > 0f);
    }
}

