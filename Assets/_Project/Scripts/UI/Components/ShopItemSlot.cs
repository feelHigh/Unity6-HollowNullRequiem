// ============================================
// ShopItemSlot.cs
// UI component for displaying a shop item in the grid
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using HNR.Progression;

namespace HNR.UI
{
    /// <summary>
    /// UI component for displaying a single shop item in the item grid.
    /// Handles visual states and selection callbacks.
    /// </summary>
    public class ShopItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Item Display")]
        [SerializeField, Tooltip("Image for item icon")]
        private Image _iconImage;

        [SerializeField, Tooltip("Text for item name")]
        private TMP_Text _nameText;

        [SerializeField, Tooltip("Text for item price")]
        private TMP_Text _priceText;

        [SerializeField, Tooltip("Icon indicating item type")]
        private Image _typeIcon;

        [Header("Visual States")]
        [SerializeField, Tooltip("Background image for state changes")]
        private Image _backgroundImage;

        [SerializeField, Tooltip("Selection highlight object")]
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
        private Color _selectedBackgroundColor = new Color(0.2f, 0.6f, 0.8f, 0.5f);

        [SerializeField, Tooltip("Background color for normal state")]
        private Color _normalBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);

        [SerializeField, Tooltip("Background color on hover")]
        private Color _hoverBackgroundColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        [Header("Type Icons")]
        [SerializeField, Tooltip("Sprite for card type")]
        private Sprite _cardTypeSprite;

        [SerializeField, Tooltip("Sprite for relic type")]
        private Sprite _relicTypeSprite;

        [SerializeField, Tooltip("Sprite for consumable type")]
        private Sprite _consumableTypeSprite;

        [SerializeField, Tooltip("Sprite for service type (remove/purify)")]
        private Sprite _serviceTypeSprite;

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

        /// <summary>The shop item this slot represents.</summary>
        public ShopItem Item => _item;

        /// <summary>Whether this slot is currently selected.</summary>
        public bool IsSelected => _isSelected;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the slot with item data and selection callback.
        /// </summary>
        /// <param name="item">Shop item to display.</param>
        /// <param name="onSelected">Callback when slot is selected.</param>
        public void Initialize(ShopItem item, Action<ShopItem> onSelected)
        {
            _item = item;
            _onSelected = onSelected;
            _isSelected = false;
            _isHovered = false;

            SetupDisplay();
        }

        private void SetupDisplay()
        {
            if (_item == null) return;

            // Set icon
            if (_iconImage != null)
            {
                _iconImage.sprite = _item.Icon;
                _iconImage.gameObject.SetActive(_item.Icon != null);
            }

            // Set name
            if (_nameText != null)
            {
                _nameText.text = _item.DisplayName;
            }

            // Set price
            if (_priceText != null)
            {
                _priceText.text = _item.Price.ToString();
            }

            // Set type icon
            if (_typeIcon != null)
            {
                _typeIcon.sprite = GetTypeSprite(_item.Type);
                _typeIcon.gameObject.SetActive(_typeIcon.sprite != null);
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

        private Sprite GetTypeSprite(ShopItemType type)
        {
            return type switch
            {
                ShopItemType.Card => _cardTypeSprite,
                ShopItemType.Relic => _relicTypeSprite,
                ShopItemType.Consumable => _consumableTypeSprite,
                ShopItemType.CardRemove => _serviceTypeSprite,
                ShopItemType.Purification => _serviceTypeSprite,
                _ => null
            };
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

            // Update name color
            if (_nameText != null)
            {
                _nameText.color = isPurchased ? Color.gray : Color.white;
            }

            // Update icon opacity
            if (_iconImage != null)
            {
                var color = _iconImage.color;
                color.a = isPurchased ? 0.5f : 1f;
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
                _backgroundImage.color = Color.gray * 0.5f;
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
