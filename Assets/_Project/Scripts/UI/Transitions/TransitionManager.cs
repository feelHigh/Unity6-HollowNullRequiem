// ============================================
// TransitionManager.cs
// Screen transition effects manager
// ============================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using HNR.Core;

namespace HNR.UI
{
    /// <summary>
    /// Screen transition effect types.
    /// </summary>
    public enum TransitionType
    {
        None,
        Fade,
        SlideLeft,
        SlideRight,
        Dissolve,
        CrossFade
    }

    /// <summary>
    /// Manages screen-to-screen transitions with various visual effects.
    /// Blocks input during transitions to prevent interaction issues.
    /// </summary>
    public class TransitionManager : MonoBehaviour
    {
        // ============================================
        // Overlay References
        // ============================================

        [Header("Overlay")]
        [SerializeField, Tooltip("Canvas group for fade overlay")]
        private CanvasGroup _fadeOverlay;

        [SerializeField, Tooltip("Image component for overlay color")]
        private Image _overlayImage;

        [SerializeField, Tooltip("Overlay color")]
        private Color _overlayColor = Color.black;

        // ============================================
        // Duration Settings
        // ============================================

        [Header("Durations")]
        [SerializeField, Tooltip("Fade transition duration")]
        private float _fadeDuration = 0.3f;

        [SerializeField, Tooltip("Slide transition duration")]
        private float _slideDuration = 0.4f;

        [SerializeField, Tooltip("Dissolve transition duration")]
        private float _dissolveDuration = 0.5f;

        // ============================================
        // Easing Settings
        // ============================================

        [Header("Easing")]
        [SerializeField, Tooltip("Ease for fade transitions")]
        private Ease _fadeEase = Ease.InOutQuad;

        [SerializeField, Tooltip("Ease for slide transitions")]
        private Ease _slideEase = Ease.InOutQuad;

        // ============================================
        // Runtime State
        // ============================================

        private bool _isTransitioning;
        private Coroutine _activeTransition;

        // ============================================
        // Properties
        // ============================================

        /// <summary>
        /// Whether a transition is currently in progress.
        /// </summary>
        public bool IsTransitioning => _isTransitioning;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);

            // Initialize overlay
            if (_fadeOverlay != null)
            {
                _fadeOverlay.alpha = 0f;
                _fadeOverlay.blocksRaycasts = false;
            }

            if (_overlayImage != null)
            {
                _overlayImage.color = _overlayColor;
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TransitionManager>();
        }

        // ============================================
        // Public Transition Methods
        // ============================================

        /// <summary>
        /// Transition between two screens.
        /// </summary>
        /// <param name="from">Screen to hide (can be null).</param>
        /// <param name="to">Screen to show (can be null).</param>
        /// <param name="type">Transition effect type.</param>
        /// <param name="onComplete">Callback when transition completes.</param>
        public void Transition(ScreenBase from, ScreenBase to, TransitionType type, Action onComplete = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[TransitionManager] Transition already in progress");
                return;
            }

            if (_activeTransition != null)
                StopCoroutine(_activeTransition);

