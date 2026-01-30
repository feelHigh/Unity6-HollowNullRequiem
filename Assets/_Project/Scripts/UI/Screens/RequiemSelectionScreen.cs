// ============================================
// RequiemSelectionScreen.cs
// Team selection screen before starting a run
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Core.Events;
using HNR.Characters;
using HNR.Cards;
using HNR.UI.Config;
using HNR.UI.Components;

namespace HNR.UI
{
    /// <summary>
    /// Screen for selecting up to 3 Requiems before starting a run.
    /// Displays all available Requiems with detailed stats preview.
    /// </summary>
    public class RequiemSelectionScreen : ScreenBase
    {
        // ============================================
        // References
        // ============================================

        [Header("Slot Configuration")]
        [SerializeField, Tooltip("Container for Requiem slots")]
        private Transform _slotContainer;

        [SerializeField, Tooltip("Prefab for individual Requiem slots")]
        private RequiemSlotUI _slotPrefab;

        [Header("Selection UI")]
        [SerializeField, Tooltip("Button to start the run")]
        private Button _startRunButton;

        [SerializeField, Tooltip("Text showing selected count")]
        private TextMeshProUGUI _selectedCountText;

        [SerializeField, Tooltip("Button to go back")]
        private Button _backButton;

        [Header("Preview Panel")]
        [SerializeField, Tooltip("Preview panel container")]
        private GameObject _previewPanel;

        [SerializeField, Tooltip("Preview portrait image")]
        private Image _previewPortrait;

        [SerializeField, Tooltip("Preview name text")]
        private TextMeshProUGUI _previewNameText;

        [SerializeField, Tooltip("Preview title text")]
        private TextMeshProUGUI _previewTitleText;

        [SerializeField, Tooltip("Preview class/aspect text")]
        private TextMeshProUGUI _previewClassText;

        [SerializeField, Tooltip("Preview stats text")]
        private TextMeshProUGUI _previewStatsText;

        [SerializeField, Tooltip("Preview backstory text")]
        private TextMeshProUGUI _previewBackstoryText;

        [SerializeField, Tooltip("Preview Requiem Art name")]
        private TextMeshProUGUI _previewArtNameText;

        [SerializeField, Tooltip("Preview Requiem Art description")]
        private TextMeshProUGUI _previewArtDescText;

        [SerializeField, Tooltip("Preview starting cards count")]
        private TextMeshProUGUI _previewCardsText;

        [Header("Team Stats")]
        [SerializeField, Tooltip("Combined team HP text")]
        private TextMeshProUGUI _teamHPText;

        [SerializeField, Tooltip("Combined team ATK text")]
        private TextMeshProUGUI _teamATKText;

        [SerializeField, Tooltip("Combined team DEF text")]
        private TextMeshProUGUI _teamDEFText;

        [Header("Data")]
        [SerializeField, Tooltip("All available Requiems")]
        private RequiemDataSO[] _availableRequiems;

        [Header("Config")]
        [SerializeField, Tooltip("Aspect icon configuration for badges")]
        private AspectIconConfigSO _aspectIconConfig;

        // ============================================
        // State
        // ============================================

        private List<RequiemSlotUI> _slots = new();
        private List<RequiemSelectionSlotComponent> _slotComponents = new();
        private List<RequiemDataSO> _selectedRequiems = new();
        private RequiemDataSO _previewedRequiem;

        private const int MAX_TEAM_SIZE = 3;

        // ============================================
        // Lifecycle
        // ============================================

