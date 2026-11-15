using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CrownExtractionTrigger : MonoBehaviour
{
    public RitualManager ritualManager;
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.AddListener(OnCrownGrabbed);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnCrownGrabbed);
    }

    private void OnCrownGrabbed(SelectEnterEventArgs args)
    {
        if (ritualManager != null)
        {
            ritualManager.CompleteRitual();
        }
    }
}
