// ============================================
// DifficultySelector.cs
// Difficulty selection component with unlock indicators
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Progression;

namespace HNR.UI.Components
{
    /// <summary>
    /// Component for selecting difficulty level (Easy/Normal/Hard).
    /// Shows unlock status and current selection.
    /// </summary>
    public class DifficultySelector : MonoBehaviour
    {
        // ============================================
        // Events
        // ============================================

        /// <summary>Invoked when difficulty selection changes.</summary>
        public event Action<DifficultyLevel> OnDifficultyChanged;

        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Buttons")]
        [SerializeField, Tooltip("Easy difficulty button")]
        private Button _easyButton;

        [SerializeField, Tooltip("Normal difficulty button")]
        private Button _normalButton;

        [SerializeField, Tooltip("Hard difficulty button")]
        private Button _hardButton;

        [Header("Button Texts")]
        [SerializeField] private TMP_Text _easyText;
        [SerializeField] private TMP_Text _normalText;
        [SerializeField] private TMP_Text _hardText;

        [Header("Lock Icons")]
        [SerializeField, Tooltip("Normal difficulty lock icon")]
        private Image _normalLockIcon;

        [SerializeField, Tooltip("Hard difficulty lock icon")]
        private Image _hardLockIcon;

        [Header("Selection Indicator")]
        [SerializeField, Tooltip("Selection highlight image")]
        private Image _selectionIndicator;

        [Header("Colors")]
        [SerializeField] private Color _selectedColor = new Color(0.9f, 0.7f, 0.2f, 1f);
        [SerializeField] private Color _unselectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Header("Alpha-Based Fading (when no SelectionIndicator)")]
        [SerializeField] private float _selectedAlpha = 1.0f;
        [SerializeField] private float _unselectedAlpha = 0.5f;
        [SerializeField] private float _lockedAlpha = 0.3f;

        [Header("Animation")]
        [SerializeField] private float _selectionAnimDuration = 0.2f;

        // ============================================
        // State
        // ============================================

