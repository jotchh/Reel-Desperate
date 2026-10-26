
using UnityEngine;
using DG.Tweening;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Movement Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite backSprite;

    [Header("Waddle Animation")]
    [SerializeField] private float waddleAngle = 12f;
    [SerializeField] private float waddleDuration = 0.2f;
    [SerializeField] private float idleReturnDuration = 0.2f;

    [Header("Squash and Stretch")]
    [SerializeField] private float stretchAmount = 0.08f;
    [SerializeField] private float stretchDuration = 0.15f;
    [SerializeField] private float idleSquash = 0.03f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Tween rotationTween;
    private Tween scaleTween;

    private bool wasMoving;
    private int lastMoveAxis = -1;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalRotation = transform.localRotation;
        originalScale = transform.localScale;

        if (idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    public void HandleMovement(Vector2 moveDirection)
    {
        bool isMoving = moveDirection.sqrMagnitude > 0.01f;

        // Face left or right.
        if (moveDirection.x < -0.01f)
            spriteRenderer.flipX = true;
        else if (moveDirection.x > 0.01f)
            spriteRenderer.flipX = false;

        if (isMoving)
        {
            // Change sprite based on vertical movement.
            if (moveDirection.y > 0.01f && backSprite != null)
            {
                spriteRenderer.sprite = backSprite;
            }
            else if (moveDirection.y < -0.01f && idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
            else if (Mathf.Abs(moveDirection.x) > 0.01f
                     && idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }

            int moveAxis =
                Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y)
                ? 0
                : 1;

            if (!wasMoving)
            {
                StartWaddle();
                StartSquashAndStretch(moveAxis);
            }
            else if (moveAxis != lastMoveAxis)
            {
                StartSquashAndStretch(moveAxis);
            }
        }
        else if (wasMoving)
        {
            StopAnimations();
        }

        wasMoving = isMoving;
    }

    private void StartWaddle()
    {
        rotationTween?.Kill();

        float baseAngle = originalRotation.eulerAngles.z;

        rotationTween = DOTween.Sequence()
            .Append(transform.DOLocalRotate(
                new Vector3(
                    originalRotation.eulerAngles.x,
                    originalRotation.eulerAngles.y,
                    baseAngle + waddleAngle),
                waddleDuration).SetEase(Ease.InOutSine))
            .Append(transform.DOLocalRotate(
                new Vector3(
                    originalRotation.eulerAngles.x,
                    originalRotation.eulerAngles.y,
                    baseAngle - waddleAngle),
                waddleDuration).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(UpdateType.Fixed);
    }

    private void StartSquashAndStretch(int moveAxis)
    {
        scaleTween?.Kill();
        lastMoveAxis = moveAxis;

        float xScale = moveAxis == 0
            ? 1f + stretchAmount
            : 1f - stretchAmount;

        float yScale = moveAxis == 1
            ? 1f + stretchAmount
            : 1f - stretchAmount;

        Vector3 stretchedScale = new Vector3(
            originalScale.x * xScale,
            originalScale.y * yScale,
            originalScale.z);

        Vector3 bounceScale = new Vector3(
            stretchedScale.x * 0.97f,
            stretchedScale.y * 1.03f,
            stretchedScale.z);

        scaleTween = DOTween.Sequence()
            .Append(transform.DOScale(bounceScale, stretchDuration)
                .SetEase(Ease.OutQuad))
            .Append(transform.DOScale(stretchedScale, stretchDuration)
                .SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(UpdateType.Fixed);
    }

    
    public void FaceDirection(Vector2 direction)
    {
        if (direction.y > 0)
        {
            // Face up.
            if (backSprite != null)
                spriteRenderer.sprite = backSprite;

            spriteRenderer.flipX = false;
        }
        else if (direction.y < 0)
        {
            // Face down.
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;

            spriteRenderer.flipX = false;
        }
        else if (direction.x < 0)
        {
            // Face left.
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;

            spriteRenderer.flipX = true;
        }
        else if (direction.x > 0)
        {
            // Face right.
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;

            spriteRenderer.flipX = false;
        }
    }

    
    private void StopAnimations()
    {
        rotationTween?.Kill();
        scaleTween?.Kill();

        wasMoving = false;
        lastMoveAxis = -1;

        rotationTween = transform
            .DOLocalRotateQuaternion(originalRotation, idleReturnDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(UpdateType.Fixed);

        Vector3 idleScale = new Vector3(
            originalScale.x * (1f + idleSquash),
            originalScale.y * (1f - idleSquash),
            originalScale.z);

        scaleTween = transform.DOScale(idleScale, idleReturnDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                scaleTween = transform.DOScale(originalScale, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(UpdateType.Fixed);
            });
    }

    private void OnDisable()
    {
        rotationTween?.Kill();
        scaleTween?.Kill();

        transform.localRotation = originalRotation;
        transform.localScale = originalScale;
    }

    private void OnDestroy()
    {
        rotationTween?.Kill();
        scaleTween?.Kill();
    }
}