        protected override void Awake()
        {
            base.Awake();
            // Wire button listeners
            if (_startRunButton != null)
                _startRunButton.onClick.AddListener(OnStartRunClicked);

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        public override void OnShow()
        {
            base.OnShow();
            _selectedRequiems.Clear();
            InitializeSlots();
            UpdateUI();
            HidePreview();
        }

        public override void OnHide()
        {
            base.OnHide();
            ClearSlots();
        }

        // ============================================
        // Slot Management
        // ============================================

        private void InitializeSlots()
        {
            ClearSlots();

            // Auto-load Requiems from Resources if not assigned in Inspector
            if (_availableRequiems == null || _availableRequiems.Length == 0)
            {
                _availableRequiems = Resources.LoadAll<RequiemDataSO>("Data/Characters/Requiems");
                Debug.Log($"[RequiemSelectionScreen] Auto-loaded {_availableRequiems?.Length ?? 0} Requiems from Resources");
            }

            if (_availableRequiems == null || _availableRequiems.Length == 0)
            {
                Debug.LogWarning("[RequiemSelectionScreen] No Requiems found! Check Resources/Data/Characters/Requiems/");
                return;
            }

            // If no slot prefab, create simple buttons dynamically
            if (_slotPrefab == null)
            {
                Debug.Log("[RequiemSelectionScreen] No slot prefab assigned - creating simple selection UI");
                CreateSimpleSelectionUI();
                return;
            }

            if (_slotContainer == null)
            {
                Debug.LogWarning("[RequiemSelectionScreen] Missing slot container reference");
                return;
            }

            foreach (var requiem in _availableRequiems)
            {
                if (requiem == null) continue;

                var slot = Instantiate(_slotPrefab, _slotContainer);
                slot.Initialize(requiem, OnSlotClicked, OnSlotHovered);
                _slots.Add(slot);
            }

            Debug.Log($"[RequiemSelectionScreen] Initialized {_slots.Count} Requiem slots");
        }

        /// <summary>
        /// Creates a simple fallback UI when no slot prefab is assigned.
        /// Uses prefabs from RuntimeUIPrefabConfig when available.
        /// </summary>
        private void CreateSimpleSelectionUI()
        {
            // Find or create a container
            Transform container = _slotContainer;
            if (container == null)
            {
                container = transform.Find("SlotContainer");
                if (container == null)
                {
                    var containerGO = new GameObject("SlotContainer", typeof(RectTransform));
                    containerGO.transform.SetParent(transform, false);
                    var layout = containerGO.AddComponent<HorizontalLayoutGroup>();
                    layout.spacing = 20f;
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    layout.childForceExpandWidth = false;
                    layout.childForceExpandHeight = false;
                    layout.childControlWidth = false;
                    layout.childControlHeight = false;
                    container = containerGO.transform;

                    // Position it - get existing RectTransform
                    var rect = containerGO.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = new Vector2(0.5f, 0.5f);
                        rect.anchorMax = new Vector2(0.5f, 0.5f);
                        rect.pivot = new Vector2(0.5f, 0.5f);
                        rect.anchoredPosition = new Vector2(0, 30);
                        rect.sizeDelta = new Vector2(900, 350);
                    }
                }
            }

            // Try to get prefab from RuntimeUIPrefabConfig
            var prefabConfig = RuntimeUIPrefabConfigSO.Instance;
            var slotPrefab = prefabConfig != null ? prefabConfig.RequiemSelectionSlotPrefab : null;

            foreach (var requiem in _availableRequiems)
            {
                if (requiem == null) continue;

                // Use prefab if available, otherwise fall back to runtime creation
                if (slotPrefab != null)
                {
                    var slotGO = Instantiate(slotPrefab, container);
                    var slotComponent = slotGO.GetComponent<RequiemSelectionSlotComponent>();
                    if (slotComponent != null)
                    {
                        slotComponent.Initialize(requiem, _aspectIconConfig);
                        slotComponent.OnClicked += OnSlotClicked;
                        _slotComponents.Add(slotComponent);
                    }
                    else
                    {
                        // Prefab missing component - wire manually
                        var button = slotGO.GetComponent<Button>();
                        var selectionBorder = slotGO.transform.Find("SelectionBorder")?.gameObject;
                        if (button != null)
                        {
                            var capturedRequiem = requiem;
                            button.onClick.AddListener(() => OnSlotClicked(capturedRequiem));
                            _simpleSlotButtons.Add((requiem, button, selectionBorder));
                        }
                    }
                }
                else
                {
                    // Fallback: Create character card slot at runtime
                    var slotGO = CreateCharacterSlot(container, requiem);
                    var button = slotGO.GetComponent<Button>();
                    var selectionBorder = slotGO.transform.Find("SelectionBorder")?.gameObject;

                    if (button != null)
                    {
                        _simpleSlotButtons.Add((requiem, button, selectionBorder));
                    }
                }
            }

            // Try to find and wire the Confirm/Start button if not assigned
            if (_startRunButton == null)
            {
                TryFindAndWireConfirmButton();
            }

            int totalSlots = _slotComponents.Count + _simpleSlotButtons.Count;
            Debug.Log($"[RequiemSelectionScreen] Created {totalSlots} character slots ({_slotComponents.Count} from prefab, {_simpleSlotButtons.Count} runtime)");
        }

