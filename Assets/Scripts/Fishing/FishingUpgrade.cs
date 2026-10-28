using UnityEngine;

public enum UpgradeRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

/*
To create a new upgrade in unity:
Assets -> Create -> Fishing -> Upgrade
Rename Upgrade and adjust values as needed. Hover over values for tooltip on what they do
*/

[CreateAssetMenu(fileName = "NewFishingUpgrade", menuName = "Fishing/Upgrade")]
public class FishingUpgrade : ScriptableObject
{
    [Header("Info")]
    public string upgradeName = "Upgrade";
    public Sprite sprite;

    [TextArea] public string description;
    public int cost;

    [Header("Bite")]
    [Tooltip("Multiply bite chance (0.25 = 125%)")]
    public float biteChanceMultiplierBonus;

    [Header("Reel Minigame")]
    [Tooltip("Added to the sweet spot size (fraction of the bar).")]
    public float sweetSpotSizeBonus;

    [Tooltip("Added to the fish speed multiplier. Negative values calm fish down.")]
    public float fishSpeedMultiplierBonus;

    [Tooltip("Added to how strongly holding Reel lifts the sweet spot.")]
    public float reelStrengthBonus;

    [Tooltip("Added to the progress fill multiplier.")]
    public float progressFillMultiplierBonus;

    [Header("Economy")]
    [Tooltip("Buy price of upgrade")]
    public float buyPrice = 10f;
}