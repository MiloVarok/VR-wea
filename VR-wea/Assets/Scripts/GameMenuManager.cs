using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    [Header("Referencias de Sistemas")]
    public RitualManager ritualManager;
    public AudioManager audioManager;

    [Header("Menú Inicial")]
    public GameObject startMenuPanel;

    [Header("Fade In")]
    public Image blackFadeOverlay;
    public float introFadeDuration = 2.5f;

    [Header("Pantalla de Finalización")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryMessageText;
    [TextArea(2, 4)]
    public string victoryMessage = "Has completado el ritual.\nLa corona ha sido liberada.";

    private bool hasGameStarted = false;

    private void Start()
    {
        if (ritualManager == null)
            ritualManager = GetComponent<RitualManager>();

        if (audioManager == null)
            audioManager = GetComponent<AudioManager>();

        if (startMenuPanel != null)
            startMenuPanel.SetActive(true);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (blackFadeOverlay != null)
        {
            blackFadeOverlay.gameObject.SetActive(true);
            Color c = blackFadeOverlay.color;
            c.a = 1f;
            blackFadeOverlay.color = c;
            blackFadeOverlay.raycastTarget = false;
        }
    }


    public void StartGame()
    {
        if (hasGameStarted) return;
        hasGameStarted = true;

        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (audioManager != null)
        {
            audioManager.PlaySFX("CandleIgnite");
            audioManager.FadeInBackgroundMusic(introFadeDuration);
        }

        StartCoroutine(FadeInSceneRoutine());
    }

    private IEnumerator FadeInSceneRoutine()
    {
        if (blackFadeOverlay != null)
        {
            float elapsed = 0f;
            Color c = blackFadeOverlay.color;

            while (elapsed < introFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(0.01f, introFadeDuration));
                c.a = Mathf.Lerp(1f, 0f, t);
                blackFadeOverlay.color = c;
                yield return null;
            }

            c.a = 0f;
            blackFadeOverlay.color = c;
            blackFadeOverlay.gameObject.SetActive(false);
        }

        if (ritualManager != null)
            ritualManager.StartRitual();
    }


    public void ShowVictoryScreen()
    {
        if (victoryMessageText != null)
            victoryMessageText.text = victoryMessage;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