        /// <summary>
        /// Creates a character slot with full body portrait like the reference design.
        /// </summary>
        private GameObject CreateCharacterSlot(Transform container, RequiemDataSO requiem)
        {
            // Main slot container
            var slotGO = new GameObject($"Slot_{requiem.RequiemName}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            slotGO.transform.SetParent(container, false);

            var slotRect = slotGO.GetComponent<RectTransform>();
            // 1:2 aspect ratio to match portrait dimensions (doubled for larger display)
            slotRect.sizeDelta = new Vector2(350, 700);

            var slotBg = slotGO.GetComponent<Image>();
            slotBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            var button = slotGO.GetComponent<Button>();
            var capturedRequiem = requiem;
            button.onClick.AddListener(() => OnSlotClicked(capturedRequiem));

            // Selection border container (4 edge rectangles for border effect)
            var borderGO = new GameObject("SelectionBorder", typeof(RectTransform));
            borderGO.transform.SetParent(slotGO.transform, false);
            var borderRect = borderGO.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;

            // Border color - blue for selection
            Color borderColor = new Color(0.2f, 0.6f, 1f, 1f); // Bright blue
            float borderWidth = 4f;

            // Top edge
            CreateBorderEdge(borderGO.transform, "TopBorder", borderColor, borderWidth,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -borderWidth), Vector2.zero);
            // Bottom edge
            CreateBorderEdge(borderGO.transform, "BottomBorder", borderColor, borderWidth,
                new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, borderWidth));
            // Left edge
            CreateBorderEdge(borderGO.transform, "LeftBorder", borderColor, borderWidth,
                new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(borderWidth, 0));
            // Right edge
            CreateBorderEdge(borderGO.transform, "RightBorder", borderColor, borderWidth,
                new Vector2(1, 0), new Vector2(1, 1), new Vector2(-borderWidth, 0), Vector2.zero);

            // Start hidden - will show on selection
            borderGO.SetActive(false);

            // Full body portrait image - fills entire slot as background
            var portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGO.transform.SetParent(slotGO.transform, false);
            var portraitRect = portraitGO.GetComponent<RectTransform>();
            // Fill entire slot - no margins
            portraitRect.anchorMin = Vector2.zero;
            portraitRect.anchorMax = Vector2.one;
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;
            var portraitImage = portraitGO.GetComponent<Image>();
            // Don't preserve aspect - stretch to fill like background
            portraitImage.preserveAspect = false;
            portraitImage.raycastTarget = false;
            // Ensure portrait renders behind frame and info panel
            portraitGO.transform.SetAsFirstSibling();

            // Use FullBodySprite if available, otherwise fall back to Portrait
            if (requiem.FullBodySprite != null)
            {
                portraitImage.sprite = requiem.FullBodySprite;
                portraitImage.color = Color.white;
            }
            else if (requiem.Portrait != null)
            {
                portraitImage.sprite = requiem.Portrait;
                portraitImage.color = Color.white;
            }
            else
            {
                // No portrait - show colored placeholder
                portraitImage.color = GetAspectColor(requiem.SoulAspect);
            }