        private DifficultyLevel _currentDifficulty = DifficultyLevel.Easy;
        private bool _normalUnlocked;
        private bool _hardUnlocked;
        private Tween _currentTween;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Currently selected difficulty.</summary>
        public DifficultyLevel CurrentDifficulty => _currentDifficulty;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Setup button listeners
            if (_easyButton != null)
                _easyButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Easy));

            if (_normalButton != null)
                _normalButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Normal));

            if (_hardButton != null)
                _hardButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Hard));
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();

            if (_easyButton != null)
                _easyButton.onClick.RemoveAllListeners();

            if (_normalButton != null)
                _normalButton.onClick.RemoveAllListeners();

            if (_hardButton != null)
                _hardButton.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            RefreshUnlockStatus();
            UpdateVisuals();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Refreshes unlock status from the progress manager.
        /// </summary>
        public void RefreshUnlockStatus()
        {
            var progressManager = BattleMissionProgressManager.Instance;
            if (progressManager != null)
            {
                _normalUnlocked = progressManager.IsDifficultyUnlocked(DifficultyLevel.Normal);
                _hardUnlocked = progressManager.IsDifficultyUnlocked(DifficultyLevel.Hard);
                _currentDifficulty = progressManager.CurrentDifficulty;
            }
            else
            {
                _normalUnlocked = false;
                _hardUnlocked = false;
                _currentDifficulty = DifficultyLevel.Easy;
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Selects a difficulty level.
        /// </summary>
        public void SelectDifficulty(DifficultyLevel difficulty)
        {
            // Check if difficulty is unlocked
            if (!IsDifficultyUnlocked(difficulty))
            {
                PlayLockedFeedback(difficulty);
                return;
            }

            // Update selection
            _currentDifficulty = difficulty;

            // Save to progress manager
            var progressManager = BattleMissionProgressManager.Instance;
            if (progressManager != null)
            {
                progressManager.CurrentDifficulty = difficulty;
            }

            // Update visuals with animation
            AnimateSelection();
            UpdateVisuals();

            // Invoke event
            OnDifficultyChanged?.Invoke(difficulty);

            Debug.Log($"[DifficultySelector] Selected: {difficulty}");
        }

        /// <summary>
        /// Sets the difficulty without triggering events (for initialization).
        /// </summary>
        public void SetDifficultyWithoutNotify(DifficultyLevel difficulty)
        {
            if (!IsDifficultyUnlocked(difficulty))
            {
                difficulty = DifficultyLevel.Easy;
            }

            _currentDifficulty = difficulty;
            UpdateVisuals();
        }

        // ============================================
        // Helper Methods
        // ============================================

        private bool IsDifficultyUnlocked(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => true,
                DifficultyLevel.Normal => _normalUnlocked,
                DifficultyLevel.Hard => _hardUnlocked,
                _ => false
            };
        }

        // ============================================
        // Visual Updates
        // ============================================

        private void UpdateVisuals()
        {
            // Update button interactability
            if (_normalButton != null)
                _normalButton.interactable = _normalUnlocked;

            if (_hardButton != null)
                _hardButton.interactable = _hardUnlocked;

            // Update lock icons
            if (_normalLockIcon != null)
                _normalLockIcon.gameObject.SetActive(!_normalUnlocked);

            if (_hardLockIcon != null)
                _hardLockIcon.gameObject.SetActive(!_hardUnlocked);

            // Update button colors
            UpdateButtonVisual(_easyButton, _easyText, DifficultyLevel.Easy);
            UpdateButtonVisual(_normalButton, _normalText, DifficultyLevel.Normal);
            UpdateButtonVisual(_hardButton, _hardText, DifficultyLevel.Hard);

            // Update selection indicator position
            UpdateSelectionIndicator();
        }

        private void UpdateButtonVisual(Button button, TMP_Text text, DifficultyLevel difficulty)
        {
            if (button == null) return;

            bool isSelected = _currentDifficulty == difficulty;
            bool isUnlocked = IsDifficultyUnlocked(difficulty);

            // When no selection indicator, use alpha-based fading to preserve button colors
            if (_selectionIndicator == null)
            {
                float targetAlpha;
                if (isSelected)
                    targetAlpha = _selectedAlpha;
                else if (isUnlocked)
                    targetAlpha = _unselectedAlpha;
                else
                    targetAlpha = _lockedAlpha;

                // Apply alpha to all images in the button hierarchy
                foreach (var image in button.GetComponentsInChildren<Image>())
                {
                    var color = image.color;
                    color.a = targetAlpha;
                    image.color = color;
                }

                // Update text alpha
                if (text != null)
                {
                    var textColor = text.color;
                    textColor.a = isUnlocked ? targetAlpha : _lockedAlpha;
                    text.color = textColor;
                }
            }
            else
            {
                // Original behavior: color tinting
                Color targetColor;
                if (isSelected)
                    targetColor = _selectedColor;
                else if (isUnlocked)
                    targetColor = _unselectedColor;
                else
                    targetColor = _lockedColor;

                // Update button image color
                var buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = targetColor;
                }

                // Update text color
                if (text != null)
                {
                    text.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }
        }

        private void UpdateSelectionIndicator()
        {
            if (_selectionIndicator == null) return;

            Button targetButton = _currentDifficulty switch
            {
                DifficultyLevel.Easy => _easyButton,
                DifficultyLevel.Normal => _normalButton,
                DifficultyLevel.Hard => _hardButton,
                _ => _easyButton
            };

            if (targetButton != null)
            {
                // Use RectTransform anchors to properly position the indicator
                // Set anchors to match the button's relative position (0-0.333, 0.333-0.666, 0.666-1)
                var indicatorRect = _selectionIndicator.rectTransform;
                int buttonIndex = _currentDifficulty switch
                {
                    DifficultyLevel.Easy => 0,
                    DifficultyLevel.Normal => 1,
                    DifficultyLevel.Hard => 2,
                    _ => 0
                };

                float anchorStart = buttonIndex / 3f;
                float anchorEnd = (buttonIndex + 1) / 3f;

                indicatorRect.anchorMin = new Vector2(anchorStart, 0);
                indicatorRect.anchorMax = new Vector2(anchorEnd, 1);
                indicatorRect.offsetMin = new Vector2(10, 5);
                indicatorRect.offsetMax = new Vector2(-5, -5);
            }
        }

        // ============================================
        // Animations
        // ============================================

        private void AnimateSelection()
        {
            _currentTween?.Kill();

            if (_selectionIndicator != null)
            {
                var indicatorRect = _selectionIndicator.rectTransform;
                int buttonIndex = _currentDifficulty switch
                {
                    DifficultyLevel.Easy => 0,
                    DifficultyLevel.Normal => 1,
                    DifficultyLevel.Hard => 2,
                    _ => 0
                };

                float targetAnchorStart = buttonIndex / 3f;
                float targetAnchorEnd = (buttonIndex + 1) / 3f;

                // Animate anchor positions using DOTween sequence
                var seq = DOTween.Sequence();
                seq.Append(DOTween.To(
                    () => indicatorRect.anchorMin.x,
                    x => indicatorRect.anchorMin = new Vector2(x, 0),
                    targetAnchorStart,
                    _selectionAnimDuration));
                seq.Join(DOTween.To(
                    () => indicatorRect.anchorMax.x,
                    x => indicatorRect.anchorMax = new Vector2(x, 1),
                    targetAnchorEnd,
                    _selectionAnimDuration));
                seq.SetEase(Ease.OutBack);
                seq.SetLink(gameObject);

                _currentTween = seq;
            }
        }

        private void PlayLockedFeedback(DifficultyLevel difficulty)
        {
            Button button = difficulty switch
            {
                DifficultyLevel.Normal => _normalButton,
                DifficultyLevel.Hard => _hardButton,
                _ => null
            };

            if (button != null)
            {
                button.transform.DOShakePosition(0.3f, 5f, 15)
                    .SetLink(gameObject);
            }
        }

        // ============================================
        // Utility
        // ============================================

        /// <summary>
        /// Gets the display name for a difficulty level.
        /// </summary>
        public static string GetDifficultyName(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => "EASY",
                DifficultyLevel.Normal => "NORMAL",
                DifficultyLevel.Hard => "HARD",
                _ => "EASY"
            };
        }

        /// <summary>
        /// Gets the description for a difficulty level.
        /// </summary>
        public static string GetDifficultyDescription(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => "Standard enemy stats",
                DifficultyLevel.Normal => "Enemies have +25% HP and +10% ATK",
                DifficultyLevel.Hard => "Enemies have +50% HP and +25% ATK",
                _ => ""
            };
        }
    }
}
