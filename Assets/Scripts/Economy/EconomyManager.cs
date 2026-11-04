
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Starting Economy")]
    [SerializeField] private int startingMoney = 0;
    [SerializeField] private int startingDailyQuota = 30;
    [SerializeField] private int quotaIncreasePerDay = 15;
    [SerializeField] private int boatPrice = 250;

    public int Money { get; private set; }
    public int TodayEarned { get; private set; }
    public int CurrentDay { get; private set; } = 1;
    public bool IsGameOver { get; private set; }
    public bool HasEscaped { get; private set; }

    public int DailyQuota
    {
        get
        {
            return Mathf.Max(0, startingDailyQuota + (CurrentDay - 1) * quotaIncreasePerDay);
        }
    }

    public int BoatPrice
    {
        get
        {
            return boatPrice;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Money = startingMoney;
    }

    public void SellFish(int sellPrice)
    {
        if (IsGameOver || HasEscaped || sellPrice <= 0)
        {
            return;
        }

        Money += sellPrice;
        TodayEarned += sellPrice;

        Debug.Log("Fish sold for $" + sellPrice + ". Money: $" + Money);
    }

    public bool TrySpendMoney(int cost)
    {
        if (IsGameOver || HasEscaped || cost < 0 || Money < cost)
        {
            return false;
        }

        Money -= cost;

        Debug.Log("Spent $" + cost + ". Money left: $" + Money);
        return true;
    }

    public bool EndDay()
    {
        if (IsGameOver || HasEscaped)
        {
            return false;
        }

        if (TodayEarned < DailyQuota || Money < DailyQuota)
        {
            IsGameOver = true;

            Debug.Log("Quota not paid. The wizard makes you EXPLODE!");

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.SetState(PlayerState.Dead);
            }

            return false;
        }

        Money -= DailyQuota;

        Debug.Log("Paid the wizard $" + DailyQuota + ". Remaining money: $" + Money);

        CurrentDay++;
        TodayEarned = 0;

        Debug.Log("Day " + CurrentDay + " begins. New quota: $" + DailyQuota);
        return true;
    }

    public bool TryBuyBoat()
    {
        if (IsGameOver || HasEscaped || Money < BoatPrice)
        {
            return false;
        }

        Money -= BoatPrice;
        HasEscaped = true;

        Debug.Log("You bought the wizard's boat! You can finally go home!");
        return true;
    }

    [ContextMenu("TEST - Sell $10 Fish")]
    private void TestSellFish()
    {
        SellFish(10);
    }

    [ContextMenu("TEST - Spend $15")]
    private void TestSpendMoney()
    {
        TrySpendMoney(15);
    }

    [ContextMenu("TEST - End Day")]
    private void TestEndDay()
    {
        EndDay();
    }

    [ContextMenu("TEST - Buy Boat")]
    private void TestBuyBoat()
    {
        TryBuyBoat();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

