using UnityEngine;

public class MagnifierZoneDetector : MonoBehaviour
{
    public RitualManager ritualManager;
    [Tooltip("Tag que debe tener el objeto Magnifier (o su lente)")]
    public string magnifierTag = "Magnifier";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(magnifierTag) && ritualManager != null)
        {
            ritualManager.SetMagnifierNearBox(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(magnifierTag) && ritualManager != null)
        {
            ritualManager.SetMagnifierNearBox(false);
        }
    }
}
