
using System.Collections.Generic;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UpgradeCard[] cards;

    [Header("Available Upgrades")]
    [SerializeField] private FishingUpgrade[] upgrades;

    private FishingStats fishingStats;
    private bool upgradeSelected;

    private void Start()
    {
        // Get the player's FishingStats component.
        if (PlayerManager.Instance != null)
            fishingStats = PlayerManager.Instance.GetComponent<FishingStats>();

        if (fishingStats == null)
        {
            Debug.LogError("Could not find the player's FishingStats!");
            return;
        }

        GenerateCards();
    }

    private void GenerateCards()
    {
        List<FishingUpgrade> available = new List<FishingUpgrade>(upgrades);

        // Shuffle upgrades to randomize selection.
        for (int i = 0; i < available.Count; i++)
        {
            int randomIndex = Random.Range(i, available.Count);

            FishingUpgrade temp = available[i];
            available[i] = available[randomIndex];
            available[randomIndex] = temp;
        }

        // Populate cards.
        for (int i = 0; i < cards.Length; i++)
        {
            if (i < available.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].SetUpgrade(available[i], SelectUpgrade);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectUpgrade(FishingUpgrade upgrade)
    {
        if (upgradeSelected || upgrade == null)
            return;

        if (RunManager.Instance == null || fishingStats == null)
        {
            Debug.LogError("Cannot apply upgrade: missing RunManager or FishingStats.");
            return;
        }

        upgradeSelected = true;

        // Apply the selected upgrade to the player.
        fishingStats.ApplyUpgrade(upgrade);

        Debug.Log("Selected upgrade: " + upgrade.upgradeName);

        // Close menu and begin the next day.
        RunManager.Instance.StartNextDay();
    }
}
