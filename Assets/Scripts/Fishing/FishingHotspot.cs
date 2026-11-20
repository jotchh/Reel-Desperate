using UnityEngine;

public class FishingHotspot : MonoBehaviour
{
    [Header("Hotspot")]
    [SerializeField] private float radius = 3f;

    [Header("Activation Timing")]
    [SerializeField] private Vector2 activeDuration = new Vector2(20f, 40f);
    [SerializeField] private Vector2 inactiveDuration = new Vector2(10f, 25f);

    [Header("Rarity Multipliers")]
    [SerializeField] private float commonMultiplier = 1f;
    [SerializeField] private float uncommonMultiplier = 1f;
    [SerializeField] private float rareMultiplier = 2f;
    [SerializeField] private float legendaryMultiplier = 3f;

    [Header("Visual")]
    [SerializeField] private HotspotRipple ripple;

    public bool IsActive { get; private set; }

    private float timer;

    private void Start()
    {
        // Randomize initial state
        SetActive(Random.value < 0.5f);
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SetActive(!IsActive);
        }
    }

    private void SetActive(bool active)
    {
        IsActive = active;

        // Set timer based on current state
        Vector2 duration = IsActive ? activeDuration : inactiveDuration;
        timer = Random.Range(duration.x, duration.y);

        // Control ripple effect
        if (ripple != null)
        {
            if (IsActive)
                ripple.Play();
            else
                ripple.Stop();
        }

        Debug.Log($"{gameObject.name} is now {(IsActive ? "ACTIVE" : "INACTIVE")}!");
    }

    public float GetMultiplier(FishRarity rarity)
    {
        if (!IsActive)
            return 1f;

        switch (rarity)
        {
            case FishRarity.Trash:
                return 1f;

            case FishRarity.Common:
                return commonMultiplier;

            case FishRarity.Uncommon:
                return uncommonMultiplier;

            case FishRarity.Rare:
                return rareMultiplier;

            case FishRarity.Legendary:
                return legendaryMultiplier;

            default:
                return 1f;
        }
    }

    public bool IsPlayerInRange(Vector2 playerPosition)
    {
        return IsActive &&
               Vector2.Distance(transform.position, playerPosition) <= radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsActive ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}