using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    [Header("Controladores (Haptic Impulse Player)")]
    [Tooltip("Objeto Left Controller")]
    public HapticImpulsePlayer leftControllerHaptics;
    [Tooltip("Objeto Right Controller")]
    public HapticImpulsePlayer rightControllerHaptics;

    [Header("Encender Lighter")]
    [Range(0f, 1f)] public float lighterIgniteAmplitude = 0.25f;
    public float lighterIgniteDuration = 0.06f;

    [Header("Encender Vela Blanca")]
    [Range(0f, 1f)] public float whiteCandleAmplitude = 0.15f;
    public float whiteCandleDuration = 0.08f;

    [Header("Error en Secuencia de Símbolos")]
    [Range(0f, 1f)] public float runeErrorAmplitude = 0.65f;
    public float runeErrorDuration = 0.20f;

    [Header("Encendido de Vela Negra")]
    [Range(0f, 1f)] public float blackCandleAmplitude = 0.55f;
    public float blackCandleDuration = 0.35f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Para conectar directo en el evento Activated / Select Entered del Lighter en el Inspector.
    /// </summary>
    public void TriggerLighterIgniteHaptic(XRBaseInteractable lighterInteractable)
    {
        VibrateInteractableHolder(lighterInteractable, lighterIgniteAmplitude, lighterIgniteDuration);
    }

    public void TriggerWhiteCandleHaptic(XRBaseInteractable lighterInteractable)
    {
        VibrateInteractableHolder(lighterInteractable, whiteCandleAmplitude, whiteCandleDuration);
    }

    public void TriggerRuneErrorHaptic(XRBaseInteractable runeInteractable)
    {
        VibrateInteractableHolder(runeInteractable, runeErrorAmplitude, runeErrorDuration);
    }

    public void TriggerBlackCandleHaptic()
    {
        VibrateBoth(blackCandleAmplitude, blackCandleDuration);
    }

    /// <summary>
    /// Identifica qué mano está interactuando con el objeto y hace vibrar únicamente ese joystick.
    /// </summary>
    public void VibrateInteractableHolder(XRBaseInteractable interactable, float amplitude, float duration)
    {
        if (interactable == null || interactable.firstInteractorSelecting == null) return;

        IXRSelectInteractor interactor = interactable.firstInteractorSelecting;
        HapticImpulsePlayer targetPlayer = interactor.transform.GetComponentInParent<HapticImpulsePlayer>();

        if (targetPlayer != null)
        {
            targetPlayer.SendHapticImpulse(amplitude, duration);
        }
    }

    public void VibrateBoth(float amplitude, float duration)
    {
        if (leftControllerHaptics != null)
            leftControllerHaptics.SendHapticImpulse(amplitude, duration);

        if (rightControllerHaptics != null)
            rightControllerHaptics.SendHapticImpulse(amplitude, duration);
    }
}