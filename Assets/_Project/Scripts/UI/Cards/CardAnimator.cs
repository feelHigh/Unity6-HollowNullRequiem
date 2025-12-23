// ============================================
// CardAnimator.cs
// DOTween-based card animation controller
// ============================================

using System;
using UnityEngine;
using DG.Tweening;
using HNR.Core;
using HNR.VFX;

namespace HNR.UI
{
    /// <summary>
    /// Handles all card animations using DOTween.
    /// Attach to card prefab alongside Card component.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class CardAnimator : MonoBehaviour
    {
        // ============================================
        // Draw Animation Settings
        // ============================================

        [Header("Draw Animation")]
        [SerializeField, Tooltip("Duration of draw animation")]
        private float _drawDuration = 0.3f;

        [SerializeField, Tooltip("Easing for draw animation")]
        private Ease _drawEase = Ease.OutBack;

        // ============================================
        // Hover Animation Settings
        // ============================================

        [Header("Hover Animation")]
        [SerializeField, Tooltip("Scale multiplier on hover")]
        private float _hoverScale = 1.1f;

        [SerializeField, Tooltip("Y offset in pixels on hover")]
        private float _hoverYOffset = 30f;

        [SerializeField, Tooltip("Duration of hover transition")]
        private float _hoverDuration = 0.15f;

        // ============================================
        // Play Animation Settings
        // ============================================

        [Header("Play Animation")]
        [SerializeField, Tooltip("Duration to move to screen center")]
        private float _playToCenter = 0.15f;

        [SerializeField, Tooltip("Duration to move to target")]
        private float _playToTarget = 0.2f;

        [SerializeField, Tooltip("Duration of fade out")]
        private float _playFade = 0.1f;

        [SerializeField, Tooltip("Scale at center before moving to target")]
        private float _playCenterScale = 1.3f;

        // ============================================
        // Discard Animation Settings
        // ============================================

        [Header("Discard Animation")]
        [SerializeField, Tooltip("Duration of discard animation")]
        private float _discardDuration = 0.25f;

        [SerializeField, Tooltip("Scale when discarded")]
        private float _discardScale = 0.8f;

        // ============================================
        // Cached References
        // ============================================

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _sortingCanvas;

        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        private int _originalSortingOrder;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _originalScale = transform.localScale;
        }

        // ============================================
        // Draw Animation
        // ============================================

