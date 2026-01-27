// ============================================
// CreditsScreen.cs
// Scrolling credits for contributors and assets
// ============================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace HNR.UI
{
    /// <summary>
    /// Scrolling credits screen with auto-scroll and skip functionality.
    /// </summary>
    public class CreditsScreen : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _creditsContent;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _skipButton;

        [Header("Credits Text")]
        [SerializeField, TextArea(20, 50)] private string _creditsText = @"
<size=48><b>HOLLOW NULL REQUIEM</b></size>

<size=36>A Portfolio Project</size>

---

<size=32><b>DEVELOPMENT</b></size>

<b>Game Design & Programming</b>
[Your Name]

<b>AI-Assisted Development</b>
Claude (Anthropic)

---

<size=32><b>THIRD-PARTY ASSETS</b></size>

<b>Animation</b>
DOTween Pro - Demigiant

<b>Save System</b>
Easy Save 3 - Moodkie

<b>Game Feel</b>
Feel - More Mountains

<b>Post-Processing</b>
Beautify 3 - Kronnect

<b>UI Kit</b>
GUI Pro - Fantasy Hero - LAYERLAB

<b>Character System</b>
Hero Editor - Layer Lab

<b>VFX</b>
Cartoon FX Remaster - Jean Moreno

---

<size=32><b>INSPIRATION</b></size>

Slay the Spire - Mega Crit Games
Monster Train - Shiny Shoe
Chaos Zero Nightmare - TAPTAP

---

<size=32><b>SPECIAL THANKS</b></size>

The roguelike deckbuilder community
Everyone who provided feedback

---

<size=24>Made with Unity</size>

<size=20>© 2024 [Your Name]
All Rights Reserved</size>
";

        [Header("Scroll Settings")]
        [SerializeField] private float _scrollDuration = 30f;
        [SerializeField] private float _startDelay = 1f;
        [SerializeField] private bool _autoScroll = true;

        // ============================================
        // Private State
        // ============================================

        private Coroutine _scrollCoroutine;
        private bool _isScrolling;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>Whether credits are currently scrolling.</summary>
        public bool IsScrolling => _isScrolling;

        /// <summary>Current scroll position (0-1, 1 = top, 0 = bottom).</summary>
        public float ScrollPosition => _scrollRect != null ? _scrollRect.verticalNormalizedPosition : 0f;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);

            if (_skipButton != null)
                _skipButton.onClick.AddListener(SkipToEnd);
        }

        private void OnDisable()
        {
            StopAutoScroll();
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Show credits screen.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, 0.3f);

            // Reset scroll position to top
            if (_scrollRect != null)
                _scrollRect.verticalNormalizedPosition = 1f;

            // Update text content
            var textComponent = _creditsContent?.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = _creditsText;

            if (_autoScroll)
                StartAutoScroll();
        }

        /// <summary>
        /// Hide credits screen.
        /// </summary>
        public void Hide()
        {
            StopAutoScroll();

            _canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// Start auto-scrolling credits.
        /// </summary>
        public void StartAutoScroll()
        {
            StopAutoScroll();
            _scrollCoroutine = StartCoroutine(AutoScrollRoutine());
        }

        /// <summary>
        /// Stop auto-scrolling.
        /// </summary>
        public void StopAutoScroll()
        {
            if (_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
                _scrollCoroutine = null;
            }
            _isScrolling = false;
        }

        /// <summary>
        /// Pause auto-scroll (can be resumed).
        /// </summary>
        public void PauseScroll()
        {
            _isScrolling = false;
        }

        /// <summary>
        /// Resume auto-scroll from current position.
        /// </summary>
        public void ResumeScroll()
        {
            _isScrolling = true;
        }

        /// <summary>
        /// Skip to end of credits.
        /// </summary>
        public void SkipToEnd()
        {
            StopAutoScroll();

            if (_scrollRect != null)
            {
                _scrollRect.DOVerticalNormalizedPos(0f, 0.5f);
            }
        }

        /// <summary>
        /// Reset to beginning of credits.
        /// </summary>
        public void ResetToStart()
        {
            StopAutoScroll();

            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// Set credits text dynamically.
        /// </summary>
        /// <param name="text">New credits text (supports rich text).</param>
        public void SetCreditsText(string text)
        {
            _creditsText = text;

            var textComponent = _creditsContent?.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = _creditsText;
        }

        /// <summary>
        /// Append to credits text.
        /// </summary>
        /// <param name="text">Text to append.</param>
        public void AppendCredits(string text)
        {
            _creditsText += "\n" + text;

            var textComponent = _creditsContent?.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = _creditsText;
        }

        /// <summary>
        /// Get credits as plain text (strips rich text tags).
        /// </summary>
        /// <returns>Plain text credits.</returns>
        public string GetCreditsPlainText()
        {
            return System.Text.RegularExpressions.Regex.Replace(_creditsText, "<.*?>", "");
        }

        /// <summary>
        /// Set scroll duration.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        public void SetScrollDuration(float duration)
        {
            _scrollDuration = Mathf.Max(1f, duration);
        }

        // ============================================
        // Private Methods
        // ============================================

        private IEnumerator AutoScrollRoutine()
        {
            yield return new WaitForSeconds(_startDelay);

            _isScrolling = true;
            float startPosition = _scrollRect != null ? _scrollRect.verticalNormalizedPosition : 1f;
            float elapsed = 0f;

            while (elapsed < _scrollDuration && _scrollRect != null)
            {
                if (_isScrolling)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / _scrollDuration;
                    _scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, 0f, progress);
                }

                yield return null;
            }

            _isScrolling = false;
            _scrollCoroutine = null;
        }

        private void OnBackClicked()
        {
            Hide();
        }
    }
}
