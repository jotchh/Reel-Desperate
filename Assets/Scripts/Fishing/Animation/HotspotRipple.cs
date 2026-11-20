using UnityEngine;
using DG.Tweening;

public class HotspotRipple : MonoBehaviour
{
    [Header("Ripple Settings")]
    [SerializeField] private float expandScale = 1.5f;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float startingAlpha = 0.6f;

    private SpriteRenderer spriteRenderer;
    private Tween rippleTween;
    private Vector3 originalScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        gameObject.SetActive(false);
    }

    public void Play()
    {
        gameObject.SetActive(true);

        rippleTween?.Kill();

        transform.localScale = originalScale;
        SetAlpha(startingAlpha);

        rippleTween = DOTween.Sequence()
            .Append(transform.DOScale(originalScale * expandScale, duration))
            .Join(spriteRenderer.DOFade(0f, duration))
            .SetLoops(-1, LoopType.Restart);
    }

    public void Stop()
    {
        rippleTween?.Kill();

        transform.localScale = originalScale;
        SetAlpha(0f);

        gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private void OnDestroy()
    {
        rippleTween?.Kill();
    }
}