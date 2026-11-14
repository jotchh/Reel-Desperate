
using System;
using TMPro;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public enum RunPhase
    {
        Day,
        DayEnd,
        GameOver
    }

    [Header("Day Settings")]
    [SerializeField] private float dayDuration = 120f;
    [SerializeField] private int baitPerDay = 10;

    [Header("References")]
    [SerializeField] private PlayerFishing playerFishing;

    [Header("UI")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text baitText;
    [SerializeField] private TMP_Text quotaText;

    public int CurrentDay =>
        EconomyManager.Instance != null
            ? EconomyManager.Instance.CurrentDay
            : 1;

    public int RemainingBait { get; private set; }
    public float TimeRemaining { get; private set; }

    public RunPhase CurrentPhase { get; private set; }

    public bool IsDayActive => CurrentPhase == RunPhase.Day;

    public event Action<int> OnDayStarted;
    public event Action<int> OnBaitChanged;
    public event Action<bool> OnDayEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (playerFishing == null)
            playerFishing = FindObjectOfType<PlayerFishing>();
    }

    private void Start()
    {
        StartDay();
    }

    private void Update()
    {
        if (!IsDayActive)
            return;

        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            EndDay();
        }

        UpdateUI();
    }

    private void StartDay()
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogError("RunManager: EconomyManager is missing!");
            return;
        }

        if (EconomyManager.Instance.IsGameOver ||
            EconomyManager.Instance.HasEscaped)
        {
            CurrentPhase = RunPhase.GameOver;
            return;
        }

        CurrentPhase = RunPhase.Day;

        TimeRemaining = dayDuration;
        RemainingBait = baitPerDay;

        UpdateUI();

        OnBaitChanged?.Invoke(RemainingBait);
        OnDayStarted?.Invoke(CurrentDay);

        Debug.Log(
            $"Day {CurrentDay} started! " +
            $"Quota: ${EconomyManager.Instance.DailyQuota}"
        );
    }

    public void UseBait()
    {
        if (!IsDayActive || RemainingBait <= 0)
            return;

        RemainingBait--;

        OnBaitChanged?.Invoke(RemainingBait);

        Debug.Log($"Bait remaining: {RemainingBait}");

        UpdateUI();

        if (RemainingBait <= 0)
            EndDay();
    }

    public void EndDay()
    {
        if (!IsDayActive)
            return;

        CurrentPhase = RunPhase.DayEnd;
        TimeRemaining = 0f;

        // Stop ongoing fishing.
        if (playerFishing != null)
            playerFishing.EndFishingForDay();

        // Let EconomyManager handle quota payment.
        bool quotaMet = EconomyManager.Instance.EndDay();

        if (!quotaMet)
            CurrentPhase = RunPhase.GameOver;

        UpdateUI();

        Debug.Log(
            quotaMet
                ? $"Day {CurrentDay - 1} completed!"
                : "Game Over! Daily quota was not met."
        );

        OnDayEnded?.Invoke(quotaMet);
    }

    // Called after the player selects an upgrade.
    public void StartNextDay()
    {
        if (CurrentPhase != RunPhase.DayEnd)
            return;

        StartDay();
    }

    private void UpdateUI()
    {
        if (dayText != null)
            dayText.text = $"DAY {CurrentDay}";

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(TimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(TimeRemaining % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (baitText != null)
            baitText.text = $"BAIT: {RemainingBait}";

        if (EconomyManager.Instance != null && quotaText != null)
        {
            EconomyManager economy = EconomyManager.Instance;

            quotaText.text = $"QUOTA: ${economy.TodayEarned} / ${economy.DailyQuota}";
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