            // Bottom info panel background
            var infoPanelGO = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
            infoPanelGO.transform.SetParent(slotGO.transform, false);
            var infoPanelRect = infoPanelGO.GetComponent<RectTransform>();
            infoPanelRect.anchorMin = new Vector2(0, 0);
            infoPanelRect.anchorMax = new Vector2(1, 0.18f);
            infoPanelRect.offsetMin = Vector2.zero;
            infoPanelRect.offsetMax = Vector2.zero;
            var infoPanelBg = infoPanelGO.GetComponent<Image>();
            infoPanelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            infoPanelBg.raycastTarget = false;

            // Name text
            var nameGO = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(infoPanelGO.transform, false);
            var nameRect = nameGO.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.4f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = new Vector2(8, 0);
            nameRect.offsetMax = new Vector2(-8, -2);
            var nameText = nameGO.GetComponent<TextMeshProUGUI>();
            nameText.text = requiem.RequiemName;
            nameText.fontSize = 24;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.raycastTarget = false;

            // Class/Type text
            var classGO = new GameObject("Class", typeof(RectTransform), typeof(TextMeshProUGUI));
            classGO.transform.SetParent(infoPanelGO.transform, false);
            var classRect = classGO.GetComponent<RectTransform>();
            classRect.anchorMin = new Vector2(0, 0);
            classRect.anchorMax = new Vector2(1, 0.45f);
            classRect.offsetMin = new Vector2(8, 2);
            classRect.offsetMax = new Vector2(-8, 0);
            var classText = classGO.GetComponent<TextMeshProUGUI>();
            classText.text = $"{requiem.Class} | {requiem.SoulAspect}";
            classText.fontSize = 16;
            classText.color = new Color(0.7f, 0.7f, 0.7f);
            classText.alignment = TextAlignmentOptions.Left;
            classText.raycastTarget = false;

            // Top-left badge with aspect icon sprite (72x72)
            var badgeGO = new GameObject("AspectBadge", typeof(RectTransform), typeof(Image));
            badgeGO.transform.SetParent(slotGO.transform, false);
            var badgeRect = badgeGO.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0, 1);
            badgeRect.anchorMax = new Vector2(0, 1);
            badgeRect.pivot = new Vector2(0, 1);
            badgeRect.sizeDelta = new Vector2(72, 72);
            badgeRect.anchoredPosition = new Vector2(8, -8);
            var badgeImage = badgeGO.GetComponent<Image>();
            badgeImage.raycastTarget = false;
            badgeImage.preserveAspect = true;

            // Use aspect icon sprite if available, otherwise fall back to colored square
            var aspectIcon = _aspectIconConfig != null ? _aspectIconConfig.GetIcon(requiem.SoulAspect) : null;
            if (aspectIcon != null)
            {
                badgeImage.sprite = aspectIcon;
                badgeImage.color = Color.white; // Use sprite's native colors
            }
            else
            {
                // Fallback to colored square when no icon config
                badgeImage.color = GetAspectColor(requiem.SoulAspect);
            }

