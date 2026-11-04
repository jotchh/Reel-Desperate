using UnityEngine;

public enum FishRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}


/*
To create a new fish in unity:
Assets -> Create -> Fishing -> Fish Data
Rename fish and adjust values as needed. Hover over values for tooltip on what they do
*/

[CreateAssetMenu(fileName = "NewFish", menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("Info")]
    public string fishName = "Fish";
    public Sprite sprite;
    public FishRarity rarity = FishRarity.Common;

    [Header("Bite")]
    [Tooltip("Chance (0-1) this fish bites on each bite check. (lower value = rarer)")]   
    [Range(0f, 1f)] public float biteChance = 0.3f;

    [Header("Reel Minigame")]
    [Tooltip("Size of the sweet spot, as a fraction of the bar (lower value = harder)")]
    [Range(0.01f, 1.00f)] public float sweetSpotSize = 0.3f;

    [Tooltip("How quickly the fish moves (higher value = harder)")]
    public float fishSpeed = 1.5f;

    [Tooltip("0 = calm, 1 = erratic movement (higher value = harder)")]
    [Range(0f, 1f)] public float erraticness = 0.3f;

    [Tooltip("Progress lost per second while the fish is outside the sweet spot. (higher value = harder)")]
    public float progressDrainRate = 0.2f;

    [Header("Economy")]
    [Tooltip("Sell price of fish")]
    public float sellPrice = 10f;

    // -1 = not tracked yet, so fish made before rarity defaults keep their values.
    [SerializeField, HideInInspector] private int lastRarity = -1;

    private void OnValidate()
    {
        if (lastRarity != -1 && lastRarity != (int)rarity)
            SetRarityDefaults();

        lastRarity = (int)rarity;
    }

    [ContextMenu("Apply Rarity Defaults")]
    private void ApplyRarityDefaults()
    {
        SetRarityDefaults();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void SetRarityDefaults()
    {
        switch (rarity)
        {
            case FishRarity.Common:
                SetValues(0.3f, 0.3f, 1.5f, 0.3f, 0.2f, 10f);
                break;

            case FishRarity.Uncommon:
                SetValues(0.15f, 0.24f, 1.75f, 0.45f, 0.25f, 25f);
                break;

            case FishRarity.Rare:
                SetValues(0.07f, 0.2f, 1.9f, 0.55f, 0.25f, 60f);
                break;

            case FishRarity.Legendary:
                SetValues(0.03f, 0.15f, 2f, 0.65f, 0.3f, 150f);
                break;
        }
    }

    private void SetValues(
        float biteChance,
        float sweetSpotSize,
        float fishSpeed,
        float erraticness,
        float progressDrainRate,
        float sellPrice)
    {
        this.biteChance = biteChance;
        this.sweetSpotSize = sweetSpotSize;
        this.fishSpeed = fishSpeed;
        this.erraticness = erraticness;
        this.progressDrainRate = progressDrainRate;
        this.sellPrice = sellPrice;
    }
}