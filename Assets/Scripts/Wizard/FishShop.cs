using UnityEngine;

public class FishShop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;

    private void Start()
    {
        if (inventory == null && PlayerManager.Instance != null)
        {
            inventory = PlayerManager.Instance.GetComponent<Inventory>();
        }
    }

    public void SellAll()
    {
        if (inventory == null && PlayerManager.Instance != null)
        {
            inventory = PlayerManager.Instance.GetComponent<Inventory>();
        }

        if (inventory == null || EconomyManager.Instance == null)
        {
            Debug.LogWarning("FishShop: Inventory or EconomyManager is missing.");
            return;
        }

        int moneyBefore = EconomyManager.Instance.Money;

        inventory.SellAllFish();

        int moneyReceived = EconomyManager.Instance.Money - moneyBefore;

        if (moneyReceived > 0)
        {
            Debug.Log("Sold fish for $" + moneyReceived + "!");
        }
        else
        {
            Debug.Log("No fish sold.");
        }
    }
}
