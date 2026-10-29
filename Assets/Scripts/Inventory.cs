
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerFishing))]
public class Inventory : MonoBehaviour
{
    [SerializeField] private List<FishData> caughtFish = new List<FishData>();
    [SerializeField] private FishData testFish;

    private PlayerFishing playerFishing;

    private void Awake()
    {
        playerFishing = GetComponent<PlayerFishing>();
    }

    private void OnEnable()
    {
        if (playerFishing != null)
        {
            playerFishing.FishCaught += AddFish;
        }
    }

    private void OnDisable()
    {
        if (playerFishing != null)
        {
            playerFishing.FishCaught -= AddFish;
        }
    }

    public void AddFish(FishData fish)
    {
        if (fish == null)
        {
            return;
        }

        caughtFish.Add(fish);
        Debug.Log("Added " + fish.fishName + " to inventory. Fish count: " + caughtFish.Count);
    }

    public void SellAllFish()
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogWarning("EconomyManager is missing from the scene.");
            return;
        }

        if (EconomyManager.Instance.IsGameOver || EconomyManager.Instance.HasEscaped)
        {
            Debug.Log("You cannot sell fish after the game has ended.");
            return;
        }

        if (caughtFish.Count == 0)
        {
            Debug.Log("There are no fish to sell.");
            return;
        }

        int totalPrice = 0;

        for (int i = 0; i < caughtFish.Count; i++)
        {
            if (caughtFish[i] != null)
            {
                totalPrice += Mathf.RoundToInt(caughtFish[i].sellPrice);
            }
        }

        if (totalPrice <= 0)
        {
            Debug.Log("These fish have no sell value.");
            return;
        }

        EconomyManager.Instance.SellFish(totalPrice);
        caughtFish.Clear();

        Debug.Log("Sold all fish for $" + totalPrice);
    }

    [ContextMenu("TEST - Add Fish")]
    private void TestAddFish()
    {
        AddFish(testFish);
    }

    [ContextMenu("TEST - Sell All Fish")]
    private void TestSellAllFish()
    {
        SellAllFish();
    }
}
