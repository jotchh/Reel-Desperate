
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[System.Serializable]
public class CaughtFish
{
    public FishData fishData;
    public string fishName;
    public Sprite sprite;
    public int quantity = 1;
}

[RequireComponent(typeof(PlayerFishing))]
public class Inventory : MonoBehaviour
{
    [Header("Fish")]
    [SerializeField] private List<CaughtFish> caughtFish = new List<CaughtFish>();
    [SerializeField] private FishData testFish;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform fishListContent;
    [SerializeField] private GameObject fishRowPrefab;
    [SerializeField] private TMP_Text inventoryValueText;

    private static List<CaughtFish> savedFish = new List<CaughtFish>();

    private PlayerFishing playerFishing;
    private PlayerMovement playerMovement;
    private PlayerManager playerManager;

    private PlayerInput input;
    private InputAction inventoryAction;

    private bool movementWasEnabled;
    private bool fishingWasEnabled;
    private bool isOpen;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    public IReadOnlyList<CaughtFish> Fish
    {
        get { return caughtFish; }
    }

    public int TotalValue
    {
        get
        {
            int total = 0;

            for (int i = 0; i < caughtFish.Count; i++)
            {
                CaughtFish fish = caughtFish[i];

                if (fish != null && fish.fishData != null)
                {
                    int price = Mathf.RoundToInt(fish.fishData.sellPrice);

                    total += price * fish.quantity;
                }
            }

            return total;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        savedFish = new List<CaughtFish>();
    }

    private void Awake()
    {
        playerFishing = GetComponent<PlayerFishing>();
        playerMovement = GetComponent<PlayerMovement>();
        playerManager = GetComponent<PlayerManager>();

        caughtFish = savedFish;

        CombineDuplicateFish();

        input = new PlayerInput();
        inventoryAction = input.Player.Inventory;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (playerFishing != null)
        {
            playerFishing.FishCaught += AddFish;
        }

        inventoryAction.performed += OnInventory;
        input.Player.Enable();
    }

    private void OnDisable()
    {
        if (inventoryAction != null)
        {
            inventoryAction.performed -= OnInventory;
        }

        if (input != null)
        {
            input.Player.Disable();
        }

        if (playerFishing != null)
        {
            playerFishing.FishCaught -= AddFish;
        }

        CloseInventory();
    }

    private void OnDestroy()
    {
        if (input != null)
        {
            input.Dispose();
        }
    }

    private bool SameFish(CaughtFish first, CaughtFish second)
    {
        if (first == null || second == null)
        {
            return false;
        }

        if (first.fishData == null || second.fishData == null)
        {
            return false;
        }

        return first.fishName == second.fishName &&
               first.fishData.rarity == second.fishData.rarity &&
               Mathf.RoundToInt(first.fishData.sellPrice) ==
               Mathf.RoundToInt(second.fishData.sellPrice);
    }

    private void CombineDuplicateFish()
    {
        for (int i = 0; i < caughtFish.Count; i++)
        {
            CaughtFish current = caughtFish[i];

            if (current == null)
            {
                caughtFish.RemoveAt(i);
                i--;
                continue;
            }

            if (current.quantity <= 0)
            {
                caughtFish.RemoveAt(i);
                i--;
                continue;
            }

            for (int j = i + 1; j < caughtFish.Count; j++)
            {
                if (SameFish(current, caughtFish[j]))
                {
                    current.quantity += caughtFish[j].quantity;

                    if (current.sprite == null)
                    {
                        current.sprite = caughtFish[j].sprite;
                    }

                    caughtFish.RemoveAt(j);
                    j--;
                }
            }
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
        newFish.quantity = 1;

        if (string.IsNullOrEmpty(newFish.fishName))
        {
            newFish.fishName = fish.fishName;
        }

        for (int i = 0; i < caughtFish.Count; i++)
        {
            if (SameFish(caughtFish[i], newFish))
            {
                caughtFish[i].quantity++;

                if (caughtFish[i].sprite == null)
                {
                    caughtFish[i].sprite = newFish.sprite;
                }

                Debug.Log(
                    "Caught " + newFish.fishName +
                    " (" + fish.rarity + ") x" +
                    caughtFish[i].quantity
                );

                RefreshInventory();
                return;
            }
        }

        caughtFish.Add(newFish);

        Debug.Log(
            "Caught " + newFish.fishName +
            " (" + fish.rarity + ")!"
        );

        RefreshInventory();
    }

    public string GetFishListText()
    {
        if (caughtFish.Count == 0)
        {
            return "No fish caught yet.";
        }

        string result = "";

        for (int i = 0; i < caughtFish.Count; i++)
        {
            CaughtFish fish = caughtFish[i];

            if (fish == null || fish.fishData == null)
            {
                continue;
            }

            int price = Mathf.RoundToInt(fish.fishData.sellPrice);
            int subtotal = price * fish.quantity;

            result += fish.fishName + " x" + fish.quantity +
                " | $" + price + " x" + fish.quantity +
                " = $" + subtotal + "\n";
        }

        return result;
    }

    private void OnInventory(InputAction.CallbackContext context)
    {
        if (isOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        if (isOpen)
        {
            return;
        }

        if (playerManager == null ||
            playerManager.CurrentState != PlayerState.Alive)
        {
            return;
        }

        if (WizardInteraction.IsAnyMenuOpen)
        {
            return;
        }

        if (inventoryPanel == null ||
            fishListContent == null ||
            fishRowPrefab == null ||
            inventoryValueText == null)
        {
            Debug.LogWarning(
                "Inventory: Missing UI references."
            );

            return;
        }

        movementWasEnabled =
            playerMovement != null && playerMovement.enabled;

        fishingWasEnabled =
            playerFishing != null && playerFishing.enabled;

        if (playerMovement != null)
        {
            playerMovement.StopMovement();
            playerMovement.enabled = false;
        }

        if (playerFishing != null)
        {
            playerFishing.enabled = false;
        }

        isOpen = true;

        RefreshInventory();

        inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = movementWasEnabled;
        }

        if (playerFishing != null)
        {
            playerFishing.enabled = fishingWasEnabled;
        }
    }

    private void ClearDisplayedRows()
    {
        if (fishListContent == null)
        {
            return;
        }

        for (int i = fishListContent.childCount - 1; i >= 0; i--)
        {
            Transform child = fishListContent.GetChild(i);

            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    public void RefreshInventory()
    {
        CombineDuplicateFish();

        ClearDisplayedRows();

        if (fishListContent != null && fishRowPrefab != null)
        {
            for (int i = 0; i < caughtFish.Count; i++)
            {
                CaughtFish fish = caughtFish[i];

                if (fish == null || fish.fishData == null)
                {
                    continue;
                }

                GameObject row = Instantiate(
                    fishRowPrefab,
                    fishListContent
                );

                row.SetActive(true);

                Transform imageObject =
                    row.transform.Find("FishImage");

                Transform nameObject =
                    row.transform.Find("FishNameText");

                Transform priceObject =
                    row.transform.Find("PriceText");

                Transform subtotalObject =
                    row.transform.Find("SubtotalText");

                if (imageObject != null)
                {
                    Image image =
                        imageObject.GetComponent<Image>();

                    if (image != null)
                    {
                        image.sprite = fish.sprite;
                        image.enabled = fish.sprite != null;
                        image.preserveAspect = true;
                    }
                }

                if (nameObject != null)
                {
                    TMP_Text nameText =
                        nameObject.GetComponent<TMP_Text>();

                    if (nameText != null)
                    {
                        nameText.text =
                            fish.fishName + " x" + fish.quantity;
                    }
                }

                int price =
                    Mathf.RoundToInt(fish.fishData.sellPrice);

                int subtotal = price * fish.quantity;

                if (priceObject != null)
                {
                    TMP_Text priceText =
                        priceObject.GetComponent<TMP_Text>();

                    if (priceText != null)
                    {
                        priceText.text =
                            "$" + price + " x" + fish.quantity;
                    }
                }

                if (subtotalObject != null)
                {
                    TMP_Text subtotalText =
                        subtotalObject.GetComponent<TMP_Text>();

                    if (subtotalText != null)
                    {
                        subtotalText.text = "$" + subtotal;
                    }
                }
            }
        }

        if (inventoryValueText != null)
        {
            inventoryValueText.text =
                "Inventory Value: $" + TotalValue;
        }
    }

    public int SellAllFish()
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogWarning(
                "Inventory: EconomyManager is missing."
            );

            return 0;
        }

        if (EconomyManager.Instance.IsGameOver ||
            EconomyManager.Instance.HasEscaped)
        {
            return 0;
        }

        if (caughtFish.Count == 0)
        {
            Debug.Log("No fish to sell.");
            return 0;
        }

        CombineDuplicateFish();

        int total = TotalValue;

        if (total <= 0)
        {
            Debug.Log("Fish have no sell value.");
            return 0;
        }

        EconomyManager.Instance.SellFish(total);

        caughtFish.Clear();

        RefreshInventory();

        Debug.Log("Sold all fish for $" + total);

        return total;
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

        RefreshInventory();

        Debug.Log("Inventory cleared.");
    }
}
