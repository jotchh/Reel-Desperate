
using System.Collections.Generic;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UpgradeCard[] cards;

    [Header("Available Upgrades")]
    [SerializeField] private FishingUpgrade[] upgrades;

    private void Start()
    {
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
                cards[i].SetUpgrade(available[i]);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }
}
