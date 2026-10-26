
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float friction = 5f;

    [Header("Sprites")]
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;

    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private PlayerManager playerManager;
    private PlayerAnimation playerAnimation;

    private PlayerInput input;
    private InputAction moveAction;

    private Vector2 moveDirection;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerManager = GetComponent<PlayerManager>();
        playerAnimation = GetComponent<PlayerAnimation>();

        input = new PlayerInput();
        moveAction = input.Player.Move;

        if (spriteRenderer != null && frontSprite == null)
        {
            frontSprite = spriteRenderer.sprite;
        }
    }

    private void OnEnable()
    {
        input?.Player.Enable();
    }

    private void OnDisable()
    {
        input?.Player.Disable();

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (rigidBody == null || playerManager == null)
            return;

        if (playerManager.CurrentState != PlayerState.Alive)
        {
            StopMovement();
            return;
        }

        moveDirection = moveAction.ReadValue<Vector2>().normalized;

        Move();
        UpdateSprite();

        if (playerAnimation != null)
        {
            playerAnimation.HandleMovement(moveDirection);
        }
    }

    private void Move()
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            rigidBody.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            rigidBody.linearVelocity = Vector2.Lerp(
                rigidBody.linearVelocity,
                Vector2.zero,
                friction * Time.fixedDeltaTime
            );
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null)
            return;

        if (moveDirection.y > 0.01f)
        {
            spriteRenderer.sprite = backSprite;
        }
        else if (moveDirection.y < -0.01f)
        {
            spriteRenderer.sprite = frontSprite;
        }
    }

    public void StopMovement()
    {
        moveDirection = Vector2.zero;

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.zero;
        }

        if (playerAnimation != null)
        {
            playerAnimation.HandleMovement(Vector2.zero);
        }
    }

    private void OnDestroy()
    {
        input?.Dispose();
    }
}
