// ============================================
// TutorialTooltipManager.cs
// First-time player guidance with tooltips
// ============================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;

namespace HNR.UI
{
    /// <summary>
    /// Tutorial tooltip data.
    /// </summary>
    [Serializable]
    public class TutorialTooltip
    {
        public string Id;
        public string Title;
        [TextArea(2, 4)] public string Message;
        public Vector2 AnchorPosition;
        public bool HasArrow;
        public Vector2 ArrowDirection;
    }

    /// <summary>
    /// Manages tutorial tooltips for first-time player guidance.
    /// Registers with ServiceLocator for global access.
    /// </summary>
    public class TutorialTooltipManager : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("UI References")]
        [SerializeField] private GameObject _tooltipPanel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _dismissButton;
        [SerializeField] private Image _arrowImage;
        [SerializeField] private RectTransform _tooltipRect;

        [Header("Configuration")]
        [SerializeField] private List<TutorialTooltip> _tooltips = new();
        [SerializeField] private float _showDelay = 0.5f;
        [SerializeField] private float _animationDuration = 0.3f;

        [Header("Settings")]
        [SerializeField] private bool _tutorialsEnabled = true;

        // ============================================
        // Private State
        // ============================================

        private HashSet<string> _shownTooltips = new();
        private TutorialTooltip _currentTooltip;
        private Coroutine _showCoroutine;

        private const string SHOWN_TOOLTIPS_KEY = "ShownTutorials";
        private const string TUTORIALS_ENABLED_KEY = "TutorialsEnabled";

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>Whether tutorials are enabled.</summary>
        public bool TutorialsEnabled
        {
            get => _tutorialsEnabled;
            set
            {
                _tutorialsEnabled = value;
                PlayerPrefs.SetInt(TUTORIALS_ENABLED_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Whether a tooltip is currently showing.</summary>
        public bool IsShowingTooltip => _currentTooltip != null;

        /// <summary>Number of tooltips shown this session.</summary>
        public int ShownCount => _shownTooltips.Count;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
            LoadShownTooltips();

            _tutorialsEnabled = PlayerPrefs.GetInt(TUTORIALS_ENABLED_KEY, 1) == 1;

            if (_dismissButton != null)
                _dismissButton.onClick.AddListener(DismissCurrentTooltip);

            HideTooltip(false);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TutorialTooltipManager>();
            Time.timeScale = 1f;
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Try to show a tooltip if not already shown.
        /// </summary>
        /// <param name="tooltipId">Tooltip ID to show.</param>
        /// <returns>True if tooltip will be shown.</returns>
        public bool TryShowTooltip(string tooltipId)
        {
            if (!_tutorialsEnabled) return false;
            if (_shownTooltips.Contains(tooltipId)) return false;
            if (_currentTooltip != null) return false;

            var tooltip = _tooltips.Find(t => t.Id == tooltipId);
            if (tooltip == null)
            {
                Debug.LogWarning($"[TutorialTooltipManager] Tooltip '{tooltipId}' not found");
                return false;
            }

            if (_showCoroutine != null)
                StopCoroutine(_showCoroutine);

            _showCoroutine = StartCoroutine(ShowTooltipDelayed(tooltip));
            return true;
        }

        /// <summary>
        /// Show tooltip immediately.
        /// </summary>
        /// <param name="tooltip">Tooltip to show.</param>
        public void ShowTooltip(TutorialTooltip tooltip)
        {
            if (tooltip == null) return;

            _currentTooltip = tooltip;

            // Set content
            if (_titleText != null)
                _titleText.text = tooltip.Title;

            if (_messageText != null)
                _messageText.text = tooltip.Message;

            // Position
            if (_tooltipRect != null)
                _tooltipRect.anchoredPosition = tooltip.AnchorPosition;

            // Arrow
            if (_arrowImage != null)
            {
                _arrowImage.gameObject.SetActive(tooltip.HasArrow);
                if (tooltip.HasArrow)
                {
                    float angle = Mathf.Atan2(tooltip.ArrowDirection.y, tooltip.ArrowDirection.x) * Mathf.Rad2Deg;
                    _arrowImage.transform.localRotation = Quaternion.Euler(0, 0, angle - 90f);
                }
            }

            // Show with animation
            _tooltipPanel.SetActive(true);
            _tooltipPanel.transform.localScale = Vector3.zero;
            _tooltipPanel.transform.DOScale(Vector3.one, _animationDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            // Pause game
            Time.timeScale = 0f;

            Debug.Log($"[TutorialTooltipManager] Showing tooltip: {tooltip.Id}");
        }

        /// <summary>
        /// Dismiss current tooltip.
        /// </summary>
        public void DismissCurrentTooltip()
        {
            if (_currentTooltip == null) return;

            // Mark as shown
            _shownTooltips.Add(_currentTooltip.Id);
            SaveShownTooltips();

            Debug.Log($"[TutorialTooltipManager] Dismissed tooltip: {_currentTooltip.Id}");

            // Hide
            HideTooltip(true);

            // Resume game
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Hide tooltip.
        /// </summary>
        /// <param name="animate">Whether to animate out.</param>
        public void HideTooltip(bool animate)
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _showCoroutine = null;
            }

            if (animate && _tooltipPanel.activeSelf)
            {
                _tooltipPanel.transform.DOScale(Vector3.zero, _animationDuration * 0.5f)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        _tooltipPanel.SetActive(false);
                        _currentTooltip = null;
                    });
            }
            else
            {
                _tooltipPanel.SetActive(false);
                _currentTooltip = null;
            }
        }

        /// <summary>
        /// Reset all shown tooltips (show tutorials again).
        /// </summary>
        public void ResetAllTooltips()
        {
            _shownTooltips.Clear();
            PlayerPrefs.DeleteKey(SHOWN_TOOLTIPS_KEY);
            PlayerPrefs.Save();
            Debug.Log("[TutorialTooltipManager] All tooltips reset");
        }

        /// <summary>
        /// Check if a tooltip was already shown.
        /// </summary>
        /// <param name="tooltipId">Tooltip ID to check.</param>
        /// <returns>True if already shown.</returns>
        public bool WasTooltipShown(string tooltipId)
        {
            return _shownTooltips.Contains(tooltipId);
        }

        /// <summary>
        /// Mark a tooltip as shown without displaying it.
        /// </summary>
        /// <param name="tooltipId">Tooltip ID to mark.</param>
        public void MarkAsShown(string tooltipId)
        {
            _shownTooltips.Add(tooltipId);
            SaveShownTooltips();
        }

        /// <summary>
        /// Add a tooltip at runtime.
        /// </summary>
        /// <param name="tooltip">Tooltip to add.</param>
        public void AddTooltip(TutorialTooltip tooltip)
        {
            if (tooltip != null && !string.IsNullOrEmpty(tooltip.Id))
            {
                _tooltips.Add(tooltip);
            }
        }

        // ============================================
        // Private Methods
        // ============================================

        private IEnumerator ShowTooltipDelayed(TutorialTooltip tooltip)
        {
            yield return new WaitForSeconds(_showDelay);
            ShowTooltip(tooltip);
            _showCoroutine = null;
        }

        private void LoadShownTooltips()
        {
            string data = PlayerPrefs.GetString(SHOWN_TOOLTIPS_KEY, "");
            if (!string.IsNullOrEmpty(data))
            {
                string[] ids = data.Split(',');
                _shownTooltips = new HashSet<string>(ids);
            }
        }

        private void SaveShownTooltips()
        {
            string data = string.Join(",", _shownTooltips);
            PlayerPrefs.SetString(SHOWN_TOOLTIPS_KEY, data);
            PlayerPrefs.Save();
        }

        // ============================================
        // Context Menu
        // ============================================

        /// <summary>
        /// Initialize default HNR tooltips.
        /// </summary>
        [ContextMenu("Initialize Default Tooltips")]
        public void InitializeDefaultTooltips()
        {
            _tooltips.Clear();

            _tooltips.Add(new TutorialTooltip
            {
                Id = "combat_start",
                Title = "Combat Basics",
                Message = "Tap a card to select it, then tap an enemy to attack!\n\nYou have 3 AP per turn to play cards.",
                AnchorPosition = new Vector2(0, -100),
                HasArrow = true,
                ArrowDirection = Vector2.down
            });

            _tooltips.Add(new TutorialTooltip
            {
                Id = "corruption_gain",
                Title = "Hollow Corruption",
                Message = "Your Requiem gained Corruption!\n\nAt 100 Corruption, they enter Null State - powerful but dangerous!",
                AnchorPosition = new Vector2(-200, 0),
                HasArrow = true,
                ArrowDirection = Vector2.left
            });

            _tooltips.Add(new TutorialTooltip
            {
                Id = "null_state",
                Title = "Null State!",
                Message = "A Requiem entered Null State!\n\n+50% damage dealt, but your team takes +25% damage. Use this power wisely!",
                AnchorPosition = Vector2.zero,
                HasArrow = false
            });

            _tooltips.Add(new TutorialTooltip
            {
                Id = "soul_essence_full",
                Title = "Soul Essence Ready",
                Message = "Your Soul Essence is full!\n\nTap a Requiem to unleash their powerful Requiem Art!",
                AnchorPosition = new Vector2(200, 100),
                HasArrow = true,
                ArrowDirection = Vector2.right
            });

            _tooltips.Add(new TutorialTooltip
            {
                Id = "shop_enter",
                Title = "The Hollow Shop",
                Message = "Spend Void Shards to buy new cards and relics.\n\nYou can also remove cards to strengthen your deck!",
                AnchorPosition = Vector2.zero,
                HasArrow = false
            });

            _tooltips.Add(new TutorialTooltip
            {
                Id = "map_branch",
                Title = "Choose Your Path",
                Message = "The map branches here!\n\nDifferent paths offer different challenges and rewards.",
                AnchorPosition = new Vector2(0, 50),
                HasArrow = true,
                ArrowDirection = Vector2.up
            });

            Debug.Log($"[TutorialTooltipManager] Initialized {_tooltips.Count} default tooltips");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
