using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishingMinigame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject minigameRoot;
    [SerializeField] private RectTransform bar;
    [SerializeField] private RectTransform sweetSpot;
    [SerializeField] private RectTransform fishIcon;
    [SerializeField] private Image fishImage;
    [SerializeField] private RectTransform pressureBar;
    [SerializeField] private RectTransform pressureFill;
    [SerializeField] private TMP_Text fishToolTip;

    [Header("Rarity Colors")]
    [SerializeField] private Color trashColor = new Color(0.6f, 0.6f, 0.6f);     // gray
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color uncommonColor = new Color(0.4f, 0.9f, 0.4f);  // green
    [SerializeField] private Color rareColor = new Color(0.35f, 0.6f, 1f);       // blue
    [SerializeField] private Color legendaryColor = new Color(1f, 0.75f, 0.2f);  // gold

    [Header("Sweet Spot Movement")]
    [Tooltip("How quickly the sweet spot falls while Reel is not held (bars per second, per second)")]
    [SerializeField] private float gravity = 2f;

    [Header("Fish Movement")]
    [Tooltip("Converts FishData.fishSpeed into bars per second")]
    [SerializeField] private float fishSpeedScale = 0.3f;
    [Tooltip("Seconds between direction changes at 0 and 1 erraticness")]
    [SerializeField] private Vector2 retargetInterval = new Vector2(1.5f, 0.3f);
    [Tooltip("Furthest the fish darts in one move at 0 and 1 erraticness (fraction of the bar)")]
    [SerializeField] private Vector2 dartDistance = new Vector2(0.2f, 1f);

    [Header("Progress")]
    [Range(0f, 1f)] [SerializeField] private float startProgress = 0.4f;
    [Tooltip("Progress gained per second while the fish is inside the sweet spot")]
    [SerializeField] private float progressFillRate = 0.3f;

    public bool IsRunning { get; private set; }

    private FishData fish;
    private FishingStats stats;
    private System.Func<bool> reelHeld;
    private System.Action<bool> finished;

    private float sweetSpotSize;
    private float sweetSpotPosition;
    private float sweetSpotVelocity;
    private float fishPosition;
    private float fishTarget;
    private float retargetTimer;
    private float progress;

    private void Awake()
    {
        Hide();
    }

    public void Begin(
        FishData fishData,
        FishingStats fishingStats,
        System.Func<bool> reelHeld,
        System.Action<bool> finished)
    {
        if (!HasUI())
        {
            Debug.LogWarning("FishingMinigame: UI references are not assigned! The fish escapes.");
            finished?.Invoke(false);
            return;
        }

        fish = fishData;
        stats = fishingStats;
        this.reelHeld = reelHeld;
        this.finished = finished;

        sweetSpotSize = Mathf.Clamp01(fish.sweetSpotSize + stats.sweetSpotSizeBonus);
        sweetSpotPosition = 0f;
        sweetSpotVelocity = 0f;
        fishPosition = 0.5f;
        fishTarget = fishPosition;
        retargetTimer = 0f;
        progress = startProgress;

        if (fishImage != null && fish.sprite != null)
            fishImage.sprite = fish.sprite;

    if (fishToolTip != null)
    {
        string hex = ColorUtility.ToHtmlStringRGB(GetRarityColor(fish.rarity));
        fishToolTip.text = $"<color=#{hex}>{fish.fishName}\n${Mathf.RoundToInt(fish.sellPrice)}</color>";
    }
        // Sizes are set in the bar's local space, so undo the scale set on each element.
        SetHeight(sweetSpot, bar.rect.height * sweetSpotSize);

        IsRunning = true;
        minigameRoot.SetActive(true);
        UpdateUI();
    }

    public void Cancel()
    {
        IsRunning = false;
        Hide();
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        MoveSweetSpot();
        MoveFish();
        UpdateProgress();
        UpdateUI();

        if (progress >= 1f)
            Finish(true);
        else if (progress <= 0f)
            Finish(false);
    }

    private void MoveSweetSpot()
    {
        bool held = reelHeld != null && reelHeld();

        sweetSpotVelocity += (held ? stats.reelStrength : -gravity) * Time.deltaTime;
        sweetSpotPosition += sweetSpotVelocity * Time.deltaTime;

        float maxPosition = 1f - sweetSpotSize;

        if (sweetSpotPosition <= 0f || sweetSpotPosition >= maxPosition)
        {
            sweetSpotPosition = Mathf.Clamp(sweetSpotPosition, 0f, maxPosition);
            sweetSpotVelocity = 0f;
        }
    }

    private void MoveFish()
    {
        retargetTimer -= Time.deltaTime;

        if (retargetTimer <= 0f)
        {
            float distance = Mathf.Lerp(dartDistance.x, dartDistance.y, fish.erraticness);

            fishTarget = Mathf.Clamp01(fishPosition + Random.Range(-distance, distance));
            retargetTimer = Mathf.Lerp(retargetInterval.x, retargetInterval.y, fish.erraticness);
        }

        float speed = fish.fishSpeed * stats.fishSpeedMultiplier * fishSpeedScale;

        fishPosition = Mathf.MoveTowards(fishPosition, fishTarget, speed * Time.deltaTime);
    }

    private void UpdateProgress()
    {
        bool inSweetSpot =
            fishPosition >= sweetSpotPosition &&
            fishPosition <= sweetSpotPosition + sweetSpotSize;

        float change = inSweetSpot
            ? progressFillRate * stats.progressFillMultiplier
            : -fish.progressDrainRate;

        progress = Mathf.Clamp01(progress + change * Time.deltaTime);
    }

    private void UpdateUI()
    {
        float barHeight = bar.rect.height;

        SetY(sweetSpot, sweetSpotPosition * barHeight);

        float iconHeight = fishIcon.rect.height * fishIcon.localScale.y;
        SetY(fishIcon, Mathf.Clamp(fishPosition * barHeight - iconHeight / 2f, 0f, barHeight - iconHeight));

        SetY(pressureFill, 0f);
        SetHeight(pressureFill, pressureBar.rect.height * progress);
    }

    private void Finish(bool caught)
    {
        System.Action<bool> callback = finished;

        Cancel();
        callback?.Invoke(caught);
    }

    private void Hide()
    {
        if (minigameRoot != null)
            minigameRoot.SetActive(false);
    }

    private bool HasUI()
    {
        return minigameRoot != null
            && bar != null
            && sweetSpot != null
            && fishIcon != null
            && pressureBar != null
            && pressureFill != null;
    }

    private static void SetY(RectTransform element, float y)
    {
        Vector2 position = element.anchoredPosition;
        position.y = y;
        element.anchoredPosition = position;
    }

    private static void SetHeight(RectTransform element, float height)
    {
        Vector2 size = element.sizeDelta;
        size.y = height / element.localScale.y;
        element.sizeDelta = size;
    }

    private Color GetRarityColor(FishRarity rarity)
    {
        switch (rarity)
        {
            case FishRarity.Trash:     return trashColor;
            case FishRarity.Uncommon:  return uncommonColor;
            case FishRarity.Rare:      return rareColor;
            case FishRarity.Legendary: return legendaryColor;
            default:                   return commonColor;
        }
    }
    }
