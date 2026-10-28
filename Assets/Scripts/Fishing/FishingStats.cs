using UnityEngine;

public class FishingStats : MonoBehaviour
{
    [Header("Bite")]
    public float biteChanceMultiplier = 1f;

    [Header("Reel Minigame")]
    public float sweetSpotSizeBonus = 0f;
    public float fishSpeedMultiplier = 1f;
    public float reelStrength = 3f;
    public float progressFillMultiplier = 1f;

    public void ApplyUpgrade(FishingUpgrade upgrade)
    {
        if (upgrade == null)
            return;

        biteChanceMultiplier += upgrade.biteChanceMultiplierBonus;
        sweetSpotSizeBonus += upgrade.sweetSpotSizeBonus;
        fishSpeedMultiplier += upgrade.fishSpeedMultiplierBonus;
        reelStrength += upgrade.reelStrengthBonus;
        progressFillMultiplier += upgrade.progressFillMultiplierBonus;
    }
}