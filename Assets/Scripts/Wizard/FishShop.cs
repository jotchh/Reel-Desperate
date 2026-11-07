
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishShop : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform fishListContent;
    [SerializeField] private GameObject fishRowPrefab;
    [SerializeField] private TMP_Text totalSellText;

    public void RefreshSellPanel()
    {
        if (inventory == null && PlayerManager.Instance != null)
        {
            inventory = PlayerManager.Instance.GetComponent<Inventory>();
        }

        if (inventory == null || fishListContent == null || fishRowPrefab == null)
        {
            Debug.LogWarning("FishShop: Missing references.");
            return;
        }

        for (int i = fishListContent.childCount - 1; i >= 0; i--)
        {
            GameObject row = fishListContent.GetChild(i).gameObject;
            row.SetActive(false);
            Destroy(row);
        }

        for (int i = 0; i < inventory.Fish.Count; i++)
        {
            CaughtFish fish = inventory.Fish[i];

            if (fish == null || fish.fishData == null)
            {
                continue;
            }

            GameObject row = Instantiate(fishRowPrefab, fishListContent);
            row.SetActive(true);

            Image image = row.transform.Find("FishImage").GetComponent<Image>();
            TMP_Text nameText = row.transform.Find("FishNameText").GetComponent<TMP_Text>();
            TMP_Text priceText = row.transform.Find("PriceText").GetComponent<TMP_Text>();
            TMP_Text subtotalText = row.transform.Find("SubtotalText").GetComponent<TMP_Text>();

            int price = Mathf.RoundToInt(fish.fishData.sellPrice);

            image.sprite = fish.sprite;
            image.enabled = fish.sprite != null;
            image.preserveAspect = true;

            nameText.text = fish.fishName + " x" + fish.quantity;
            priceText.text = "$" + price + " x" + fish.quantity;
            subtotalText.text = "$" + (price * fish.quantity);
        }

        if (totalSellText != null)
        {
            totalSellText.text = "Total Sell Price: $" + inventory.TotalValue;
        }
    }

    public void SellAll()
    {
        if (inventory == null && PlayerManager.Instance != null)
        {
            inventory = PlayerManager.Instance.GetComponent<Inventory>();
        }

        if (inventory == null)
        {
            return;
        }

        int total = inventory.SellAllFish();

        if (total > 0)
        {
            Debug.Log("Sold fish for $" + total);
        }

        RefreshSellPanel();
    }
}
