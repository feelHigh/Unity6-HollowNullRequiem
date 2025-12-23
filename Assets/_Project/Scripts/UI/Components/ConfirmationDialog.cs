// ============================================
// ConfirmationDialog.cs
// Reusable confirmation dialog modal
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

        private static void CreateInstance()
        {
            // Create dialog at runtime if needed
            var go = new GameObject("ConfirmationDialog");

            // Create Canvas
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Above everything

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            // Create overlay
            var overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(go.transform, false);
            var overlayRect = overlayGO.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            var canvasGroup = overlayGO.AddComponent<CanvasGroup>();

            // Background panel
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(overlayGO.transform, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // Dialog panel
            var dialogGO = new GameObject("DialogPanel");
            dialogGO.transform.SetParent(overlayGO.transform, false);
            var dialogRect = dialogGO.AddComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(500, 250);
            var dialogImage = dialogGO.AddComponent<Image>();
            dialogImage.color = UIColors.PanelGray;

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(dialogGO.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(-40, 40);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "Confirm";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = UIColors.SoulCyan;
            titleText.alignment = TextAlignmentOptions.Center;

            // Message
            var messageGO = new GameObject("Message");
            messageGO.transform.SetParent(dialogGO.transform, false);
            var messageRect = messageGO.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0, 0.35f);
            messageRect.anchorMax = new Vector2(1, 0.85f);
            messageRect.sizeDelta = new Vector2(-40, 0);
            var messageText = messageGO.AddComponent<TextMeshProUGUI>();
            messageText.text = "Are you sure?";
            messageText.fontSize = 18;
            messageText.color = Color.white;
            messageText.alignment = TextAlignmentOptions.Center;

            // Confirm button
            var confirmGO = new GameObject("ConfirmButton");
            confirmGO.transform.SetParent(dialogGO.transform, false);
            var confirmRect = confirmGO.AddComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.55f, 0.1f);
            confirmRect.anchorMax = new Vector2(0.95f, 0.3f);
            confirmRect.sizeDelta = Vector2.zero;
            var confirmImage = confirmGO.AddComponent<Image>();
            confirmImage.color = UIColors.CorruptionGlow;
            var confirmBtn = confirmGO.AddComponent<Button>();
            confirmBtn.targetGraphic = confirmImage;

            var confirmTextGO = new GameObject("Text");
            confirmTextGO.transform.SetParent(confirmGO.transform, false);
            var confirmTextRect = confirmTextGO.AddComponent<RectTransform>();
            confirmTextRect.anchorMin = Vector2.zero;
            confirmTextRect.anchorMax = Vector2.one;
            confirmTextRect.sizeDelta = Vector2.zero;
            var confirmBtnText = confirmTextGO.AddComponent<TextMeshProUGUI>();
            confirmBtnText.text = "Confirm";
            confirmBtnText.fontSize = 18;
            confirmBtnText.fontStyle = FontStyles.Bold;
            confirmBtnText.color = Color.white;
            confirmBtnText.alignment = TextAlignmentOptions.Center;

            // Cancel button
            var cancelGO = new GameObject("CancelButton");
            cancelGO.transform.SetParent(dialogGO.transform, false);
            var cancelRect = cancelGO.AddComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.05f, 0.1f);
            cancelRect.anchorMax = new Vector2(0.45f, 0.3f);
            cancelRect.sizeDelta = Vector2.zero;
            var cancelImage = cancelGO.AddComponent<Image>();
            cancelImage.color = UIColors.PanelGray;
            var cancelBtn = cancelGO.AddComponent<Button>();
            cancelBtn.targetGraphic = cancelImage;

            var cancelTextGO = new GameObject("Text");
            cancelTextGO.transform.SetParent(cancelGO.transform, false);
            var cancelTextRect = cancelTextGO.AddComponent<RectTransform>();
            cancelTextRect.anchorMin = Vector2.zero;
            cancelTextRect.anchorMax = Vector2.one;
            cancelTextRect.sizeDelta = Vector2.zero;
            var cancelBtnText = cancelTextGO.AddComponent<TextMeshProUGUI>();
            cancelBtnText.text = "Cancel";
            cancelBtnText.fontSize = 18;
            cancelBtnText.fontStyle = FontStyles.Bold;
            cancelBtnText.color = UIColors.SoulCyan;
            cancelBtnText.alignment = TextAlignmentOptions.Center;

            // Add ConfirmationDialog component
            var dialog = go.AddComponent<ConfirmationDialog>();
            dialog._overlay = canvasGroup;
            dialog._backgroundPanel = bgImage;
            dialog._dialogPanel = dialogRect;
            dialog._titleText = titleText;
            dialog._messageText = messageText;
            dialog._confirmButton = confirmBtn;
            dialog._confirmButtonText = confirmBtnText;
            dialog._cancelButton = cancelBtn;
            dialog._cancelButtonText = cancelBtnText;

            // Don't destroy on load
            DontDestroyOnLoad(go);
        }
    }
}
