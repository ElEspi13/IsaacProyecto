using UnityEngine;

public class CameraSizeController : MonoBehaviour
{
    public float targetWidth = 25f;
    public float targetHeight = 5f;

    void Start()
    {
        Camera cam = GetComponent<Camera>();

        float screenAspect = (float)Screen.width / Screen.height;
        float targetAspect = targetWidth / targetHeight;

        if (screenAspect >= targetAspect)
        {
            // Pantalla m�s ancha: limitamos por altura
            cam.orthographicSize = targetHeight / 2f;
        }
        else
        {
            // Pantalla m�s alta: limitamos por ancho
            float adjustedHeight = targetWidth / screenAspect;
            cam.orthographicSize = adjustedHeight / 2f;
        }
    }
}
