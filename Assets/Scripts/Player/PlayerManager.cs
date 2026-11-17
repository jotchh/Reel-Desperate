
using UnityEngine;
using DG.Tweening;

public enum PlayerState
{
    Alive,
    Fishing,
    Dead
}

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerFishing))]
[RequireComponent(typeof(PlayerAnimation))]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public PlayerState CurrentState { get; private set; } = PlayerState.Alive;

    [Header("Explosion")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] explosionFrames;
    [SerializeField] private float frameDuration = 0.08f;

    private PlayerMovement movement;
    private PlayerFishing fishing;
    private PlayerAnimation playerAnimation;

    private Sequence explosionSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        movement = GetComponent<PlayerMovement>();
        fishing = GetComponent<PlayerFishing>();
        playerAnimation = GetComponent<PlayerAnimation>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;

        if (newState != PlayerState.Alive)
        {
            movement.StopMovement();
        }

        if (newState == PlayerState.Dead)
        {
            Debug.Log("Player exploded!");

            fishing.EndFishingForDay();
            PlayExplosion();
        }
    }

    private void PlayExplosion()
    {
        if (spriteRenderer == null || explosionFrames == null || explosionFrames.Length == 0)
        {
            Debug.LogWarning("Explosion sprites are missing!");
            return;
        }

        explosionSequence?.Kill();

        explosionSequence = DOTween.Sequence();

        foreach (Sprite frame in explosionFrames)
        {
            Sprite currentFrame = frame;

            explosionSequence.AppendCallback(() =>
            {
                spriteRenderer.sprite = currentFrame;
            });

            explosionSequence.AppendInterval(frameDuration);
        }

        explosionSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void OnDestroy()
    {
        explosionSequence?.Kill();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