            // HP text in bottom right
            var hpGO = new GameObject("HP", typeof(RectTransform), typeof(TextMeshProUGUI));
            hpGO.transform.SetParent(infoPanelGO.transform, false);
            var hpRect = hpGO.GetComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0.6f, 0);
            hpRect.anchorMax = new Vector2(1, 1);
            hpRect.offsetMin = new Vector2(0, 2);
            hpRect.offsetMax = new Vector2(-8, -2);
            var hpText = hpGO.GetComponent<TextMeshProUGUI>();
            hpText.text = $"HP {requiem.BaseHP}";
            hpText.fontSize = 18;
            hpText.fontStyle = FontStyles.Bold;
            hpText.color = new Color(0.9f, 0.9f, 0.9f);
            hpText.alignment = TextAlignmentOptions.Right;
            hpText.raycastTarget = false;

            return slotGO;
        }

        /// <summary>
        /// Attempts to find and wire the confirm/start run button in the scene.
        /// </summary>
        private void TryFindAndWireConfirmButton()
        {
            // Search for common button names in the hierarchy
            string[] buttonNames = { "ConfirmTeamButton", "StartRunButton", "ConfirmButton", "StartButton", "Confirm", "Start" };

            foreach (var name in buttonNames)
            {
                var found = transform.Find(name);
                if (found == null)
                {
                    // Search recursively
                    found = FindChildRecursive(transform, name);
                }

                if (found != null)
                {
                    var btn = found.GetComponent<Button>();
                    if (btn != null)
                    {
                        _startRunButton = btn;
                        _startRunButton.onClick.RemoveAllListeners();
                        _startRunButton.onClick.AddListener(OnStartRunClicked);
                        Debug.Log($"[RequiemSelectionScreen] Found and wired button: {name}");
                        return;
                    }
                }
            }

            // If still not found, create one
            CreateConfirmButton();
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    return child;

                var found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void CreateConfirmButton()
        {
            // Try to use prefab from RuntimeUIPrefabConfig
            var prefabConfig = RuntimeUIPrefabConfigSO.Instance;
            var btnPrefab = prefabConfig != null ? prefabConfig.ConfirmTeamButtonPrefab : null;

            GameObject btnGO;
            if (btnPrefab != null)
            {
                btnGO = Instantiate(btnPrefab, transform);
                btnGO.name = "ConfirmTeamButton";

                // Position below the slots
                var rect = btnGO.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(0, -150);
                }

                _startRunButton = btnGO.GetComponent<Button>();
                if (_startRunButton != null)
                {
                    _startRunButton.onClick.RemoveAllListeners();
                    _startRunButton.onClick.AddListener(OnStartRunClicked);
                }

                Debug.Log("[RequiemSelectionScreen] Created Confirm Team button from prefab");
            }
            else
            {
                // Fallback: Create at runtime
                btnGO = new GameObject("ConfirmTeamButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnGO.transform.SetParent(transform, false);

                var rect = btnGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, -150); // Below the slots
                rect.sizeDelta = new Vector2(200, 60);

                // Create white texture for button
                var whiteTex = new Texture2D(4, 4);
                var colors = new Color[16];
                for (int i = 0; i < 16; i++) colors[i] = Color.white;
                whiteTex.SetPixels(colors);
                whiteTex.Apply();
                var whiteSprite = Sprite.Create(whiteTex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

                var image = btnGO.GetComponent<Image>();
                image.sprite = whiteSprite;
                image.color = new Color(0.2f, 0.6f, 0.3f, 1f); // Green color

                _startRunButton = btnGO.GetComponent<Button>();
                _startRunButton.onClick.AddListener(OnStartRunClicked);

                // Button text
                var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textGO.transform.SetParent(btnGO.transform, false);
                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                var text = textGO.GetComponent<TextMeshProUGUI>();
                text.text = "CONFIRM TEAM";
                text.fontSize = 24;
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.white;
                text.raycastTarget = false;

                Debug.Log("[RequiemSelectionScreen] Created dynamic Confirm Team button (runtime fallback)");
            }
        }

        private Color GetAspectColor(SoulAspect aspect)
        {
            return aspect switch
            {
                SoulAspect.Flame => new Color(0.9f, 0.3f, 0.2f, 0.8f),
                SoulAspect.Shadow => new Color(0.3f, 0.2f, 0.4f, 0.8f),
                SoulAspect.Light => new Color(0.9f, 0.9f, 0.5f, 0.8f),
                SoulAspect.Nature => new Color(0.3f, 0.7f, 0.3f, 0.8f),
                _ => new Color(0.5f, 0.5f, 0.5f, 0.8f)
            };
        }

        /// <summary>
        /// Creates a single edge of a border rectangle.
        /// </summary>
        private void CreateBorderEdge(Transform parent, string name, Color color, float width,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var edgeGO = new GameObject(name, typeof(RectTransform), typeof(Image));
            edgeGO.transform.SetParent(parent, false);
            var rect = edgeGO.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var image = edgeGO.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private List<(RequiemDataSO requiem, Button button, GameObject selectionBorder)> _simpleSlotButtons = new();

        private void ClearSlots()
        {
            foreach (var slot in _slots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            _slots.Clear();

            // Clear slot components (prefab-based)
            foreach (var slotComponent in _slotComponents)
            {
                if (slotComponent != null)
                {
                    slotComponent.OnClicked -= OnSlotClicked;
                    Destroy(slotComponent.gameObject);
                }
            }
            _slotComponents.Clear();

            // Also clear simple slot buttons (runtime fallback)
            foreach (var (_, button, _) in _simpleSlotButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _simpleSlotButtons.Clear();
        }

        // ============================================
        // Slot Callbacks
        // ============================================

        private void OnSlotClicked(RequiemDataSO requiem)
        {
            if (requiem == null) return;

            if (_selectedRequiems.Contains(requiem))
            {
                // Deselect
                _selectedRequiems.Remove(requiem);
                Debug.Log($"[RequiemSelectionScreen] Deselected: {requiem.RequiemName}");
            }
            else if (_selectedRequiems.Count < MAX_TEAM_SIZE)
            {
                // Select
                _selectedRequiems.Add(requiem);
                Debug.Log($"[RequiemSelectionScreen] Selected: {requiem.RequiemName}");
            }
            else
            {
                Debug.Log($"[RequiemSelectionScreen] Max team size reached ({MAX_TEAM_SIZE})");
            }

            UpdateUI();
        }

        private void OnSlotHovered(RequiemDataSO requiem)
        {
            if (requiem != null)
            {
                ShowPreview(requiem);
            }
        }

        // ============================================
        // UI Updates
        // ============================================

        private void UpdateUI()
        {
            // Update slot selection states (RequiemSlotUI prefab)
            foreach (var slot in _slots)
            {
                if (slot != null && slot.Requiem != null)
                {
                    slot.SetSelected(_selectedRequiems.Contains(slot.Requiem));
                }
            }

            // Update slot component selection states (RequiemSelectionSlotComponent prefab)
            foreach (var slotComponent in _slotComponents)
            {
                if (slotComponent != null && slotComponent.RequiemData != null)
                {
                    slotComponent.SetSelected(_selectedRequiems.Contains(slotComponent.RequiemData));
                }
            }

            // Update simple slot button selection states (runtime fallback)
            foreach (var (requiem, button, selectionBorder) in _simpleSlotButtons)
            {
                if (button != null)
                {
                    bool isSelected = _selectedRequiems.Contains(requiem);

                    // Show/hide blue border based on selection
                    if (selectionBorder != null)
                    {
                        selectionBorder.SetActive(isSelected);
                    }
                }
            }

            // Update selection count text
            if (_selectedCountText != null)
            {
                _selectedCountText.text = $"{_selectedRequiems.Count}/{MAX_TEAM_SIZE}";

                // Color based on selection state
                _selectedCountText.color = _selectedRequiems.Count == MAX_TEAM_SIZE
                    ? new Color(0.3f, 0.9f, 0.3f) // Green when full
                    : Color.white;
            }

            // Update start button
            if (_startRunButton != null)
            {
                _startRunButton.interactable = _selectedRequiems.Count == MAX_TEAM_SIZE;
            }

            // Update team stats
            UpdateTeamStats();
        }

        private void UpdateTeamStats()
        {
            int totalHP = 0;
            int totalATK = 0;
            int totalDEF = 0;

            foreach (var requiem in _selectedRequiems)
            {
                totalHP += requiem.BaseHP;
                totalATK += requiem.BaseATK;
                totalDEF += requiem.BaseDEF;
            }

            if (_teamHPText != null)
                _teamHPText.text = $"Team HP: {totalHP}";

            if (_teamATKText != null)
                _teamATKText.text = $"Team ATK: {totalATK}";

            if (_teamDEFText != null)
                _teamDEFText.text = $"Team DEF: {totalDEF}";
        }

        // ============================================
        // Preview Panel
        // ============================================

        private void ShowPreview(RequiemDataSO requiem)
        {
            _previewedRequiem = requiem;

            if (_previewPanel != null)
                _previewPanel.SetActive(true);

            // Portrait
            if (_previewPortrait != null && requiem.Portrait != null)
                _previewPortrait.sprite = requiem.Portrait;

            // Identity
            if (_previewNameText != null)
                _previewNameText.text = requiem.RequiemName;

            if (_previewTitleText != null)
                _previewTitleText.text = requiem.Title;

            // Classification
            if (_previewClassText != null)
                _previewClassText.text = $"{requiem.Class} - {requiem.SoulAspect}";

            // Stats
            if (_previewStatsText != null)
            {
                _previewStatsText.text = $"HP: {requiem.BaseHP}\n" +
                                         $"ATK: {requiem.BaseATK}\n" +
                                         $"DEF: {requiem.BaseDEF}\n" +
                                         $"SE Rate: {requiem.SERate:F1}x";
            }

            // Backstory
            if (_previewBackstoryText != null)
                _previewBackstoryText.text = requiem.Backstory;

            // Requiem Art
            if (requiem.RequiemArt != null)
            {
                if (_previewArtNameText != null)
                    _previewArtNameText.text = requiem.RequiemArt.ArtName;

                if (_previewArtDescText != null)
                    _previewArtDescText.text = requiem.RequiemArt.Description;
            }

            // Cards
            if (_previewCardsText != null)
                _previewCardsText.text = $"Starting Cards: {requiem.StartingCardCount}";
        }

        private void HidePreview()
        {
            _previewedRequiem = null;

            if (_previewPanel != null)
                _previewPanel.SetActive(false);
        }

        // ============================================
        // Button Handlers
        // ============================================

        public void OnStartRunClicked()
        {
            if (_selectedRequiems.Count != MAX_TEAM_SIZE)
            {
                Debug.LogWarning("[RequiemSelectionScreen] Cannot start run - team not full");
                return;
            }

            // Publish team selection event
            Debug.Log($"[RequiemSelectionScreen] Publishing TeamSelectedEvent with {_selectedRequiems.Count} Requiems. Subscriber count: {EventBus.GetSubscriberCount<TeamSelectedEvent>()}");
            EventBus.Publish(new TeamSelectedEvent(_selectedRequiems.ToArray()));
            Debug.Log("[RequiemSelectionScreen] TeamSelectedEvent published");

            // Start the run via GameManager
            var gameManager = ServiceLocator.Get<IGameManager>();
            if (gameManager != null)
            {
                Debug.Log($"[RequiemSelectionScreen] Starting run with team: " +
                          $"{string.Join(", ", _selectedRequiems.ConvertAll(r => r.RequiemName))}");
                gameManager.StartNewRun();
            }
            else
            {
                Debug.LogError("[RequiemSelectionScreen] GameManager not found!");
            }
        }

        private void OnBackClicked()
        {
            // Navigate back to Bastion
            var gameManager = ServiceLocator.Get<IGameManager>();
            gameManager?.ChangeState(GameState.Bastion);
        }

        public override bool OnBackPressed()
        {
            OnBackClicked();
            return true;
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Get the currently selected Requiems.
        /// </summary>
        public IReadOnlyList<RequiemDataSO> SelectedRequiems => _selectedRequiems;

        /// <summary>
        /// Programmatically select a Requiem.
        /// </summary>
        public void SelectRequiem(RequiemDataSO requiem)
        {
            if (requiem != null && !_selectedRequiems.Contains(requiem) && _selectedRequiems.Count < MAX_TEAM_SIZE)
            {
                _selectedRequiems.Add(requiem);
                UpdateUI();
            }
        }

        /// <summary>
        /// Clear all selections.
        /// </summary>
        public void ClearSelection()
        {
            _selectedRequiems.Clear();
            UpdateUI();
        }
    }
}
