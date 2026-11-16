using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

    [Header("Split Runes (2 Left + 2 Right)")]
    public Image[] runeImages;
    public XRBaseInteractable[] runeInteractables;
    public Color normalRuneColor = Color.cyan;
    public Color pressedRuneColor = Color.green;

    [Header("Box Lid Animation & Crown Drip")]
    public Animator[] boxAnimators;
    public string openParameterName = "Open";
    public bool parameterIsBool = false;
    public ParticleSystem crownLiquidParticles;

    [Header("Atmosphere, Audio & Menu")]
    public Image darkVignette;
    public AudioManager audioManager;
    public GameMenuManager gameMenuManager;
    [Tooltip("Desmarcar si se usa el menu inicial")]
    public bool startRitualImmediately = false;

    [Range(0f, 1f)] public float whisperVolAt2Candles = 0.15f;
    [Range(0f, 1f)] public float whisperVolAt1Candle = 0.30f;
    [Range(0f, 1f)] public float whisperVolAt0Candles = 0.55f;
    [Range(0f, 1f)] public float blackCandleWhisperVol = 0.20f;

    private bool isRitualStarted = false;
    private bool canSeeRunes = false;
    private bool hasReachedFourCandles = false;
    private bool isSequenceReady = false;
    private bool isBoxOpen = false;
    private bool isRitualFinished = false;
    private int activeBlackFlames = 0;
    private float runeChaosTimer = 0f;

    private RuneSymbol[] activeRunes = new RuneSymbol[4];
    private List<int> expectedHierarchyOrder = new List<int>();
    private List<int> playerEnteredValues = new List<int>();
    private bool[] buttonPressedState = new bool[4];

    void Start()
    {
        if (audioManager == null)
            audioManager = GetComponent<AudioManager>();

        if (gameMenuManager == null)
            gameMenuManager = GetComponent<GameMenuManager>();

        if (crownLiquidParticles != null)
            crownLiquidParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

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

        GenerateRandomPuzzle();
        SetRunesVisible(false);

        if (startRitualImmediately)
            StartRitual();
    }

    public void StartRitual()
    {
        if (isRitualStarted) return;
        isRitualStarted = true;
        StartCoroutine(ExtinguishCandlesRoutine());
    }

    public void GenerateRandomPuzzle()
    {
        if (symbolPool.Count < 4)
        {
            Debug.LogError("[RitualManager] al menos 4 símbolos en la Pool.");
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
        if (!isRitualStarted || isRitualFinished) return;

        int litCount = GetLitWhiteCandlesCount();

        float dangerFactor = Mathf.Clamp01((4f - litCount) / 4f);
        if (darkVignette != null)
        {
            Color vignetteColor = darkVignette.color;
            vignetteColor.a = dangerFactor * 0.85f;
            darkVignette.color = vignetteColor;
        }

        if (litCount >= 4)
        {
            canSeeRunes = true;
            hasReachedFourCandles = true;
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

        UpdateWhisperLogic(litCount);

        if (!isBoxOpen && canSeeRunes)
        {
            if (litCount == 5 && !isSequenceReady)
            {
                SetRunesVisible(true);
                runeChaosTimer += Time.deltaTime;
                if (runeChaosTimer >= 0.15f)
                {
                    runeChaosTimer = 0f;
                    ScrambleRuneSymbols();
                }
            }
            else if (litCount == 3 || litCount == 4)
            {
                SetRunesVisible(true);
                RestoreCleanSymbols();
                runeChaosTimer = 0.15f;
            }
            else
            {
                SetRunesVisible(false);
                runeChaosTimer = 0.15f;
            }
        }
        else
        {
            SetRunesVisible(false);
            runeChaosTimer = 0.15f;
        }
    }

    private void UpdateWhisperLogic(int litWhiteCount)
    {
        if (audioManager == null) return;

        float whiteWhisperVol = 0f;

        if (hasReachedFourCandles)
        {
            if (litWhiteCount == 2)
                whiteWhisperVol = whisperVolAt2Candles;
            else if (litWhiteCount == 1)
                whiteWhisperVol = whisperVolAt1Candle;
            else if (litWhiteCount == 0)
                whiteWhisperVol = whisperVolAt0Candles;
        }

        float blackWhisperVol = (activeBlackFlames > 0) ? blackCandleWhisperVol : 0f;

        audioManager.SetWhisperVolume(Mathf.Max(whiteWhisperVol, blackWhisperVol));
    }

    private void SetRunesVisible(bool visible)
    {
        if (runeImages == null) return;

        for (int i = 0; i < runeImages.Length; i++)
        {
            if (runeImages[i] != null && runeImages[i].gameObject.activeSelf != visible)
            {
                runeImages[i].gameObject.SetActive(visible);
            }
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

                if (hasReachedFourCandles)
                {
                    if (audioManager != null)
                        audioManager.PlayRandomSpooky();

                    if (Random.value < randomBlackFlameChance)
                        IgniteBlackFlame();
                }
            }
        }
    }

    public void IgniteWhiteCandle(GameObject targetFlame)
    {
        if (!isRitualStarted || isRitualFinished || targetFlame == null) return;

        if (!targetFlame.activeSelf)
        {
            targetFlame.SetActive(true);
            if (audioManager != null) audioManager.PlaySFX("CandleIgnite");
            TryOpenBox();
        }
    }

    public void PressRune(int runeIndex)
    {
        int litCount = GetLitWhiteCandlesCount();

        if (!isRitualStarted || isBoxOpen || isSequenceReady || !canSeeRunes) return;
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

            if (audioManager != null) audioManager.PlaySFX("RunePress");

            if (playerEnteredValues.Count >= 4)
            {
                isSequenceReady = true;
                if (audioManager != null) audioManager.PlaySFX("SequenceReady");
                TryOpenBox();
            }
        }
        else
        {
            IgniteBlackFlame();
            ResetRunes();
            if (audioManager != null) audioManager.PlaySFX("RuneError");
        }
    }

    private void TryOpenBox()
    {
        if (isSequenceReady && !isBoxOpen && GetLitWhiteCandlesCount() == 5)
        {
            isBoxOpen = true;
            SetRunesVisible(false);
            TriggerBoxOpenAnimation();

            if (crownLiquidParticles != null)
                crownLiquidParticles.Play();

            if (audioManager != null) audioManager.PlaySFX("BoxOpen");
        }
    }

    private void TriggerBoxOpenAnimation()
    {
        if (boxAnimators == null || string.IsNullOrEmpty(openParameterName)) return;

        foreach (Animator anim in boxAnimators)
        {
            if (anim == null) continue;

            if (parameterIsBool)
                anim.SetBool(openParameterName, true);
            else
                anim.SetTrigger(openParameterName);
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
            if (audioManager != null) audioManager.PlaySFX("BlackCandle");
        }
    }

    public void SetMagnifierNearBox(bool isNear) { }

    public void CompleteRitual()
    {
        if (!isBoxOpen || isRitualFinished) return;

        isRitualFinished = true;
        if (darkVignette != null) darkVignette.gameObject.SetActive(false);

        if (audioManager != null)
        {
            audioManager.StopAllLoops();
            audioManager.PlaySFX("Complete");
        }

        if (gameMenuManager != null)
            gameMenuManager.ShowVictoryScreen();
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