using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class RuneSymbol
{
    public string symbolName;
    public Sprite sprite;
    public int hierarchyValue;
}

public class RitualManager : MonoBehaviour
{
    [Header("White Candles (Flame 1 to 5)")]
    public List<GameObject> whiteFlames;
    public float minExtinguishTime = 12f;
    public float maxExtinguishTime = 20f;

    [Header("Black Candles (BlackFlame 1 to 3)")]
    public List<GameObject> blackFlames;
    [Range(0f, 1f)]
    public float randomBlackFlameChance = 0.15f;

    [Header("Symbol Pool & Hierarchy")]
    public List<RuneSymbol> symbolPool = new List<RuneSymbol>();

    [Header("Runes & Magnifier")]
    public GameObject runesContainer;
    public Image[] runeImages;
    public Color normalRuneColor = Color.cyan;
    public Color pressedRuneColor = Color.green;

    [Header("Box Lid & Crown Extraction")]
    [Tooltip("Arrastrá acá el objeto de la TAPA de la caja que debe rotar para abrirse.")]
    public Transform boxLid;
    [Tooltip("Ángulo en grados para abrir la tapa (ej. X = -95 o +95 según el eje de la bisagra).")]
    public Vector3 lidOpenRotationOffset = new Vector3(-95f, 0f, 0f);
    public float lidOpenSpeed = 2.5f;

    [Header("Atmosphere & Feedback")]
    public Image darkVignette;
    public AudioSource whisperAudio;
    public AudioSource errorAudio;
    public AudioSource successAudio;

    // Estado interno del ritual
    private bool isMagnifierNearBox = false;
    private bool canSeeRunes = false;
    private bool isSequenceReady = false;
    private bool isBoxOpen = false;
    private bool isRitualFinished = false;
    private int activeBlackFlames = 0;
    private float runeChaosTimer = 0f;

    // Rotación de la tapa
    private Quaternion lidClosedRotation;
    private Quaternion lidTargetRotation;

    // Estado interno de las 4 runas activas
    private RuneSymbol[] activeRunes = new RuneSymbol[4];
    private List<int> expectedHierarchyOrder = new List<int>();
    private List<int> playerEnteredValues = new List<int>();
    private bool[] buttonPressedState = new bool[4];

    void Start()
    {
        foreach (GameObject flame in whiteFlames)
        {
            if (flame != null)
                flame.SetActive(false);
        }

        foreach (GameObject blackFlame in blackFlames)
        {
            if (blackFlame != null)
                blackFlame.SetActive(false);
        }

        if (runesContainer != null)
            runesContainer.SetActive(false);

        if (boxLid != null)
        {
            lidClosedRotation = boxLid.localRotation;
            lidTargetRotation = lidClosedRotation * Quaternion.Euler(lidOpenRotationOffset);
        }

        GenerateRandomPuzzle();
        StartCoroutine(ExtinguishCandlesRoutine());
    }

    public void GenerateRandomPuzzle()
    {
        if (symbolPool.Count < 4)
        {
            Debug.LogError("[RitualManager] Necesitás cargar al menos 4 símbolos en Symbol Pool.");
            return;
        }

        List<RuneSymbol> shuffledPool = symbolPool.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < 4; i++)
        {
            activeRunes[i] = shuffledPool[i];
            buttonPressedState[i] = false;

            if (runeImages != null && i < runeImages.Length && runeImages[i] != null)
            {
                runeImages[i].sprite = activeRunes[i].sprite;
                runeImages[i].color = normalRuneColor;
                runeImages[i].transform.localRotation = Quaternion.identity;
            }
        }

        expectedHierarchyOrder = activeRunes
            .Select(r => r.hierarchyValue)
            .OrderBy(val => val)
            .ToList();

