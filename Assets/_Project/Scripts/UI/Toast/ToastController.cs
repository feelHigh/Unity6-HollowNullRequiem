// ============================================
// ToastController.cs
// Individual toast notification controller
// ============================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HNR.UI.Toast
{
    /// <summary>
    /// Controls individual toast notification behavior.
    /// Attach to toast prefab.
    /// </summary>
    public class ToastController : MonoBehaviour
    {
        // ============================================
        // Visual References
        // ============================================

        [Header("Visual References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Button _dismissButton;

        [Header("Type Icons")]
        [SerializeField] private Sprite _infoIcon;
        [SerializeField] private Sprite _successIcon;
        [SerializeField] private Sprite _warningIcon;
        [SerializeField] private Sprite _errorIcon;

        [Header("Type Colors")]
        [SerializeField] private Color _infoColor = new Color(0.2f, 0.6f, 0.9f);
        [SerializeField] private Color _successColor = new Color(0.2f, 0.8f, 0.4f);
        [SerializeField] private Color _warningColor = new Color(0.9f, 0.7f, 0.2f);
        [SerializeField] private Color _errorColor = new Color(0.9f, 0.3f, 0.3f);

        // ============================================
        // State
        // ============================================

        private float _displayDuration;
        private float _fadeInDuration;
        private float _fadeOutDuration;
        private bool _isDismissed;

        /// <summary>
        /// Fired when toast is dismissed.
        /// </summary>
        public event Action OnDismissed;

        /// <summary>
        /// Whether this toast has been dismissed.
        /// </summary>
        public bool IsDismissed => _isDismissed;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the toast with content and timing.
        /// </summary>
        public void Initialize(string message, ToastType type, float displayDuration, float fadeInDuration, float fadeOutDuration)
        {
            _displayDuration = displayDuration;
            _fadeInDuration = fadeInDuration;
            _fadeOutDuration = fadeOutDuration;

            // Set message
            if (_messageText != null)
            {
                _messageText.text = message;
            }

            // Set type-specific styling
            ApplyTypeStyle(type);

            // Setup dismiss button
            if (_dismissButton != null)
            {
                _dismissButton.onClick.AddListener(Dismiss);
            }

            // Start lifecycle
            StartCoroutine(ToastLifecycle());
        }

        // ============================================
        // Type Styling
        // ============================================

        private void ApplyTypeStyle(ToastType type)
        {
            Color bgColor;
            Sprite iconSprite;

            switch (type)
            {
                case ToastType.Success:
                    bgColor = _successColor;
                    iconSprite = _successIcon;
                    break;
                case ToastType.Warning:
                    bgColor = _warningColor;
                    iconSprite = _warningIcon;
                    break;
                case ToastType.Error:
                    bgColor = _errorColor;
                    iconSprite = _errorIcon;
                    break;
                case ToastType.Info:
                default:
                    bgColor = _infoColor;
                    iconSprite = _infoIcon;
                    break;
            }

            if (_background != null)
            {
                _background.color = bgColor;
            }

            if (_icon != null && iconSprite != null)
            {
                _icon.sprite = iconSprite;
            }
        }

        // ============================================
        // Lifecycle
        // ============================================

        private IEnumerator ToastLifecycle()
        {
            // Start invisible
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            // Fade in
            yield return FadeIn();

            // Wait for display duration
            yield return new WaitForSecondsRealtime(_displayDuration);

            // Fade out and destroy
            if (!_isDismissed)
            {
                yield return FadeOut();
                DestroyToast();
            }
        }

        private IEnumerator FadeIn()
        {
            if (_canvasGroup == null) yield break;

            float elapsed = 0f;
            Vector3 startPos = transform.localPosition + Vector3.up * 20f;
            Vector3 endPos = transform.localPosition;

            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _fadeInDuration;

                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                transform.localPosition = Vector3.Lerp(startPos, endPos, EaseOutQuad(t));

                yield return null;
            }

            _canvasGroup.alpha = 1f;
            transform.localPosition = endPos;
        }

        private IEnumerator FadeOut()
        {
            if (_canvasGroup == null) yield break;

            float elapsed = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _fadeOutDuration;

                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

                yield return null;
            }

            _canvasGroup.alpha = 0f;
        }

        // ============================================
        // Dismissal
        // ============================================

        /// <summary>
        /// Dismiss this toast immediately.
        /// </summary>
        public void Dismiss()
        {
            if (_isDismissed) return;

            _isDismissed = true;
            StopAllCoroutines();
            StartCoroutine(DismissSequence());
        }

        private IEnumerator DismissSequence()
        {
            yield return FadeOut();
            DestroyToast();
        }

        private void DestroyToast()
        {
            _isDismissed = true;
            OnDismissed?.Invoke();
            Destroy(gameObject);
        }

        // ============================================
        // Easing
        // ============================================

        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
    }
}
