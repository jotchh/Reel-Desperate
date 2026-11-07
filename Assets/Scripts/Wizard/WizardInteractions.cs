
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class WizardInteraction : MonoBehaviour
{
    [Header("Wizard")]
    [SerializeField] private Transform wizardVisual;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private GameObject talkPrompt;

    [Header("Shop")]
    [SerializeField] private GameObject wizardMenu;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject sellPanel;
    [SerializeField] private FishShop fishShop;
    [SerializeField] private string upgradeSceneName = "WizardShop";

    [Header("Bounce")]
    [SerializeField] private float bounceDuration = 0.12f;
    [SerializeField] private float bounceAmount = 0.1f;
    [SerializeField] private float waddleAngle = 8f;

    public static bool IsAnyMenuOpen { get; private set; }

    private PlayerInput input;
    private InputAction talkAction;

    private PlayerMovement playerMovement;
    private PlayerFishing playerFishing;

    private bool movementWasEnabled;
    private bool fishingWasEnabled;
    private bool menuOpen;

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Sequence bounceTween;

    public void TestButton()
    {
        Debug.Log("BUTTON CLICKED!");
    }

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

        if (wizardMenu != null)
        {
            wizardMenu.SetActive(false);
        }

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
            talkPrompt.SetActive(!menuOpen && CanTalk());
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

        Inventory inventory = PlayerManager.Instance.GetComponent<Inventory>();

        if (inventory != null && inventory.IsOpen)
        {
            return false;
        }

        float distance = Vector2.Distance(
            PlayerManager.Instance.transform.position,
            wizardVisual.position
        );

        return distance <= interactionDistance;
    }

    private void OnTalk(InputAction.CallbackContext context)
    {
        if (menuOpen)
        {
            CloseMenu();
        }
        else if (!IsAnyMenuOpen && CanTalk())
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (menuOpen || IsAnyMenuOpen || !CanTalk())
        {
            return;
        }

        if (wizardMenu == null || mainPanel == null || sellPanel == null)
        {
            Debug.LogWarning("Wizard: Menu panels are missing.");
            return;
        }

        PlayerManager player = PlayerManager.Instance;

        playerMovement = player.GetComponent<PlayerMovement>();
        playerFishing = player.GetComponent<PlayerFishing>();

        movementWasEnabled = playerMovement != null && playerMovement.enabled;
        fishingWasEnabled = playerFishing != null && playerFishing.enabled;

        if (playerMovement != null)
        {
            playerMovement.StopMovement();
            playerMovement.enabled = false;
        }

        if (playerFishing != null)
        {
            playerFishing.enabled = false;
        }

        menuOpen = true;
        IsAnyMenuOpen = true;

        Bounce();

        wizardMenu.SetActive(true);
        BackToMain();

        if (talkPrompt != null)
        {
            talkPrompt.SetActive(false);
        }
    }

    public void OpenSellPanel()
    {
        if (!menuOpen)
        {
            return;
        }

        mainPanel.SetActive(false);
        sellPanel.SetActive(true);

        if (fishShop != null)
        {
            fishShop.RefreshSellPanel();
        }
    }

    public void BackToMain()
    {
        if (!menuOpen)
        {
            return;
        }

        mainPanel.SetActive(true);
        sellPanel.SetActive(false);
    }

    public void OpenUpgradesScene()
    {
        if (!menuOpen)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(upgradeSceneName))
        {
            Debug.LogWarning("WizardShop scene is not in the build list.");
            return;
        }

        CloseMenu();
        SceneManager.LoadScene(upgradeSceneName);
    }

    public void CloseMenu()
    {
        if (!menuOpen)
        {
            return;
        }

        menuOpen = false;
        IsAnyMenuOpen = false;

        if (wizardMenu != null)
        {
            wizardMenu.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = movementWasEnabled;
        }

        if (playerFishing != null)
        {
            playerFishing.enabled = fishingWasEnabled;
        }

        playerMovement = null;
        playerFishing = null;
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
            wizardVisual.DOScale(originalScale, bounceDuration)
                .SetEase(Ease.OutBack)
        );

        bounceTween.Join(
            wizardVisual.DOLocalRotate(originalRotation, bounceDuration)
        );
    }

    private void OnDisable()
    {
        talkAction.performed -= OnTalk;
        input.Player.Disable();

        CloseMenu();

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
