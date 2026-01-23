// ============================================
// RelicShopSlot.cs
// UI component for displaying a relic in the shop overlay
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using HNR.Progression;

namespace HNR.UI.Components
{
    /// <summary>
    /// UI component for displaying a single relic in the relic shop overlay.
    /// Shows relic icon, price, and visual states for selection/purchase.
    /// </summary>
    public class RelicShopSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Relic Display")]
        [SerializeField, Tooltip("Image for relic icon (64x64)")]
        private Image _iconImage;

        [SerializeField, Tooltip("Text for relic price")]
        private TMP_Text _priceText;

        [Header("Visual States")]
        [SerializeField, Tooltip("Background image for state changes")]
        private Image _backgroundImage;

        [SerializeField, Tooltip("Selection highlight border")]
        private GameObject _selectionHighlight;

        [SerializeField, Tooltip("Sold overlay when purchased")]
        private GameObject _soldOverlay;

        [SerializeField, Tooltip("Text on sold overlay")]
        private TMP_Text _soldText;

        [Header("Colors")]
        [SerializeField, Tooltip("Color when item is affordable")]
        private Color _affordableColor = Color.white;

        [SerializeField, Tooltip("Color when item is not affordable")]
        private Color _unaffordableColor = new Color(0.6f, 0.3f, 0.3f, 1f);

        [SerializeField, Tooltip("Background color for selected state")]
        private Color _selectedBackgroundColor = new Color(0.3f, 0.5f, 0.7f, 0.8f);

        [SerializeField, Tooltip("Background color for normal state")]
        private Color _normalBackgroundColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);

        [SerializeField, Tooltip("Background color on hover")]
        private Color _hoverBackgroundColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);

        // ============================================
        // Private Fields
        // ============================================

        private ShopItem _item;
        private Action<ShopItem> _onSelected;
        private bool _isSelected;
        private bool _isHovered;

        // ============================================
        // Properties
        // ============================================

        /// <summary>The shop item (relic) this slot represents.</summary>
        public ShopItem Item => _item;

        /// <summary>Whether this slot is currently selected.</summary>
        public bool IsSelected => _isSelected;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the slot with relic shop item data.
        /// </summary>
        /// <param name="item">Shop item containing relic data.</param>
        /// <param name="onSelected">Callback when slot is selected.</param>
        public void Initialize(ShopItem item, Action<ShopItem> onSelected)
        {
            _item = item;
            _onSelected = onSelected;
            _isSelected = false;
            _isHovered = false;

            // Auto-discover components if not serialized (for dynamically created slots)
            AutoDiscoverComponents();

            SetupDisplay();
        }

        /// <summary>
        /// Auto-discovers child components if serialized references are not set.
        /// Used for dynamically created placeholder slots.
        /// </summary>
        private void AutoDiscoverComponents()
        {
            // Find icon image
            if (_iconImage == null)
            {
                var iconTransform = transform.Find("Icon");
                if (iconTransform != null)
                {
                    _iconImage = iconTransform.GetComponent<Image>();
                }
            }

            // Find price text
            if (_priceText == null)
            {
                var priceTransform = transform.Find("Price");
                if (priceTransform != null)
                {
                    _priceText = priceTransform.GetComponent<TMP_Text>();
                }
            }

            // Find background image (on self)
            if (_backgroundImage == null)
            {
                _backgroundImage = GetComponent<Image>();
            }

            // Find selection highlight
            if (_selectionHighlight == null)
            {
                var highlightTransform = transform.Find("SelectionHighlight");
                if (highlightTransform != null)
                {
                    _selectionHighlight = highlightTransform.gameObject;
                }
            }

            // Find sold overlay
            if (_soldOverlay == null)
            {
                var soldTransform = transform.Find("SoldOverlay");
                if (soldTransform != null)
                {
                    _soldOverlay = soldTransform.gameObject;
                }
            }
        }

        private void SetupDisplay()
        {
            if (_item == null || _item.Type != ShopItemType.Relic) return;

            // Set relic icon
            if (_iconImage != null)
            {
                var icon = _item.RelicData?.Icon;
                _iconImage.sprite = icon;

                // If no icon, show a placeholder color instead of white box
                if (icon == null)
                {
                    _iconImage.color = new Color(0.4f, 0.3f, 0.5f, 0.8f); // Purple placeholder
                }
                else
                {
                    _iconImage.color = Color.white;
                }
            }

            // Set price
            if (_priceText != null)
            {
                _priceText.text = _item.Price.ToString();
            }

            // Initial visual state
            if (_selectionHighlight != null)
            {
                _selectionHighlight.SetActive(false);
            }

            if (_soldOverlay != null)
            {
                _soldOverlay.SetActive(false);
            }
        }

        // ============================================
        // Visual Updates
        // ============================================

        /// <summary>
        /// Update visual state based on affordability.
        /// </summary>
        /// <param name="currentShards">Player's current Void Shard balance.</param>
        public void UpdateVisuals(int currentShards)
        {
            if (_item == null) return;

            bool isPurchased = _item.IsPurchased;
            bool canAfford = _item.CanPurchase(currentShards);

            // Update sold overlay
            if (_soldOverlay != null)
            {
                _soldOverlay.SetActive(isPurchased);
            }

            // Update price color
            if (_priceText != null)
            {
                if (isPurchased)
                {
                    _priceText.color = Color.gray;
                }
                else
                {
                    _priceText.color = canAfford ? _affordableColor : _unaffordableColor;
                }
            }

            // Update icon opacity (preserve RGB, only change alpha)
            if (_iconImage != null)
            {
                var color = _iconImage.color;
                color.a = isPurchased ? 0.4f : (_iconImage.sprite == null ? 0.8f : 1f);
                _iconImage.color = color;
            }

            UpdateBackgroundColor();
        }

        /// <summary>
        /// Set the selection state of this slot.
        /// </summary>
        /// <param name="selected">Whether this slot is selected.</param>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (_selectionHighlight != null)
            {
                _selectionHighlight.SetActive(selected);
            }

            UpdateBackgroundColor();
        }

        private void UpdateBackgroundColor()
        {
            if (_backgroundImage == null) return;

            if (_item?.IsPurchased == true)
            {
                _backgroundImage.color = Color.gray * 0.4f;
            }
            else if (_isSelected)
            {
                _backgroundImage.color = _selectedBackgroundColor;
            }
            else if (_isHovered)
            {
                _backgroundImage.color = _hoverBackgroundColor;
            }
            else
            {
                _backgroundImage.color = _normalBackgroundColor;
            }
        }

        // ============================================
        // Event Handlers
        // ============================================

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_item == null || _item.IsPurchased) return;

            _onSelected?.Invoke(_item);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_item?.IsPurchased == true) return;

            _isHovered = true;
            UpdateBackgroundColor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateBackgroundColor();
        }
    }
}
