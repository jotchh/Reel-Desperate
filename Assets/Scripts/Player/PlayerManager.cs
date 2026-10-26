
using UnityEngine;

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

    private PlayerMovement movement;
    private PlayerFishing fishing;
    private PlayerAnimation playerAnimation;

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
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState)
            return;

        if (CurrentState == PlayerState.Dead)
            return;

        CurrentState = newState;

        if (newState != PlayerState.Alive)
        {
            movement.StopMovement();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
