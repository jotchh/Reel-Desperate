
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerFishing : MonoBehaviour
{
    [Header("Ground Tilemap")]
    [SerializeField] private Tilemap groundTilemap;

    [Header("Fishing Rod Objects")]
    [SerializeField] private GameObject rodUp;
    [SerializeField] private GameObject rodDown;
    [SerializeField] private GameObject rodLeft;
    [SerializeField] private GameObject rodRight;

    private PlayerManager playerManager;
    private PlayerAnimation playerAnimation;

    private PlayerInput input;
    private InputAction interactAction;

    private Vector3Int waterDirection;

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

        input = new PlayerInput();
        interactAction = input.Player.Interact;

        HideAllRods();
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.performed += OnInteract;
            input.Player.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
            input?.Player.Disable();
        }

        HideAllRods();
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
                {
                    playerManager.SetState(PlayerState.Fishing);

                    Vector2 direction = new Vector2(
                        waterDirection.x,
                        waterDirection.y
                    );

                    // Force the player to face the water.
                    playerAnimation?.FaceDirection(direction);
                    ShowRod(direction);
                }
                break;

            case PlayerState.Fishing:
                playerManager.SetState(PlayerState.Alive);
                HideAllRods();
                break;

            case PlayerState.Dead:
                HideAllRods();
                break;
        }
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
