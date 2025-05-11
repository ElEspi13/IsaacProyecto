using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    void Update()
    {
        if (Time.timeScale == 0f) // Solo escucha cuando el juego está pausado (tras Game Over)
        {
            if (Input.GetKeyDown(KeyCode.Return)) // Intro
            {

                GeneradorMazmorra generador = GameObject.Find("Sistema").GetComponent<GeneradorMazmorra>();
                generador.ReiniciarMazmorra();

                // Reiniciar la posición de la cámara
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    mainCamera.transform.position = new Vector3(0, 0, -10);
                }
                else
                {
                    Debug.LogWarning("⚠️ No se encontró la cámara principal.");
                }

                // Restaurar estadísticas del jugador
                if (EstadisticasJugador.instancia != null)
                {
                    EstadisticasJugador.instancia.RestaurarValoresIniciales();
                }
                else
                {
                    Debug.LogWarning("⚠️ No se encontró la instancia de EstadisticasJugador.");
                }

                // Buscar y desactivar UI de Game Over y Victoria, si existen
                GameObject canvas = GameObject.Find("Canvas");
                if (canvas != null)
                {
                    canvas.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("⚠️ No se encontró el canvas principal.");
                }

                GameObject victoria = GameObject.Find("Victoria");
                if (victoria != null)
                {
                    victoria.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("⚠️ No se encontró el canvas de victoria.");
                }

                Time.timeScale = 1f; // Asegúrate de restaurar el tiempo
            }
            else if (Input.GetKeyDown(KeyCode.Escape)) // Escape
            {
                Debug.Log("Saliendo del juego...");
                Application.Quit(); // Sale del juego (en el editor no hace nada, pero en build sí)
            }
        }
    }
}