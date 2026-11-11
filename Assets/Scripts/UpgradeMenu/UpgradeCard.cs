
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UpgradeCard : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text upgradeNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image upgradeImage;
    [SerializeField] private Button button;

    public AudioSource audioSource;
    public AudioClip onClickClip;

    [Header("Hover Animation")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float animationDuration = 0.2f;

    private FishingUpgrade upgrade;
    private Tween scaleTween;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetUpgrade(FishingUpgrade upgrade)
    {
        if (upgrade == null)
            return;

        this.upgrade = upgrade;

        upgradeNameText.text = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
        upgradeImage.sprite = upgrade.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        scaleTween?.Kill();

        scaleTween = transform.DOScale(
            originalScale * hoverScale,
            animationDuration
        ).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scaleTween?.Kill();

        scaleTween = transform.DOScale(
            originalScale,
            animationDuration
        ).SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }
}
