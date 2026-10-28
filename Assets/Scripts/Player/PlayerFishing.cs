using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(FishingStats))]
public class PlayerFishing : MonoBehaviour
{
    private enum FishingPhase
    {
        None,
        Waiting,
        Bite,
        Reeling
    }

    [Header("Ground Tilemap")]
    [SerializeField] private Tilemap groundTilemap;

    [Header("Fishing Rod Objects")]
    [SerializeField] private GameObject rodUp;
    [SerializeField] private GameObject rodDown;
    [SerializeField] private GameObject rodLeft;
    [SerializeField] private GameObject rodRight;

    [Header("Fish")]
    [SerializeField] private FishData[] fishPool;

    [Header("Bite Timing")]
    [SerializeField] private float minWaitTime = 1.5f;
    [SerializeField] private float biteCheckInterval = 1f;
    [SerializeField] private float reactionWindow = 0.8f;

    [Header("Bite Indicator")]
    [SerializeField] private GameObject biteIndicator;

    [Header("Reel Minigame")]
    [SerializeField] private FishingMinigame minigame;

    public event System.Action<FishData> FishCaught;

    private PlayerManager playerManager;
    private PlayerAnimation playerAnimation;
    private FishingStats stats;

    private PlayerInput input;
    private InputAction interactAction;
    private InputAction reelAction;

    private Vector3Int waterDirection;

    private FishingPhase phase = FishingPhase.None;
    private FishData hookedFish;
    private Coroutine biteRoutine;
    private Vector3 biteIndicatorScale = Vector3.one;

    private readonly Vector3Int[] directions =
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        playerAnimation = GetComponent<PlayerAnimation>();
        stats = GetComponent<FishingStats>();

        input = new PlayerInput();
        interactAction = input.Player.Interact;
        reelAction = input.Player.Reel;

        HideAllRods();
        HideBiteIndicator();
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.performed += OnInteract;
            reelAction.performed += OnReel;
            input.Player.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
            reelAction.performed -= OnReel;
            input?.Player.Disable();
        }

        StopFishing();
    }

    private void OnDestroy()
    {
        input?.Dispose();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (playerManager == null)
            return;

        switch (playerManager.CurrentState)
        {
            case PlayerState.Alive:
                if (CanFish())
                    StartFishing();
                break;

            case PlayerState.Fishing:
                // Don't let Interact cancel mid-minigame.
                if (phase != FishingPhase.Reeling)
                    StopFishing();
                break;

            case PlayerState.Dead:
                StopFishing();
                break;
        }
    }

    private void OnReel(InputAction.CallbackContext context)
    {
        if (phase == FishingPhase.Bite)
            HookFish();
    }

    private void StartFishing()
    {
        if (fishPool == null || fishPool.Length == 0)
            Debug.LogWarning("PlayerFishing: Fish Pool is empty, nothing will bite!");

        playerManager.SetState(PlayerState.Fishing);

        Vector2 direction = new Vector2(
            waterDirection.x,
            waterDirection.y
        );

        // Force the player to face the water.
        playerAnimation?.FaceDirection(direction);
        ShowRod(direction);

        phase = FishingPhase.Waiting;
        biteRoutine = StartCoroutine(BiteLoop());
    }

    private void StopFishing()
    {
        if (biteRoutine != null)
        {
            StopCoroutine(biteRoutine);
            biteRoutine = null;
        }

        if (minigame != null && minigame.IsRunning)
            minigame.Cancel();

        phase = FishingPhase.None;
        hookedFish = null;

        HideBiteIndicator();
        HideAllRods();

        if (playerManager != null && playerManager.CurrentState == PlayerState.Fishing)
            playerManager.SetState(PlayerState.Alive);
    }

    private IEnumerator BiteLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(minWaitTime);

            FishData fish = null;

            while (fish == null)
            {
                yield return new WaitForSeconds(biteCheckInterval);
                fish = RollForBite();
            }

            hookedFish = fish;
            phase = FishingPhase.Bite;
            ShowBiteIndicator();

            yield return new WaitForSeconds(reactionWindow);

            // Player didn't click in time; HookFish() stops this coroutine if they did.
            Debug.Log($"The {fish.fishName} got away before you could hook it.");

            hookedFish = null;
            phase = FishingPhase.Waiting;
            HideBiteIndicator();
        }
    }

    private FishData RollForBite()
    {
        if (fishPool == null)
            return null;

        List<FishData> bites = new List<FishData>();

        foreach (FishData fish in fishPool)
        {
            if (fish != null && Random.value < fish.biteChance * stats.biteChanceMultiplier)
                bites.Add(fish);
        }

        if (bites.Count == 0)
            return null;

        return bites[Random.Range(0, bites.Count)];
    }

    private void HookFish()
    {
        if (biteRoutine != null)
        {
            StopCoroutine(biteRoutine);
            biteRoutine = null;
        }

        HideBiteIndicator();
        phase = FishingPhase.Reeling;

        if (minigame == null)
        {
            Debug.LogWarning("PlayerFishing: Minigame is not assigned! Catching instantly.");
            OnMinigameFinished(true);
            return;
        }

        minigame.Begin(hookedFish, stats, () => reelAction.IsPressed(), OnMinigameFinished);
    }

    private void OnMinigameFinished(bool caught)
    {
        FishData fish = hookedFish;

        if (caught)
        {
            Debug.Log($"Caught a {fish.fishName}!");
            FishCaught?.Invoke(fish);
        }
        else
        {
            Debug.Log($"The {fish.fishName} escaped!");
        }

        StopFishing();
    }

    private void ShowBiteIndicator()
    {
        if (biteIndicator == null)
            return;

        biteIndicator.SetActive(true);

        Transform indicator = biteIndicator.transform;
        indicator.DOKill();
        indicator.localScale = biteIndicatorScale;
        indicator.DOPunchScale(biteIndicatorScale * 0.5f, 0.3f, 8, 0.5f);
    }

    private void HideBiteIndicator()
    {
        if (biteIndicator == null)
            return;

        biteIndicator.transform.DOKill();
        biteIndicator.transform.localScale = biteIndicatorScale;
        biteIndicator.SetActive(false);
    }

    private void ShowRod(Vector2 direction)
    {
        HideAllRods();

        if (direction.y > 0)
        {
            if (rodUp != null)
                rodUp.SetActive(true);
        }
        else if (direction.y < 0)
        {
            if (rodDown != null)
                rodDown.SetActive(true);
        }
        else if (direction.x < 0)
        {
            if (rodLeft != null)
                rodLeft.SetActive(true);
        }
        else if (direction.x > 0)
        {
            if (rodRight != null)
                rodRight.SetActive(true);
        }
    }

    private void HideAllRods()
    {
        if (rodUp != null)
            rodUp.SetActive(false);

        if (rodDown != null)
            rodDown.SetActive(false);

        if (rodLeft != null)
            rodLeft.SetActive(false);

        if (rodRight != null)
            rodRight.SetActive(false);
    }

    private bool CanFish()
    {
        if (groundTilemap == null)
        {
            Debug.LogWarning(
                "PlayerFishing: Ground Tilemap is not assigned!"
            );

            return false;
        }

        Vector3Int playerCell =
            groundTilemap.WorldToCell(transform.position);

        foreach (Vector3Int direction in directions)
        {
            Vector3Int neighborCell = playerCell + direction;

            // An adjacent cell without a ground tile is treated as water. For now.
            if (!groundTilemap.HasTile(neighborCell))
            {
                waterDirection = direction;
                return true;
            }
        }

        return false;
    }
}