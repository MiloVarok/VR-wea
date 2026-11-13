using UnityEngine;

public class LimitesHabitacionVR : MonoBehaviour
{
    [Header("Límites de la Habitación (desde el centro 0,0)")]
    public float limiteX = 2.6f;
    public float limiteZ = 2.6f;

    [Header("Exclusión de la Mesa (Cilindro central)")]
    public Transform centroMesa;
    public float radioExclusionMesa = 1.05f;

    [Header("Referencia a la Cámara")]
    public Transform mainCamera;

    void LateUpdate()
    {
        Vector3 posRig = transform.position;
        posRig.y = 0f;
        transform.position = posRig;

        if (mainCamera == null) return;

        Vector3 posCabeza = mainCamera.position;
        Vector3 desplazamientoCorreccion = Vector3.zero;

        // Freno contra las paredes
        if (posCabeza.x > limiteX)
            desplazamientoCorreccion.x = limiteX - posCabeza.x;
        else if (posCabeza.x < -limiteX)
            desplazamientoCorreccion.x = -limiteX - posCabeza.x;

        if (posCabeza.z > limiteZ)
            desplazamientoCorreccion.z = limiteZ - posCabeza.z;
        else if (posCabeza.z < -limiteZ)
            desplazamientoCorreccion.z = -limiteZ - posCabeza.z;

        transform.position += desplazamientoCorreccion;

        // Freno circular contra la mesa central
        if (centroMesa != null)
        {
            Vector2 cabeza2D = new Vector2(mainCamera.position.x, mainCamera.position.z);
            Vector2 mesa2D = new Vector2(centroMesa.position.x, centroMesa.position.z);
            float distancia = Vector2.Distance(cabeza2D, mesa2D);

            if (distancia < radioExclusionMesa && distancia > 0.001f)
            {
                
                Vector2 direccionAfuera = (cabeza2D - mesa2D).normalized;
                float penetracion = radioExclusionMesa - distancia;
                transform.position += new Vector3(direccionAfuera.x * penetracion, 0f, direccionAfuera.y * penetracion);
            }
        }
    }
}