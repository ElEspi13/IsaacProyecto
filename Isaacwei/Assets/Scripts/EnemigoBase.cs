using UnityEngine;

public class EnemigoBase : MonoBehaviour
{
    public float vida = 10f;

    public virtual void RecibirDano(float cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            Morir();
        }
    }

    protected virtual void Morir()
    {
        Debug.Log("¡Enemigo eliminado!");
        Destroy(gameObject);
        DungeonManager.Instance.EliminarEnemigo();

    }
}

