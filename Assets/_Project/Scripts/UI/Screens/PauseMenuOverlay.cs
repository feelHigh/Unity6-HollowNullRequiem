// ============================================
// PauseMenuOverlay.cs
// Combat pause menu with resume, settings, and abandon options
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.UI;
using HNR.UI.Components;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Pause menu overlay shown during combat.
    /// Provides options to resume, open settings, or abandon run.
    /// </summary>
    public class PauseMenuOverlay : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        private static PauseMenuOverlay _instance;
        public static PauseMenuOverlay Instance => _instance;

        // ============================================
        // UI References
        // ============================================

        [Header("Container")]
        [SerializeField, Tooltip("Main overlay canvas group")]
        private CanvasGroup _overlay;

        [SerializeField, Tooltip("Dark background panel")]
        private Image _backgroundPanel;

        [SerializeField, Tooltip("Menu content panel")]
        private RectTransform _menuPanel;

        [Header("Title")]
        [SerializeField, Tooltip("Pause menu title")]
        private TMP_Text _titleText;

        [Header("Buttons")]
        [SerializeField, Tooltip("Resume button")]
        private Button _resumeButton;

        [SerializeField, Tooltip("Settings button")]
        private Button _settingsButton;

        [SerializeField, Tooltip("Abandon run button")]
        private Button _abandonButton;

        // ============================================
        // Configuration
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.25f;
        [SerializeField] private float _fadeOutDuration = 0.2f;
        [SerializeField] private float _scaleFromValue = 0.8f;

        // ============================================
        // State
        // ============================================

        private bool _isShowing;
        private float _previousTimeScale;
        private Tween _currentTween;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Setup singleton
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // Setup initial state
            if (_overlay != null)
            {
                _overlay.alpha = 0f;
                _overlay.interactable = false;
                _overlay.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            // Setup button listeners in OnEnable to ensure fields are wired
            // (Awake runs before SerializedObject wiring in scene generators)
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(OnResumeClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);

            if (_abandonButton != null)
                _abandonButton.onClick.AddListener(OnAbandonClicked);
        }

        private void OnDisable()
        {
            // Cleanup button listeners
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(OnResumeClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (_abandonButton != null)
                _abandonButton.onClick.RemoveListener(OnAbandonClicked);
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();

            if (_instance == this)
                _instance = null;

            // Ensure time is restored if destroyed while showing
            if (_isShowing)
            {
                Time.timeScale = 1f;
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Shows the pause menu and pauses the game.
        /// </summary>
        public void Show()
        {
            if (_isShowing) return;

            _isShowing = true;
            gameObject.SetActive(true);

            // Pause game
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // Animate in
            _currentTween?.Kill();

            if (_overlay != null)
            {
                _overlay.alpha = 0f;
                _overlay.interactable = true;
                _overlay.blocksRaycasts = true;
            }

            if (_menuPanel != null)
            {
                _menuPanel.localScale = Vector3.one * _scaleFromValue;
            }

            var sequence = DOTween.Sequence();
            sequence.Append(_overlay.DOFade(1f, _fadeInDuration).SetEase(Ease.OutQuad));

            if (_menuPanel != null)
            {
                sequence.Join(_menuPanel.DOScale(1f, _fadeInDuration).SetEase(Ease.OutBack));
            }

            // Use SetUpdate(true) to animate even when Time.timeScale = 0
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            _currentTween = sequence;

            Debug.Log("[PauseMenuOverlay] Shown");
        }

        /// <summary>
        /// Hides the pause menu and resumes the game.
        /// </summary>
        public void Hide()
        {
            if (!_isShowing) return;

            _currentTween?.Kill();

            var sequence = DOTween.Sequence();

            if (_menuPanel != null)
            {
                sequence.Append(_menuPanel.DOScale(_scaleFromValue, _fadeOutDuration).SetEase(Ease.InQuad));
            }

            sequence.Join(_overlay.DOFade(0f, _fadeOutDuration).SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                if (_overlay != null)
                {
                    _overlay.interactable = false;
                    _overlay.blocksRaycasts = false;
                }
                gameObject.SetActive(false);
                _isShowing = false;

                // Resume game
                Time.timeScale = _previousTimeScale > 0 ? _previousTimeScale : 1f;
            });

            // Use SetUpdate(true) to animate even when Time.timeScale = 0
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            _currentTween = sequence;

            Debug.Log("[PauseMenuOverlay] Hidden");
        }

        /// <summary>
        /// Whether the pause menu is currently visible.
        /// </summary>
        public bool IsShowing => _isShowing;

        // ============================================
        // Button Handlers
        // ============================================

        private void OnResumeClicked()
        {
            Hide();
        }

        private void OnSettingsClicked()
        {
            // Find and show settings screen directly (it's a MonoBehaviour, not ScreenBase)
            var settingsScreen = FindFirstObjectByType<SettingsScreen>(FindObjectsInactive.Include);
            if (settingsScreen != null)
            {
                // Hide pause menu first, then show settings
                Hide();
                settingsScreen.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[PauseMenuOverlay] SettingsScreen not found in scene");
            }
        }

        private void OnAbandonClicked()
        {
            ConfirmationDialog.Show(
                "Abandon Run?",
                "Are you sure you want to abandon the current run? All progress will be lost.",
                onConfirm: () =>
                {
                    // Restore time scale first
                    Time.timeScale = 1f;
                    _isShowing = false;

                    // End run without victory
                    if (ServiceLocator.TryGet<IRunManager>(out var runManager))
                    {
                        runManager.EndRun(false);
                    }

                    // Delete saved run
                    if (ServiceLocator.TryGet<ISaveManager>(out var saveManager))
                    {
                        saveManager.DeleteRun();
                    }

                    // Navigate to Bastion
                    if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
                    {
                        gameManager.ChangeState(GameState.Bastion);
                    }
                },
                onCancel: null,
                confirmText: "Abandon",
                cancelText: "Continue"
            );
        }

        // ============================================
        // Static Helper
        // ============================================

        /// <summary>
        /// Toggles the pause menu.
        /// </summary>
        public static void Toggle()
        {
            if (_instance == null) return;

            if (_instance.IsShowing)
                _instance.Hide();
            else
                _instance.Show();
        }
    }
}
