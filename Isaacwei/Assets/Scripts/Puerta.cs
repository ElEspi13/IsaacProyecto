using UnityEngine;

public class Door : MonoBehaviour
{
    public ROOM_DIRECTIONS direccion; // Dirección relativa de la puerta

    [Header("Componentes")]
    public Collider2D triggerCollider; // Collider que permite moverse entre salas
    public Collider2D blockingCollider; // Collider que bloquea la puerta si hay enemigos
    public SpriteRenderer puertaSprite; // Sprite de la puerta para mostrar estado (opcional)

    private void Start()
    {
        // Revisar el estado inicial de la puerta
        RevisarPuerta();

        // Suscribirse al evento de limpieza de enemigos (si existe en DungeonManager)
        DungeonManager.Instance.OnSalaLimpia += AbrirPuerta;
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento al destruir el objeto
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnSalaLimpia -= AbrirPuerta;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar si el jugador entra en el trigger
        if (other.CompareTag("Player") && !blockingCollider.enabled)
        {
            Debug.Log($"🔍 Jugador entrando por la puerta en dirección: {direccion}");

            // Calcular la sala conectada y mover al jugador
            DungeonManager.Instance.MoverJugadorADireccion(direccion);
        }
    }

    private void RevisarPuerta()
    {
        // Verificar si la sala actual está limpia de enemigos
        bool salaLimpia = DungeonManager.Instance.SalaActualLimpia();
        Debug.Log($"pueltas:{triggerCollider.enabled}");
        Debug.Log($"pueltas:{blockingCollider.enabled}");
        if (salaLimpia)
        {
            triggerCollider.enabled = salaLimpia; // Activar el trigger si la sala está limpia
            blockingCollider.enabled = !salaLimpia; // Bloquear la puerta si hay enemigos
        }
        else if (salaLimpia == false) // Si la sala no está limpia, bloquear la puerta
        {
            triggerCollider.enabled = salaLimpia; // Activar el trigger si la sala está limpia
            blockingCollider.enabled = !salaLimpia; // Bloquear la puerta si hay enemigos
        }

            

        
        // Cambiar el sprite de la puerta para reflejar su estado (opcional)
        if (puertaSprite != null)
        {
            puertaSprite.color = salaLimpia ? Color.white : Color.gray; // Blanco para abierta, gris para cerrada
        }
        Debug.Log($"pueltas");
    }
    private void AbrirPuerta()
    {
        
            triggerCollider.enabled = true; // Activar el trigger si la sala está limpia
            blockingCollider.enabled = false; // Bloquear la puerta si hay enemigos
        

    }
}