        /// <summary>
        /// Animate card from deck to hand position.
        /// </summary>
        /// <param name="targetPosition">Final position in hand.</param>
        /// <param name="rotation">Z rotation for fan layout.</param>
        /// <param name="delay">Stagger delay for multiple cards.</param>
        /// <returns>Tween for sequencing.</returns>
        public Tween AnimateDraw(Vector3 targetPosition, float rotation, float delay)
        {
            _originalPosition = targetPosition;

            var seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(_rectTransform.DOAnchorPos(targetPosition, _drawDuration).SetEase(_drawEase));
            seq.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, rotation), _drawDuration));

            return seq;
        }

        /// <summary>
        /// Animate card from deck to hand with scale reveal.
        /// </summary>
        /// <param name="targetPosition">Final position in hand.</param>
        /// <param name="rotation">Z rotation for fan layout.</param>
        /// <param name="delay">Stagger delay for multiple cards.</param>
        /// <returns>Tween for sequencing.</returns>
        public Tween AnimateDrawWithReveal(Vector3 targetPosition, float rotation, float delay)
        {
            _originalPosition = targetPosition;
            transform.localScale = Vector3.zero;

            var seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(_rectTransform.DOAnchorPos(targetPosition, _drawDuration).SetEase(_drawEase));
            seq.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, rotation), _drawDuration));
            seq.Join(transform.DOScale(_originalScale, _drawDuration).SetEase(_drawEase));

            return seq;
        }

        // ============================================
        // Hover Animation
        // ============================================

        /// <summary>
        /// Animate hover state on/off.
        /// </summary>
        /// <param name="isHovered">True to enter hover, false to exit.</param>
        public void AnimateHover(bool isHovered)
        {
            if (isHovered)
            {
                _rectTransform.DOScale(_hoverScale, _hoverDuration).SetEase(Ease.OutQuad);
                _rectTransform.DOAnchorPosY(_originalPosition.y + _hoverYOffset, _hoverDuration);
                SetSortingOrder(100);
            }
            else
            {
                _rectTransform.DOScale(_originalScale, _hoverDuration).SetEase(Ease.OutQuad);
                _rectTransform.DOAnchorPosY(_originalPosition.y, _hoverDuration);
                SetSortingOrder(_originalSortingOrder);
            }
        }

        /// <summary>
        /// Update the stored original position (call when hand is reorganized).
        /// </summary>
        /// <param name="newPosition">New hand slot position.</param>
        public void UpdateOriginalPosition(Vector3 newPosition)
        {
            _originalPosition = newPosition;
        }

        // ============================================
        // Play Animation
        // ============================================

        /// <summary>
        /// Animate card being played to target.
        /// </summary>
        /// <param name="targetPosition">Target position (enemy/self).</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        /// <returns>Tween for sequencing.</returns>
        public Tween AnimatePlay(Vector3 targetPosition, Action onComplete = null)
        {
            var screenCenter = Vector3.zero;

            var seq = DOTween.Sequence();
            seq.Append(_rectTransform.DOAnchorPos(screenCenter, _playToCenter).SetEase(Ease.OutQuad));
            seq.Append(_rectTransform.DOScale(_playCenterScale, 0.1f).SetEase(Ease.OutQuad));
            seq.Append(_rectTransform.DOAnchorPos(targetPosition, _playToTarget).SetEase(Ease.InQuad));
            seq.Join(_canvasGroup.DOFade(0f, _playFade).SetDelay(_playToTarget - _playFade));
            seq.OnComplete(() => onComplete?.Invoke());

            return seq;
        }

        /// <summary>
        /// Animate card being played without target (AOE/self effects).
        /// </summary>
        /// <param name="onComplete">Callback when animation completes.</param>
        /// <returns>Tween for sequencing.</returns>
        public Tween AnimatePlayNoTarget(Action onComplete = null)
        {
            var screenCenter = Vector3.zero;

            var seq = DOTween.Sequence();
            seq.Append(_rectTransform.DOAnchorPos(screenCenter, _playToCenter).SetEase(Ease.OutQuad));
            seq.Append(_rectTransform.DOScale(_playCenterScale, 0.15f).SetEase(Ease.OutQuad));
            seq.Append(_canvasGroup.DOFade(0f, 0.2f));
            seq.OnComplete(() => onComplete?.Invoke());

            return seq;
        }

        // ============================================
        // Discard Animation
        // ============================================

        /// <summary>
        /// Animate card moving to discard pile.
        /// </summary>
        /// <param name="discardPosition">Discard pile position.</param>
        /// <returns>Tween for sequencing.</returns>
        public Tween AnimateDiscard(Vector3 discardPosition)
        {
            var seq = DOTween.Sequence();
            seq.Append(_rectTransform.DOAnchorPos(discardPosition, _discardDuration).SetEase(Ease.InQuad));
            seq.Join(_rectTransform.DOScale(_discardScale, _discardDuration));
            seq.Join(_canvasGroup.DOFade(0f, _discardDuration * 0.8f).SetDelay(_discardDuration * 0.2f));

            return seq;
        }

        /// <summary>
        /// Animate card being exhausted (removed from game).
        /// </summary>
        /// <returns>Tween for sequencing.</returns>
        public Tween AnimateExhaust()
        {
            var seq = DOTween.Sequence();
            seq.Append(_rectTransform.DOScale(0f, 0.3f).SetEase(Ease.InBack));
            seq.Join(_canvasGroup.DOFade(0f, 0.3f));

            // Trigger exhaust VFX at midpoint of animation
            seq.InsertCallback(0.15f, () =>
            {
                if (ServiceLocator.TryGet<VFXPoolManager>(out var vfxPool))
                {
                    vfxPool.Spawn("vfx_corruption", transform.position);
                }
            });

            return seq;
        }

        // ============================================
        // Utility Methods
        // ============================================

        /// <summary>
        /// Set canvas sorting order for layering during animations.
        /// </summary>
        /// <param name="order">Sorting order value.</param>
        public void SetSortingOrder(int order)
        {
            if (_sortingCanvas == null)
            {
                _sortingCanvas = GetComponent<Canvas>();
                if (_sortingCanvas == null)
                {
                    _sortingCanvas = gameObject.AddComponent<Canvas>();
                    _sortingCanvas.overrideSorting = true;
                }
                _originalSortingOrder = _sortingCanvas.sortingOrder;
            }
            _sortingCanvas.sortingOrder = order;
        }

        /// <summary>
        /// Store current sorting order as original.
        /// </summary>
        /// <param name="order">Order to store.</param>
        public void SetOriginalSortingOrder(int order)
        {
            _originalSortingOrder = order;
            SetSortingOrder(order);
        }

        /// <summary>
        /// Kill all active tweens and reset to original state.
        /// </summary>
        public void ResetState()
        {
            DOTween.Kill(_rectTransform);
            DOTween.Kill(_canvasGroup);
            transform.localScale = _originalScale;
            _canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Immediately set position without animation.
        /// </summary>
        /// <param name="position">Target position.</param>
        /// <param name="rotation">Z rotation.</param>
        public void SetPositionImmediate(Vector3 position, float rotation)
        {
            _originalPosition = position;
            _rectTransform.anchoredPosition = position;
            _rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);
        }

        private void OnDestroy()
        {
            DOTween.Kill(_rectTransform);
            DOTween.Kill(_canvasGroup);
        }
    }
}
