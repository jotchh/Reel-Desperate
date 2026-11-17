
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("HUD")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text baitText;
    [SerializeField] private TMP_Text quotaText;
    [SerializeField] private Button endDayButton;

    [Header("Upgrade Menu")]
    [SerializeField] private GameObject upgradeMenuPrefab;
    [SerializeField] private Transform upgradeMenuParent;

private GameObject upgradeMenuInstance;

    public int CurrentDay =>
        EconomyManager.Instance != null
            ? EconomyManager.Instance.CurrentDay
            : 1;

    public int RemainingBait { get; private set; }
    public float TimeRemaining { get; private set; }

    public RunPhase CurrentPhase { get; private set; }

    public bool IsDayActive => CurrentPhase == RunPhase.Day;

    // Player can end the day early if they run out of bait
    // OR have enough earned money and balance to pay the quota.
    private bool CanEndDayEarly
    {
        get
        {
            if (!IsDayActive || EconomyManager.Instance == null)
                return false;

            EconomyManager economy = EconomyManager.Instance;

            bool quotaMet =
                economy.TodayEarned >= economy.DailyQuota &&
                economy.Money >= economy.DailyQuota;

            return RemainingBait <= 0 || quotaMet;
        }
    }

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
        if (endDayButton != null)
        {
            endDayButton.onClick.AddListener(EndDayEarly);
            endDayButton.gameObject.SetActive(false);
        }

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

        // Reset day state.
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
    }

    public void EndDayEarly()
    {
        if (!CanEndDayEarly)
            return;

        EndDay();
    }

    private void OpenUpgradeMenu()
    {
        if (upgradeMenuInstance != null)
            return;

        if (upgradeMenuPrefab == null)
        {
            Debug.LogError("Upgrade Menu Prefab is missing!");
            return;
        }

        upgradeMenuInstance = Instantiate(upgradeMenuPrefab);
    }

    private void EndDay()
    {
        if (!IsDayActive)
            return;

        CurrentPhase = RunPhase.DayEnd;
        TimeRemaining = 0f;

        if (playerFishing != null)
            playerFishing.EndFishingForDay();

        bool quotaMet = EconomyManager.Instance.EndDay();

        if (!quotaMet)
        {
            CurrentPhase = RunPhase.GameOver;
            UpdateUI();
            OnDayEnded?.Invoke(false);
            return;
        }

        // Successful day: open the upgrade menu.
        OpenUpgradeMenu();

        UpdateUI();
        OnDayEnded?.Invoke(true);
    }

    // Call after the player selects an upgrade.
    public void StartNextDay()
    {
        if (CurrentPhase != RunPhase.DayEnd)
            return;

        if (upgradeMenuInstance != null)
        {
            Destroy(upgradeMenuInstance);
            upgradeMenuInstance = null;
        }

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

            quotaText.text =
                $"QUOTA: ${economy.TodayEarned} / ${economy.DailyQuota}";
        }

        // Show early-end button when quota is met or bait runs out.
        if (endDayButton != null)
            endDayButton.gameObject.SetActive(CanEndDayEarly);
    }

    private void OnDestroy()
    {
        if (endDayButton != null)
            endDayButton.onClick.RemoveListener(EndDayEarly);

        if (Instance == this)
            Instance = null;
    }
}
