using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class InstructionSheet : MonoBehaviour
{
    [Header("Páginas de Instrucciones")]
    [Tooltip("Páginas en orden.")]
    public List<Sprite> pages = new List<Sprite>();
    [Tooltip("Componente UI")]
    public Image pageDisplayImage;

    [Header("Controles e Interacción")]
    [Tooltip("Si está activo: da vuelta la hoja con gatillo")]
    public bool flipOnTriggerWhenHeld = true;
    [Tooltip("ID de sonido en AudioManager")]
    public string pageFlipSoundId = "ItemPickup";

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private int currentPageIndex = 0;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

    
        grabInteractable.throwOnDetach = false;
    }

    private void Start()
    {
        // Guarda el punto de retorno en la mesa
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        rb.useGravity = false;
        rb.isKinematic = true;

        UpdatePageVisual();
    }

    private void OnEnable()
    {
        grabInteractable.selectExited.AddListener(OnSheetReleased);
        grabInteractable.activated.AddListener(OnSheetTriggered);
    }

    private void OnDisable()
    {
        grabInteractable.selectExited.RemoveListener(OnSheetReleased);
        grabInteractable.activated.RemoveListener(OnSheetTriggered);
    }

    private void OnSheetTriggered(ActivateEventArgs args)
    {
        if (flipOnTriggerWhenHeld)
        {
            NextPage();
        }
    }

    private void OnSheetReleased(SelectExitEventArgs args)
    {
        StartCoroutine(ReturnToTableEndOfFrame());
    }

    private IEnumerator ReturnToTableEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.useGravity = false;
        rb.isKinematic = true;

        transform.SetPositionAndRotation(initialPosition, initialRotation);

        // Pa qe siga mostrando la misma página que se estaba leyendo
        UpdatePageVisual();
    }

    public void NextPage()
    {
        if (pages == null || pages.Count == 0) return;

        currentPageIndex = (currentPageIndex + 1) % pages.Count;
        UpdatePageVisual();
        PlayFlipFeedback();
    }

    public void PreviousPage()
    {
        if (pages == null || pages.Count == 0) return;

        currentPageIndex = (currentPageIndex - 1 + pages.Count) % pages.Count;
        UpdatePageVisual();
        PlayFlipFeedback();
    }

    private void UpdatePageVisual()
    {
        if (pageDisplayImage != null && pages != null && pages.Count > 0)
        {
            pageDisplayImage.sprite = pages[currentPageIndex];
        }
    }

    private void PlayFlipFeedback()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(pageFlipSoundId))
        {
            AudioManager.Instance.PlaySFX(pageFlipSoundId);
        }

    }
}