            _activeTransition = StartCoroutine(DoTransition(from, to, type, onComplete));
        }

        /// <summary>
        /// Fade screen to black.
        /// </summary>
        /// <param name="onComplete">Callback when fade completes.</param>
        public void FadeToBlack(Action onComplete = null)
        {
            if (_fadeOverlay == null) return;

            _fadeOverlay.blocksRaycasts = true;
            _fadeOverlay.DOFade(1f, _fadeDuration)
                .SetEase(_fadeEase)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Fade from black to clear.
        /// </summary>
        /// <param name="onComplete">Callback when fade completes.</param>
        public void FadeFromBlack(Action onComplete = null)
        {
            if (_fadeOverlay == null) return;

            _fadeOverlay.DOFade(0f, _fadeDuration)
                .SetEase(_fadeEase)
                .OnComplete(() =>
                {
                    _fadeOverlay.blocksRaycasts = false;
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Execute action during black screen (fade out, action, fade in).
        /// </summary>
        /// <param name="duringBlack">Action to execute while screen is black.</param>
        /// <param name="onComplete">Callback when entire sequence completes.</param>
        public void FadeOutInWithAction(Action duringBlack, Action onComplete = null)
        {
            FadeToBlack(() =>
            {
                duringBlack?.Invoke();
                FadeFromBlack(onComplete);
            });
        }

        /// <summary>
        /// Flash the screen (quick fade to color and back).
        /// </summary>
        /// <param name="color">Flash color.</param>
        /// <param name="duration">Total flash duration.</param>
        public void FlashScreen(Color color, float duration = 0.2f)
        {
            if (_overlayImage == null || _fadeOverlay == null) return;

            var originalColor = _overlayImage.color;
            _overlayImage.color = color;

            var seq = DOTween.Sequence();
            seq.Append(_fadeOverlay.DOFade(0.5f, duration * 0.3f));
            seq.Append(_fadeOverlay.DOFade(0f, duration * 0.7f));
            seq.OnComplete(() => _overlayImage.color = originalColor);
        }

        // ============================================
        // Transition Coroutines
        // ============================================

        private IEnumerator DoTransition(ScreenBase from, ScreenBase to, TransitionType type, Action onComplete)
        {
            _isTransitioning = true;
            _fadeOverlay.blocksRaycasts = true;

            switch (type)
            {
                case TransitionType.Fade:
                    yield return FadeTransition(from, to);
                    break;

                case TransitionType.SlideLeft:
                    yield return SlideTransition(from, to, false);
                    break;

                case TransitionType.SlideRight:
                    yield return SlideTransition(from, to, true);
                    break;

                case TransitionType.Dissolve:
                    yield return DissolveTransition(from, to);
                    break;

                case TransitionType.CrossFade:
                    yield return CrossFadeTransition(from, to);
                    break;

                case TransitionType.None:
                default:
                    HideScreen(from);
                    ShowScreen(to);
                    break;
            }

            _isTransitioning = false;
            _fadeOverlay.blocksRaycasts = false;
            _activeTransition = null;
            onComplete?.Invoke();
        }

        private IEnumerator FadeTransition(ScreenBase from, ScreenBase to)
        {
            // Fade to black
            yield return _fadeOverlay.DOFade(1f, _fadeDuration)
                .SetEase(_fadeEase)
                .WaitForCompletion();

            // Switch screens
            HideScreen(from);
            ShowScreen(to);

            // Brief pause at black
            yield return new WaitForSeconds(0.05f);

            // Fade from black
            yield return _fadeOverlay.DOFade(0f, _fadeDuration)
                .SetEase(_fadeEase)
                .WaitForCompletion();
        }

        private IEnumerator SlideTransition(ScreenBase from, ScreenBase to, bool slideRight)
        {
            float direction = slideRight ? 1f : -1f;
            var fromRect = from?.GetComponent<RectTransform>();
            var toRect = to?.GetComponent<RectTransform>();

            float screenWidth = Screen.width;

            // Position incoming screen off-screen
            if (toRect != null)
            {
                toRect.anchoredPosition = new Vector2(screenWidth * -direction, 0);
                ShowScreen(to);
            }

            // Animate both screens
            var seq = DOTween.Sequence();

            if (fromRect != null)
            {
                seq.Append(fromRect.DOAnchorPosX(screenWidth * direction, _slideDuration)
                    .SetEase(_slideEase));
            }

            if (toRect != null)
            {
                seq.Join(toRect.DOAnchorPosX(0, _slideDuration)
                    .SetEase(_slideEase));
            }

            yield return seq.WaitForCompletion();

            // Hide outgoing screen and reset position
            HideScreen(from);
            if (fromRect != null)
            {
                fromRect.anchoredPosition = Vector2.zero;
            }
        }

        private IEnumerator DissolveTransition(ScreenBase from, ScreenBase to)
        {
            var fromGroup = from?.GetComponent<CanvasGroup>();
            var toGroup = to?.GetComponent<CanvasGroup>();

            // Ensure canvas groups exist
            if (fromGroup == null && from != null)
                fromGroup = from.gameObject.AddComponent<CanvasGroup>();
            if (toGroup == null && to != null)
                toGroup = to.gameObject.AddComponent<CanvasGroup>();

            // Setup initial states
            if (toGroup != null)
            {
                toGroup.alpha = 0f;
                ShowScreen(to);
            }

            // Cross-dissolve
            var seq = DOTween.Sequence();

            if (fromGroup != null)
                seq.Append(fromGroup.DOFade(0f, _dissolveDuration).SetEase(Ease.InQuad));

            if (toGroup != null)
                seq.Join(toGroup.DOFade(1f, _dissolveDuration).SetEase(Ease.OutQuad));

            yield return seq.WaitForCompletion();

            // Cleanup
            HideScreen(from);
            if (fromGroup != null)
                fromGroup.alpha = 1f;
        }

        private IEnumerator CrossFadeTransition(ScreenBase from, ScreenBase to)
        {
            var fromGroup = from?.GetComponent<CanvasGroup>();
            var toGroup = to?.GetComponent<CanvasGroup>();

            if (toGroup == null && to != null)
                toGroup = to.gameObject.AddComponent<CanvasGroup>();

            // Show incoming screen behind
            if (toGroup != null)
            {
                toGroup.alpha = 1f;
                ShowScreen(to);
                to.transform.SetAsFirstSibling();
            }

            // Fade out current screen to reveal new one
            if (fromGroup != null)
            {
                yield return fromGroup.DOFade(0f, _fadeDuration)
                    .SetEase(_fadeEase)
                    .WaitForCompletion();

                fromGroup.alpha = 1f;
            }

            HideScreen(from);
            to?.transform.SetAsLastSibling();
        }

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Show a screen (activate and call OnShow).
        /// </summary>
        private void ShowScreen(ScreenBase screen)
        {
            if (screen == null) return;
            screen.gameObject.SetActive(true);
            screen.OnShow();
        }

        /// <summary>
        /// Hide a screen (call OnHide and deactivate).
        /// </summary>
        private void HideScreen(ScreenBase screen)
        {
            if (screen == null) return;
            screen.OnHide();
            screen.gameObject.SetActive(false);
        }
    }
}
