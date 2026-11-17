using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(XROrigin))]
public class RecenterPlayerOnStart : MonoBehaviour
{
    [Tooltip("Dirección hacia donde debe mirar el jugador al iniciar (por defecto +Z, hacia el menú y la mesa).")]
    public Vector3 desiredForward = Vector3.forward;

    private IEnumerator Start()
    {
        // Espera a que el subsistema OpenXR de Quest 2 entregue la rotación inicial del casco
        yield return null;
        yield return null;

        XROrigin xrOrigin = GetComponent<XROrigin>();
        if (xrOrigin != null && xrOrigin.Camera != null)
        {
            xrOrigin.MatchOriginUpCameraForward(Vector3.up, desiredForward);
        }
    }
}