        playerEnteredValues.Clear();
    }

    void Update()
    {
        // Apertura suave de la tapa de la caja una vez resuelto el código y encendida la 5ta vela
        if (isBoxOpen && boxLid != null)
        {
            boxLid.localRotation = Quaternion.Slerp(boxLid.localRotation, lidTargetRotation, Time.deltaTime * lidOpenSpeed);
        }

        if (isRitualFinished) return;

        int litCount = GetLitWhiteCandlesCount();

        // Oscurecimiento de pantalla y susurros
        float dangerFactor = Mathf.Clamp01((4f - litCount) / 4f);
        if (darkVignette != null)
        {
            Color vignetteColor = darkVignette.color;
            vignetteColor.a = dangerFactor * 0.85f;
            darkVignette.color = vignetteColor;
        }

        if (whisperAudio != null)
            whisperAudio.volume = 0.1f + (dangerFactor * 0.9f);

        // Umbral de visibilidad y regeneración al caer por debajo de 3 velas
        if (litCount >= 4)
        {
            canSeeRunes = true;
        }
        else if (litCount <= 2)
        {
            if (canSeeRunes)
            {
                canSeeRunes = false;
                if (!isSequenceReady && !isBoxOpen)
                {
                    ResetRunes();
                    GenerateRandomPuzzle();
                }
            }
        }

        // Comportamiento visual de los símbolos según cantidad de velas y lupa
        if (!isBoxOpen && isMagnifierNearBox && canSeeRunes)
        {
            if (litCount == 5 && !isSequenceReady)
            {
                runesContainer.SetActive(true);
                runeChaosTimer += Time.deltaTime;
                if (runeChaosTimer >= 0.25f)
                {
                    runeChaosTimer = 0f;
                    ScrambleRuneSymbols();
                }
            }
            else if (litCount == 3 || litCount == 4)
            {
                runesContainer.SetActive(true);
                RestoreCleanSymbols();
            }
            else
            {
                runesContainer.SetActive(false);
            }
        }
        else if (runesContainer != null)
        {
            runesContainer.SetActive(false);
        }
    }

    private IEnumerator ExtinguishCandlesRoutine()
    {
        while (!isRitualFinished)
        {
            yield return new WaitForSeconds(Random.Range(minExtinguishTime, maxExtinguishTime));
            if (isRitualFinished) break;

            List<GameObject> litFlames = whiteFlames.FindAll(f => f != null && f.activeSelf);
            if (litFlames.Count > 0)
            {
                litFlames[Random.Range(0, litFlames.Count)].SetActive(false);

                if (Random.value < randomBlackFlameChance)
                    IgniteBlackFlame();
            }
        }
    }

    public void IgniteWhiteCandle(GameObject targetFlame)
    {
        if (isRitualFinished || targetFlame == null) return;

        if (!targetFlame.activeSelf)
        {
            targetFlame.SetActive(true);
            TryOpenBox();
        }
    }

    public void PressRune(int runeIndex)
    {
        int litCount = GetLitWhiteCandlesCount();

        if (isBoxOpen || isSequenceReady || !isMagnifierNearBox || !canSeeRunes) return;
        if (litCount != 3 && litCount != 4) return;
        if (runeIndex < 0 || runeIndex >= 4 || buttonPressedState[runeIndex]) return;

        int pressedValue = activeRunes[runeIndex].hierarchyValue;
        int currentStep = playerEnteredValues.Count;

        if (pressedValue == expectedHierarchyOrder[currentStep])
        {
            buttonPressedState[runeIndex] = true;
            playerEnteredValues.Add(pressedValue);

            if (runeImages[runeIndex] != null)
                runeImages[runeIndex].color = pressedRuneColor;

            if (playerEnteredValues.Count >= 4)
            {
                isSequenceReady = true;
                if (successAudio != null) successAudio.Play();
                TryOpenBox();
            }
        }
        else
        {
            IgniteBlackFlame();
            ResetRunes();
            if (errorAudio != null) errorAudio.Play();
        }
    }

    /// <summary>
    /// Abre la tapa de la caja únicamente cuando la secuencia fue ingresada Y están las 5 velas encendidas.
    /// </summary>
    private void TryOpenBox()
    {
        if (isSequenceReady && !isBoxOpen && GetLitWhiteCandlesCount() == 5)
        {
            isBoxOpen = true;
            if (runesContainer != null) runesContainer.SetActive(false);
            if (successAudio != null) successAudio.Play();
        }
    }

    private void ResetRunes()
    {
        if (isSequenceReady || isBoxOpen) return;

        playerEnteredValues.Clear();
        for (int i = 0; i < 4; i++)
        {
            buttonPressedState[i] = false;
            if (runeImages != null && i < runeImages.Length && runeImages[i] != null)
            {
                runeImages[i].color = normalRuneColor;
            }
        }
    }

    private void ScrambleRuneSymbols()
    {
        if (symbolPool.Count == 0 || runeImages == null) return;

        foreach (Image img in runeImages)
        {
            if (img != null)
            {
                img.sprite = symbolPool[Random.Range(0, symbolPool.Count)].sprite;
                img.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0, 4) * 90f);
            }
        }
    }

    private void RestoreCleanSymbols()
    {
        if (runeImages == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (i < runeImages.Length && runeImages[i] != null && activeRunes[i] != null)
            {
                if (runeImages[i].sprite != activeRunes[i].sprite)
                    runeImages[i].sprite = activeRunes[i].sprite;

                runeImages[i].transform.localRotation = Quaternion.identity;
            }
        }
    }

    private void IgniteBlackFlame()
    {
        if (activeBlackFlames < blackFlames.Count)
        {
            if (blackFlames[activeBlackFlames] != null)
                blackFlames[activeBlackFlames].SetActive(true);

            activeBlackFlames++;
        }
    }

    public void SetMagnifierNearBox(bool isNear)
    {
        isMagnifierNearBox = isNear;
    }

    public void CompleteRitual()
    {
        if (!isBoxOpen || isRitualFinished) return;

        isRitualFinished = true;
        if (darkVignette != null) darkVignette.gameObject.SetActive(false);
        if (whisperAudio != null) whisperAudio.Stop();
        if (successAudio != null) successAudio.Play();
    }

    private int GetLitWhiteCandlesCount()
    {
        int count = 0;
        foreach (GameObject flame in whiteFlames)
        {
            if (flame != null && flame.activeSelf) count++;
        }
        return count;
    }
}