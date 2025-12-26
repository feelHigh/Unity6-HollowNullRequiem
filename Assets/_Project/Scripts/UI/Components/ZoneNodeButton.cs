// ============================================
// ZoneNodeButton.cs
// Zone selection button with locked/unlocked states
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
    /// Button component for zone selection in Battle Mission screen.
    /// Displays zone name, number, and clear status with locked/unlocked visuals.
    /// </summary>
    public class ZoneNodeButton : MonoBehaviour
    {
        // ============================================
        // Events
        // ============================================

        /// <summary>Invoked when zone is clicked (only if unlocked).</summary>
        public event Action<int> OnZoneSelected;

        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("Zone number (1-3)")]
        private int _zoneNumber = 1;

        [SerializeField, Tooltip("Zone display name")]
        private string _zoneName = "Zone 1";

        [Header("UI References")]
        [SerializeField, Tooltip("Main button component")]
        private Button _button;

        [SerializeField, Tooltip("Zone number text")]
        private TMP_Text _zoneNumberText;

        [SerializeField, Tooltip("Zone name text")]
        private TMP_Text _zoneNameText;

        [SerializeField, Tooltip("Clear status text (e.g., 'CLEARED')")]
        private TMP_Text _statusText;

        [SerializeField, Tooltip("Lock icon image")]
        private Image _lockIcon;

        [SerializeField, Tooltip("Clear checkmark image")]
        private Image _clearIcon;

        [SerializeField, Tooltip("Background image")]
        private Image _backgroundImage;

        [SerializeField, Tooltip("Zone preview image/artwork")]
        private Image _zoneImage;

        [Header("Colors")]
        [SerializeField] private Color _unlockedColor = new Color(0.2f, 0.6f, 0.9f, 1f);
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _clearedColor = new Color(0.3f, 0.8f, 0.3f, 1f);

        [Header("Animation")]
        [SerializeField] private float _punchScale = 0.1f;
        [SerializeField] private float _punchDuration = 0.2f;

        // ============================================
        // State
        // ============================================

        private bool _isUnlocked;
        private bool _isCleared;
        private Tween _currentTween;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Zone number (1-3).</summary>
        public int ZoneNumber => _zoneNumber;

        /// <summary>Whether this zone is currently unlocked.</summary>
        public bool IsUnlocked => _isUnlocked;

        /// <summary>Whether this zone has been cleared.</summary>
        public bool IsCleared => _isCleared;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();

            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void Start()
        {
            UpdateVisuals();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Sets the zone configuration.
        /// </summary>
        public void SetZone(int zoneNumber, string zoneName)
        {
            _zoneNumber = zoneNumber;
            _zoneName = zoneName;

            if (_zoneNumberText != null)
                _zoneNumberText.text = $"Zone {zoneNumber}";

            if (_zoneNameText != null)
                _zoneNameText.text = zoneName;
        }

        /// <summary>
        /// Updates the locked/unlocked state of the zone.
        /// </summary>
        public void SetUnlocked(bool unlocked)
        {
            _isUnlocked = unlocked;
            UpdateVisuals();
        }

        /// <summary>
        /// Updates the cleared state of the zone.
        /// </summary>
        public void SetCleared(bool cleared)
        {
            _isCleared = cleared;
            UpdateVisuals();
        }

        /// <summary>
        /// Updates the zone state based on progress manager.
        /// </summary>
        public void UpdateFromProgressManager(DifficultyLevel difficulty)
        {
            var progressManager = BattleMissionProgressManager.Instance;
            if (progressManager == null) return;

            _isUnlocked = progressManager.IsZoneUnlocked(_zoneNumber, difficulty);
            _isCleared = progressManager.IsZoneCleared(_zoneNumber, difficulty);
            UpdateVisuals();
        }

        /// <summary>
        /// Sets the zone preview image.
        /// </summary>
        public void SetZoneImage(Sprite sprite)
        {
            if (_zoneImage != null)
            {
                _zoneImage.sprite = sprite;
                _zoneImage.gameObject.SetActive(sprite != null);
            }
        }

        // ============================================
        // Visual Updates
        // ============================================

        private void UpdateVisuals()
        {
            // Update button interactability
            if (_button != null)
            {
                _button.interactable = _isUnlocked;
            }

            // Update lock icon
            if (_lockIcon != null)
            {
                _lockIcon.gameObject.SetActive(!_isUnlocked);
            }

            // Update clear icon
            if (_clearIcon != null)
            {
                _clearIcon.gameObject.SetActive(_isCleared);
            }

            // Update status text
            if (_statusText != null)
            {
                if (_isCleared)
                {
                    _statusText.text = "CLEARED";
                    _statusText.color = _clearedColor;
                }
                else if (_isUnlocked)
                {
                    _statusText.text = "";
                }
                else
                {
                    _statusText.text = "LOCKED";
                    _statusText.color = _lockedColor;
                }
            }

            // Update background color
            if (_backgroundImage != null)
            {
                Color targetColor;
                if (_isCleared)
                    targetColor = _clearedColor;
                else if (_isUnlocked)
                    targetColor = _unlockedColor;
                else
                    targetColor = _lockedColor;

                _backgroundImage.color = targetColor;
            }

            // Update text colors based on state
            Color textColor = _isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            if (_zoneNumberText != null)
                _zoneNumberText.color = textColor;
            if (_zoneNameText != null)
                _zoneNameText.color = textColor;
        }

        // ============================================
        // Button Handler
        // ============================================

        private void OnButtonClicked()
        {
            if (!_isUnlocked)
            {
                // Shake animation for locked
                _currentTween?.Kill();
                _currentTween = transform.DOShakePosition(0.3f, 10f, 20)
                    .SetLink(gameObject);
                return;
            }

            // Punch scale animation for selection
            _currentTween?.Kill();
            _currentTween = transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, 5)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);

            // Invoke event
            OnZoneSelected?.Invoke(_zoneNumber);
        }

        // ============================================
        // Animation
        // ============================================

        /// <summary>
        /// Plays an entrance animation.
        /// </summary>
        public void PlayEntranceAnimation(float delay = 0f)
        {
            _currentTween?.Kill();

            transform.localScale = Vector3.zero;
            _currentTween = transform.DOScale(1f, 0.3f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);
        }

        /// <summary>
        /// Plays a highlight animation (for newly unlocked zones).
        /// </summary>
        public void PlayUnlockAnimation()
        {
            _currentTween?.Kill();

            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 5));

            if (_backgroundImage != null)
            {
                sequence.Join(_backgroundImage.DOColor(_unlockedColor, 0.4f));
            }

            sequence.SetLink(gameObject);
            _currentTween = sequence;
        }
    }
}
