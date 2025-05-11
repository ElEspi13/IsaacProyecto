using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class ObjetoControlador : MonoBehaviour
{
    public enum TipoEfecto { Vida, Dano, VelocidadAtaque, Tamano,
        VelocidadDisparo
    }
    public enum TipoOperacion { Sumar, Restar, Multiplicar, Dividir }

    [System.Serializable]
    public class Efecto
    {
        public TipoEfecto tipoEfecto;
        public TipoOperacion operacion;
        public float cantidad;
    }

    public Efecto[] efectos; // Lista de efectos que aplicará este objeto

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("✅ Algo ha entrado en el trigger: " + other.name);
            
            if (EstadisticasJugador.instancia!= null)
            {
                Debug.Log("✅ Algo ha entrado en el trigger: " + other.name);
                foreach (var efecto in efectos)
                {
                    EstadisticasJugador.instancia.AplicarModificador(efecto.tipoEfecto, efecto.operacion, efecto.cantidad);
                }

                Destroy(gameObject); // Destruir objeto al recogerlo
            }
        }
    }
}
