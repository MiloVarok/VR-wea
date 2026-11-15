using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class ItemTableRespawn : MonoBehaviour
{
    [Tooltip("Cuántos metros por debajo de su altura inicial en la mesa debe caer para regresar solo.")]
    public float fallThreshold = 0.15f;
    [Tooltip("Reproducir sonido al reaparecer en la mesa.")]
    public string respawnSoundId = "ItemDrop";

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        // Si lo tenés agarrado en la mano, no hace nada
        if (grabInteractable != null && grabInteractable.isSelected) return;

        // Si cayó por debajo del nivel de la mesa, vuelve exactamente a su punto de origen
        if (transform.position.y < initialPosition.y - fallThreshold)
        {
            RespawnOnTable();
        }
    }

    public void RespawnOnTable()
    {
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.SetPositionAndRotation(initialPosition, initialRotation);

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(respawnSoundId))
        {
            AudioManager.Instance.PlaySFX(respawnSoundId);
        }
    }
}