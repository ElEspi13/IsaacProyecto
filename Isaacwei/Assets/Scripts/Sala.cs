using UnityEngine;

public class Sala : MonoBehaviour
{
    public bool TienePuertaArriba;
    public bool TienePuertaAbajo;
    public bool TienePuertaIzquierda;
    public bool TienePuertaDerecha;

    void Awake()
    {
        Transform puertas = transform.Find("Puertas");
        if (puertas != null)
        {
            TienePuertaArriba = puertas.Find("PuertaArriba") != null;
            TienePuertaAbajo = puertas.Find("PuertaAbajo") != null;
            TienePuertaIzquierda = puertas.Find("PuertaIzquierda") != null;
            TienePuertaDerecha = puertas.Find("PuertaDerecha") != null;
        }
    }
}
public enum DungeonType
{
    Normal,
    Boss,
    Treasure
}