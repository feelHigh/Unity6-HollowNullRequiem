// ============================================
// ConfirmationDialog.cs
// Reusable confirmation dialog modal
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.UI.Config;

namespace HNR.UI.Components
{
    /// <summary>
    /// Reusable confirmation dialog with title, message, and confirm/cancel buttons.
    /// Uses DOTween for smooth fade animations.
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        private static ConfirmationDialog _instance;
        public static ConfirmationDialog Instance => _instance;

        // ============================================
        // Static Prefab Reference
        // ============================================

        private static GameObject _dialogPrefab;

        // ============================================
        // UI References
        // ============================================

        [Header("Container")]
        [SerializeField, Tooltip("Main overlay canvas group")]
        private CanvasGroup _overlay;

        [SerializeField, Tooltip("Dark background panel")]
        private Image _backgroundPanel;

        [SerializeField, Tooltip("Dialog content panel")]
        private RectTransform _dialogPanel;

        [Header("Content")]
        [SerializeField, Tooltip("Dialog title text")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Dialog message text")]
        private TMP_Text _messageText;

        [Header("Buttons")]
        [SerializeField, Tooltip("Confirm/Yes button")]
        private Button _confirmButton;

        [SerializeField, Tooltip("Confirm button text")]
        private TMP_Text _confirmButtonText;

        [SerializeField, Tooltip("Cancel/No button")]
        private Button _cancelButton;

        [SerializeField, Tooltip("Cancel button text")]
        private TMP_Text _cancelButtonText;

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
        private Action _onConfirm;
        private Action _onCancel;
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
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDisable()
        {
            // Cleanup button listeners
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();

            if (_instance == this)
                _instance = null;
        }

        // ============================================
        // Static API
        // ============================================

        /// <summary>
        /// Shows a confirmation dialog.
        /// </summary>
        /// <param name="title">Dialog title.</param>
        /// <param name="message">Dialog message.</param>
        /// <param name="onConfirm">Action when confirmed.</param>
        /// <param name="onCancel">Action when cancelled (optional).</param>
        /// <param name="confirmText">Confirm button text (default: "Confirm").</param>
        /// <param name="cancelText">Cancel button text (default: "Cancel").</param>
        public static void Show(
            string title,
            string message,
            Action onConfirm,
            Action onCancel = null,
            string confirmText = "Confirm",
            string cancelText = "Cancel")
        {
            if (_instance == null)
            {
                Debug.LogWarning("[ConfirmationDialog] No instance found. Creating one...");
                CreateInstance();
            }

            _instance?.ShowDialog(title, message, onConfirm, onCancel, confirmText, cancelText);
        }

        /// <summary>
        /// Hides the current dialog without triggering callbacks.
        /// </summary>
        public static void Hide()
        {
            _instance?.HideDialog();
        }

        // ============================================
        // Instance Methods
        // ============================================

        private void ShowDialog(
            string title,
            string message,
            Action onConfirm,
            Action onCancel,
            string confirmText,
            string cancelText)
        {
            if (_isShowing) return;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            // Set content
            if (_titleText != null) _titleText.text = title;
            if (_messageText != null) _messageText.text = message;
            if (_confirmButtonText != null) _confirmButtonText.text = confirmText;
            if (_cancelButtonText != null) _cancelButtonText.text = cancelText;

            // Show dialog
            gameObject.SetActive(true);
            _isShowing = true;

            // Animate in
            _currentTween?.Kill();

            if (_overlay != null)
            {
                _overlay.alpha = 0f;
                _overlay.interactable = true;
                _overlay.blocksRaycasts = true;
            }

            // Scale animation for dialog panel
            if (_dialogPanel != null)
            {
                _dialogPanel.localScale = Vector3.one * _scaleFromValue;
            }

            var sequence = DOTween.Sequence();
            sequence.Append(_overlay.DOFade(1f, _fadeInDuration).SetEase(Ease.OutQuad));

            if (_dialogPanel != null)
            {
                sequence.Join(_dialogPanel.DOScale(1f, _fadeInDuration).SetEase(Ease.OutBack));
            }

            // Use SetUpdate(true) to animate even when Time.timeScale = 0
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            _currentTween = sequence;

            Debug.Log($"[ConfirmationDialog] Showing: {title}");
        }

        private void HideDialog()
        {
            if (!_isShowing) return;

            _currentTween?.Kill();

            var sequence = DOTween.Sequence();

            if (_dialogPanel != null)
            {
                sequence.Append(_dialogPanel.DOScale(_scaleFromValue, _fadeOutDuration).SetEase(Ease.InQuad));
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
            });

            // Use SetUpdate(true) to animate even when Time.timeScale = 0
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            _currentTween = sequence;
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnConfirmClicked()
        {
            var callback = _onConfirm;
            HideDialog();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            var callback = _onCancel;
            HideDialog();
            callback?.Invoke();
        }

        // ============================================
        // Runtime Instance Creation
        // ============================================

        /// <summary>
        /// Sets the prefab to use for dialog creation. Call before first Show().
        /// </summary>
        public static void SetDialogPrefab(GameObject prefab)
        {
            _dialogPrefab = prefab;
        }

        private static void CreateInstance()
        {
            // Try to use prefab from config
            var prefab = _dialogPrefab;
            if (prefab == null)
            {
                var config = RuntimeUIPrefabConfigSO.Instance;
                prefab = config?.ConfirmationDialogPrefab;
            }

            if (prefab == null)
            {
                Debug.LogError("[ConfirmationDialog] No prefab available. Assign ConfirmationDialogPrefab in RuntimeUIPrefabConfig or call SetDialogPrefab().");
                return;
            }

            var prefabInstance = Instantiate(prefab);
            prefabInstance.name = "ConfirmationDialog";
            DontDestroyOnLoad(prefabInstance);
            Debug.Log("[ConfirmationDialog] Created from prefab");
        }
    }
}
