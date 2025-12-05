// ============================================
// LoadingScreen.cs
// Loading screen with progress bar and tips
// ============================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace HNR.UI
{
    /// <summary>
    /// Loading screen with progress bar, rotating tips, and spinner animation.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _tipText;
        [SerializeField] private Image _spinnerIcon;

        [Header("Tips")]
        [SerializeField] private List<string> _loadingTips = new()
        {
            "Tip: Corruption can be both a curse and a blessing. Null State grants immense power!",
            "Tip: Team composition matters. Mix damage dealers with support characters.",
            "Tip: Cards from all three Requiems share one deck. Plan your synergies!",
            "Tip: Soul Essence builds over combat. Save it for your Requiem Arts!",
            "Tip: Echo Events offer risk-reward choices. Choose wisely!",
            "Tip: Block resets at the end of your turn. Use it or lose it!",
            "Tip: Status effects stack. Burn and Poison can devastate enemies!",
            "Tip: The Shop sells card removal. Sometimes a smaller deck is better!",
            "Tip: Relics provide passive bonuses throughout your run.",
            "Tip: Null State corruption resets to 50 after combat, not 0."
        };

        [Header("Animation")]
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private float _tipChangeInterval = 5f;
        [SerializeField] private float _spinnerSpeed = 180f;

        // ============================================
        // Private State
        // ============================================

        private Coroutine _tipRotation;
        private int _currentTipIndex;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>Current loading progress (0-1).</summary>
        public float Progress => _progressBar != null ? _progressBar.value : 0f;

        /// <summary>Whether the loading screen is visible.</summary>
        public bool IsVisible => gameObject.activeSelf && _canvasGroup.alpha > 0f;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            Hide(false);
        }

        private void Update()
        {
            if (_spinnerIcon != null && IsVisible)
            {
                _spinnerIcon.transform.Rotate(0f, 0f, -_spinnerSpeed * Time.deltaTime);
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Show loading screen.
        /// </summary>
        /// <param name="animate">Whether to fade in.</param>
        public void Show(bool animate = true)
        {
            gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;

            SetProgress(0f);
            ShowRandomTip();
            StartTipRotation();

            if (animate)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, _fadeDuration);
            }
            else
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Hide loading screen.
        /// </summary>
        /// <param name="animate">Whether to fade out.</param>
        public void Hide(bool animate = true)
        {
            StopTipRotation();
            _canvasGroup.blocksRaycasts = false;

            if (animate)
            {
                _canvasGroup.DOFade(0f, _fadeDuration).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                _canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set loading progress (0-1).
        /// </summary>
        /// <param name="progress">Progress value between 0 and 1.</param>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (_progressBar != null)
                _progressBar.value = progress;

            if (_progressText != null)
                _progressText.text = $"{progress * 100:F0}%";
        }

        /// <summary>
        /// Set custom loading message instead of percentage.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public void SetMessage(string message)
        {
            if (_progressText != null)
                _progressText.text = message;
        }

        /// <summary>
        /// Show a random tip immediately.
        /// </summary>
        public void ShowRandomTip()
        {
            if (_loadingTips.Count == 0 || _tipText == null) return;

            _currentTipIndex = Random.Range(0, _loadingTips.Count);
            _tipText.text = _loadingTips[_currentTipIndex];
        }

        /// <summary>
        /// Show next tip with fade animation.
        /// </summary>
        public void ShowNextTip()
        {
            if (_loadingTips.Count == 0 || _tipText == null) return;

            _tipText.DOFade(0f, 0.2f).OnComplete(() =>
            {
                _currentTipIndex = (_currentTipIndex + 1) % _loadingTips.Count;
                _tipText.text = _loadingTips[_currentTipIndex];
                _tipText.DOFade(1f, 0.2f);
            });
        }

        /// <summary>
        /// Add a custom tip to the rotation.
        /// </summary>
        /// <param name="tip">Tip text to add.</param>
        public void AddTip(string tip)
        {
            if (!string.IsNullOrEmpty(tip) && !_loadingTips.Contains(tip))
                _loadingTips.Add(tip);
        }

        /// <summary>
        /// Clear all tips.
        /// </summary>
        public void ClearTips()
        {
            _loadingTips.Clear();
        }

        /// <summary>
        /// Set tips from external source.
        /// </summary>
        /// <param name="tips">List of tips.</param>
        public void SetTips(List<string> tips)
        {
            _loadingTips = tips ?? new List<string>();
        }

        // ============================================
        // Private Methods
        // ============================================

        private void StartTipRotation()
        {
            StopTipRotation();
            _tipRotation = StartCoroutine(TipRotationRoutine());
        }

        private void StopTipRotation()
        {
            if (_tipRotation != null)
            {
                StopCoroutine(_tipRotation);
                _tipRotation = null;
            }
        }

        private IEnumerator TipRotationRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_tipChangeInterval);
                ShowNextTip();
            }
        }

        private void OnDisable()
        {
            StopTipRotation();
        }
    }
}
