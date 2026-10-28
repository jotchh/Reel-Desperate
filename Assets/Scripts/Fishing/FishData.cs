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
    [Range(0f, 1f)] public float biteChance = 0.2f;

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
}