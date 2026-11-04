
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CaughtFish
{
    public FishData fishData;
    public string fishName;
    public Sprite sprite;
}

[RequireComponent(typeof(PlayerFishing))]
public class Inventory : MonoBehaviour
{
    [SerializeField] private List<CaughtFish> caughtFish = new List<CaughtFish>();
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

        FishAppearance appearance = fish.GetRandomAppearance();

        CaughtFish newFish = new CaughtFish();

        newFish.fishData = fish;
        newFish.fishName = appearance.fishName;
        newFish.sprite = appearance.sprite;

        caughtFish.Add(newFish);

        Debug.Log("Caught " + newFish.fishName + " (" + fish.rarity + ")!");
        Debug.Log("Inventory count: " + caughtFish.Count);
    }

    public void SellAllFish()
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogWarning("EconomyManager is missing.");
            return;
        }

        if (EconomyManager.Instance.IsGameOver || EconomyManager.Instance.HasEscaped)
        {
            return;
        }

        if (caughtFish.Count == 0)
        {
            Debug.Log("No fish to sell.");
            return;
        }

        int totalPrice = 0;

        for (int i = 0; i < caughtFish.Count; i++)
        {
            if (caughtFish[i] != null && caughtFish[i].fishData != null)
            {
                int price = Mathf.RoundToInt(caughtFish[i].fishData.sellPrice);

                totalPrice += price;

                Debug.Log(caughtFish[i].fishName + " sold for $" + price);
            }
        }

        if (totalPrice <= 0)
        {
            Debug.Log("Fish have no sell value.");
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

    [ContextMenu("TEST - Clear Inventory")]
    private void TestClearInventory()
    {
        caughtFish.Clear();
        Debug.Log("Inventory cleared.");
    }
}
