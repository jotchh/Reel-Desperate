using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

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


    [Header("Dialogue")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [TextArea(2, 3)]
    [SerializeField]
    private string[] dialogueLines =
    {
    "Quit standing around! Those fish won't catch themselves!",
    "Time is money, and you're wasting mine.",
    "Chop chop! That quota isn't going to meet itself.",
    "The ocean is right there, you know.",
    "Less talking, more fishing!",
    "Every second you spend here is a second you're not making me richer.",
    "If you're done sightseeing, perhaps you could get back to work?"
};


    [Header("Empty Inventory Dialogue")]
    [SerializeField]
    private string[] emptyInventoryLines =
{
    "WOAH, WOAH, WOAH! You have NOTHING to sell?!",
    "You come to MY shop... with EMPTY HANDS?!",
    "What exactly am I supposed to buy? THE AIR?!",
    "I am a BUSINESSMAN, not a charity!",
    "You have NOTHING! NADA! ZERO! GET BACK TO FISHING!",
    "I can't buy your hopes and dreams. BRING ME FISH!"
};

    [Header("Audio")]
    [SerializeField] private AudioSource wizardAudio;
    [SerializeField] private AudioClip menuSound;
    [SerializeField] private AudioClip dialogueSound;


    [Header("Dialogue Typing")]
    [SerializeField] private float letterDelay = 0.04f;
    [SerializeField] private int soundEveryLetters = 3;
    [SerializeField] private float minVoicePitch = 0.8f;
    [SerializeField] private float maxVoicePitch = 1.1f;

    private Coroutine typingCoroutine;


    private int lastDialogueIndex = -1;

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

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
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

        if (RunManager.Instance != null)
        {
            RunManager.Instance.PauseTimer("Wizard");
        }

        Bounce();
        PlaySound(menuSound);

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

    StopDialogueTyping();

    mainPanel.SetActive(false);
    sellPanel.SetActive(true);

    if (dialoguePanel != null)
    {
        dialoguePanel.SetActive(false);
    }

    if (fishShop != null)
    {
        fishShop.RefreshSellPanel();
    }

    PlaySound(menuSound);
}



    public void BackToMain()
    {
        if (!menuOpen)
        {
            return;
        }

        StopDialogueTyping();

        mainPanel.SetActive(true);
        sellPanel.SetActive(false);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        PlaySound(menuSound);
    }


    public void OpenDialogue()
    {
        if (!menuOpen || dialoguePanel == null || dialogueText == null)
        {
            Debug.LogWarning("Wizard: Dialogue UI is missing.");
            return;
        }

        mainPanel.SetActive(false);
        sellPanel.SetActive(false);
        dialoguePanel.SetActive(true);

        NextDialogue();
    }

    public void NextDialogue()
    {
        if (!menuOpen || dialogueText == null ||
            dialogueLines == null || dialogueLines.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, dialogueLines.Length);

        if (dialogueLines.Length > 1 && index == lastDialogueIndex)
        {
            index = (index + 1) % dialogueLines.Length;
        }

        lastDialogueIndex = index;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(
            TypeDialogue(dialogueLines[index])
        );
    }

    private void PlaySound(AudioClip clip)
    {
        if (wizardAudio != null && clip != null)
        {
            wizardAudio.PlayOneShot(clip);
        }
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

    public void BuyBoat()
    {
        if (EconomyManager.Instance == null) return;

        if (EconomyManager.Instance.TryBuyBoat())
        {
            Debug.Log("Player purchased the boat!");
            // TO DO: Play ending anim.
        }
        else
        {
            Debug.Log("You don't have enough money!");
            // TO DO: Display insufficient funds dialogue.
        }
    }

    public void CloseMenu()
    {
        if (!menuOpen)
        {
            return;
        }

        menuOpen = false;
        IsAnyMenuOpen = false;

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ResumeTimer("Wizard");
        }

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

        StopDialogueTyping();
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

        bounceTween = DOTween.Sequence().SetUpdate(true);

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

        StopDialogueTyping();
    }

    private void OnDestroy()
    {
        if (input != null)
        {
            input.Dispose();
        }
    }

    private void StopDialogueTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (wizardAudio != null)
        {
            wizardAudio.pitch = 1f;
        }
    }
    private IEnumerator TypeDialogue(string message)
    {
        dialogueText.text = "";

        int letterCount = 0;

        for (int i = 0; i < message.Length; i++)
        {
            dialogueText.text += message[i];

            if (!char.IsWhiteSpace(message[i]) &&
                !char.IsPunctuation(message[i]))
            {
                letterCount++;

                if (letterCount % soundEveryLetters == 0)
                {
                    if (wizardAudio != null && dialogueSound != null)
                    {
                        wizardAudio.pitch = Random.Range(
                            minVoicePitch,
                            maxVoicePitch
                        );

                        wizardAudio.PlayOneShot(dialogueSound);
                    }
                }
            }

            yield return new WaitForSecondsRealtime(letterDelay);
        }

        if (wizardAudio != null)
        {
            wizardAudio.pitch = 1f;
        }

        typingCoroutine = null;
    }


    public void ShowDialogue(string message)
    {
        if (!menuOpen || dialoguePanel == null || dialogueText == null)
        {
            Debug.LogWarning("Wizard: Dialogue UI is missing.");
            return;
        }

        StopDialogueTyping();

        mainPanel.SetActive(false);
        sellPanel.SetActive(false);
        dialoguePanel.SetActive(true);

        typingCoroutine = StartCoroutine(TypeDialogue(message));
    }

    public void ShowEmptySaleDialogue()
    {
        if (emptyInventoryLines == null || emptyInventoryLines.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, emptyInventoryLines.Length);

        ShowDialogue(emptyInventoryLines[index]);
    }


}
