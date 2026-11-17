using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class LighterHoldController : MonoBehaviour
{
    public Animator lighterAnimator;
    [Tooltip("Nombre del parámetro Bool en el Animator que mantiene abierta la tapa.")]
    public string isOpenBoolParameter = "IsOpen";
    [Tooltip("Objeto visual de la llama del encendedor.")]
    public GameObject lighterFlame;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (lighterAnimator == null)
            lighterAnimator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void Start()
    {
        SetLighterState(false);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        SetLighterState(true);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        SetLighterState(false);
    }

    private void SetLighterState(bool open)
    {
        if (lighterAnimator != null && !string.IsNullOrEmpty(isOpenBoolParameter))
            lighterAnimator.SetBool(isOpenBoolParameter, open);

        if (lighterFlame != null)
            lighterFlame.SetActive(open);
    }
}
