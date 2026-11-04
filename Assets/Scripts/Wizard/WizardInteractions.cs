using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class WizardInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform wizardVisual;
    [SerializeField] private GameObject talkPrompt;
    [SerializeField] private FishShop fishShop;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 2f;

    [Header("Bounce")]
    [SerializeField] private float bounceDuration = 0.12f;
    [SerializeField] private float bounceAmount = 0.1f;
    [SerializeField] private float waddleAngle = 8f;

    private PlayerInput input;
    private InputAction talkAction;

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Sequence bounceTween;

    private void Awake()
    {
        input = new PlayerInput();
        talkAction = input.Player.Talk;

        if (wizardVisual == null)
        {
            wizardVisual = transform;
        }

        if (fishShop == null)
        {
            fishShop = GetComponent<FishShop>();
        }

        originalScale = wizardVisual.localScale;
        originalRotation = wizardVisual.localEulerAngles;

        if (talkPrompt != null)
        {
            talkPrompt.SetActive(false);
        }
    }

    private void OnEnable()
    {
        talkAction.performed += OnTalk;
        input.Player.Enable();
    }

    private void Update()
    {
        if (talkPrompt != null)
        {
            talkPrompt.SetActive(CanTalk());
        }
    }

    private bool CanTalk()
    {
        if (PlayerManager.Instance == null)
        {
            return false;
        }

        if (PlayerManager.Instance.CurrentState != PlayerState.Alive)
        {
            return false;
        }

        Vector3 playerPosition = PlayerManager.Instance.transform.position;
        Vector3 wizardPosition = wizardVisual.position;

        float distance = Vector2.Distance(playerPosition, wizardPosition);

        return distance <= interactionDistance;
    }

    private void OnTalk(InputAction.CallbackContext context)
    {
        if (!CanTalk())
        {
            return;
        }

        Bounce();

        if (fishShop != null)
        {
            fishShop.SellAll();
        }
        else
        {
            Debug.LogWarning("Wizard: FishShop is missing.");
        }
    }

    private void Bounce()
    {
        if (bounceTween != null)
        {
            bounceTween.Kill();
        }

        wizardVisual.localScale = originalScale;
        wizardVisual.localEulerAngles = originalRotation;

        Vector3 squash = new Vector3(
            originalScale.x * (1f + bounceAmount),
            originalScale.y * (1f - bounceAmount),
            originalScale.z
        );

        Vector3 stretch = new Vector3(
            originalScale.x * (1f - bounceAmount),
            originalScale.y * (1f + bounceAmount),
            originalScale.z
        );

        bounceTween = DOTween.Sequence();

        bounceTween.Append(wizardVisual.DOScale(squash, bounceDuration));

        bounceTween.Join(
            wizardVisual.DOLocalRotate(
                originalRotation + new Vector3(0f, 0f, waddleAngle),
                bounceDuration
            )
        );

        bounceTween.Append(wizardVisual.DOScale(stretch, bounceDuration));

        bounceTween.Join(
            wizardVisual.DOLocalRotate(
                originalRotation + new Vector3(0f, 0f, -waddleAngle),
                bounceDuration
            )
        );

        bounceTween.Append(
            wizardVisual.DOScale(originalScale, bounceDuration).SetEase(Ease.OutBack)
        );

        bounceTween.Join(
            wizardVisual.DOLocalRotate(originalRotation, bounceDuration)
        );
    }

    private void OnDisable()
    {
        if (talkAction != null)
        {
            talkAction.performed -= OnTalk;
        }

        if (input != null)
        {
            input.Player.Disable();
        }

        if (bounceTween != null)
        {
            bounceTween.Kill();
        }

        if (wizardVisual != null)
        {
            wizardVisual.localScale = originalScale;
            wizardVisual.localEulerAngles = originalRotation;
        }

        if (talkPrompt != null)
        {
            talkPrompt.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (input != null)
        {
            input.Dispose();
        }
    }
}
