// ============================================
// Card.cs
// Visual representation of a card in hand/play
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using HNR.Core.Interfaces;
using HNR.UI;
using HNR.UI.Config;

namespace HNR.Cards
{
    /// <summary>
    /// Visual representation of a card in hand/play.
    /// Handles display, interaction, and pooling.
    /// Implements ICardDisplay for use in non-combat screens (Sanctuary, Results, Treasure).
    /// </summary>
    public class Card : MonoBehaviour, IPoolable, ICardDisplay, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // ============================================
        // Visual References
        // ============================================

        [Header("Visual References")]
        [SerializeField, Tooltip("Card illustration")]
        private Image _cardArt;

        [SerializeField, Tooltip("Card frame/border (legacy - use _cardBorder for new prefabs)")]
        private Image _cardFrame;

        [SerializeField, Tooltip("Card border layer (Border or BorderGem based on rarity)")]
        private Image _cardBorder;

        [SerializeField, Tooltip("Card background/mask layer")]
        private Image _cardBackground;

        [SerializeField, Tooltip("Card name display")]
        private TextMeshProUGUI _nameText;

        [SerializeField, Tooltip("AP cost display")]
        private TextMeshProUGUI _costText;

        [SerializeField, Tooltip("Card effect description")]
        private TextMeshProUGUI _descriptionText;

        [SerializeField, Tooltip("Selection highlight effect")]
        private GameObject _selectionGlow;

        // ============================================
        // Cost Frame Elements (Layered Sprites)
        // ============================================

        [Header("Cost Frame Elements")]
        [SerializeField, Tooltip("Cost frame background layer")]
        private Image _costBg;

        [SerializeField, Tooltip("Cost frame border layer")]
        private Image _costBorder;

        [SerializeField, Tooltip("Cost frame gradient layer")]
        private Image _costGradient;

        [SerializeField, Tooltip("Cost frame inner border layer")]
        private Image _costInnerBorder;

        // ============================================
        // Sprite Configuration
        // ============================================

        [Header("Sprite Configuration")]
        [SerializeField, Tooltip("Card sprite configuration asset")]
        private CardSpriteConfigSO _spriteConfig;

        // ============================================
        // Type Colors
        // ============================================

        [Header("Type Colors")]
        [SerializeField, Tooltip("Strike card frame color")]
        private Color _strikeColor = new Color(0.8f, 0.2f, 0.2f);

        [SerializeField, Tooltip("Guard card frame color")]
        private Color _guardColor = new Color(0.2f, 0.4f, 0.8f);

        [SerializeField, Tooltip("Skill card frame color")]
        private Color _skillColor = new Color(0.2f, 0.7f, 0.3f);

        [SerializeField, Tooltip("Power card frame color")]
        private Color _powerColor = new Color(0.6f, 0.2f, 0.7f);

        // ============================================
        // Runtime State
        // ============================================

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private CardInstance _cardInstance;
        private bool _isSelected;
        private int _sortOrder;

        // ============================================
        // Properties
        // ============================================

        /// <summary>The card instance data this visual represents.</summary>
        public CardInstance CardInstance => _cardInstance;

        /// <summary>Whether this card is currently selected.</summary>
        public bool IsSelected => _isSelected;

        /// <summary>Current sort order for rendering.</summary>
        public int SortOrder => _sortOrder;

        // ============================================
        // Events
        // ============================================

        /// <summary>Fired when card is clicked.</summary>
        public event Action<Card> OnCardClicked;

        /// <summary>Fired when pointer enters card.</summary>
        public event Action<Card> OnCardHoverEnter;

        /// <summary>Fired when pointer exits card.</summary>
        public event Action<Card> OnCardHoverExit;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_selectionGlow != null)
            {
                _selectionGlow.SetActive(false);
            }
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize card with instance data.
        /// </summary>
        /// <param name="instance">Card instance to display</param>
        public void Initialize(CardInstance instance)
        {
            _cardInstance = instance;
            UpdateVisuals();
        }

        /// <summary>
        /// Set card display from CardDataSO (ICardDisplay implementation).
        /// Creates a temporary CardInstance for display purposes.
        /// Used by non-combat screens (Sanctuary, Results, Treasure).
        /// </summary>
        /// <param name="card">Card data to display</param>
        public void SetCard(CardDataSO card)
        {
            if (card == null)
            {
                Debug.LogWarning("[Card] SetCard called with null card data");
                return;
            }

            // Create a temporary CardInstance for display
            _cardInstance = new CardInstance(card);
            UpdateVisuals();
        }

        /// <summary>
        /// Update all visual elements from card data.
        /// </summary>
        public void UpdateVisuals()
        {
            if (_cardInstance == null) return;

            var data = _cardInstance.Data;

            if (_nameText != null)
            {
                _nameText.text = data.CardName;
            }

            if (_costText != null)
            {
                _costText.text = _cardInstance.CurrentCost.ToString();
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = data.GetFormattedDescription();
            }

            if (_cardArt != null && data.CardArt != null)
            {
                _cardArt.sprite = data.CardArt;
            }

            // Apply frame sprites from config (or fallback to legacy tinting)
            ApplyFrameSprites(data);

            // Apply cost frame sprites
            ApplyCostFrameSprites(data);
        }

        /// <summary>
        /// Apply frame sprites from config or fallback to legacy color tinting.
        /// </summary>
        private void ApplyFrameSprites(CardDataSO cardData)
        {
            if (_spriteConfig != null)
            {
                // New sprite-based approach: pre-colored sprites, no tinting needed
                if (_cardBackground != null)
                {
                    _cardBackground.sprite = _spriteConfig.GetBackgroundSprite(cardData.CardType);
                    _cardBackground.color = Color.white;
                }

                if (_cardBorder != null)
                {
                    _cardBorder.sprite = _spriteConfig.GetBorderSprite(cardData.CardType, cardData.Rarity);
                    _cardBorder.color = Color.white;
                }

                // Also update legacy frame if present (for backwards compatibility)
                if (_cardFrame != null)
                {
                    _cardFrame.color = Color.white;
                }
            }
            else
            {
                // Legacy fallback: tint white sprite with type color
                if (_cardFrame != null)
                {
                    _cardFrame.color = GetFrameColor(cardData.CardType);
                }
            }
        }

        /// <summary>
        /// Apply cost frame sprites with type-based tinting.
        /// </summary>
        private void ApplyCostFrameSprites(CardDataSO cardData)
        {
            if (_spriteConfig == null) return;

            Color costTint = _spriteConfig.GetCostFrameTint(cardData.CardType);

            if (_costBg != null)
            {
                _costBg.sprite = _spriteConfig.CostFrameBg;
                _costBg.color = costTint;
            }

            if (_costBorder != null)
            {
                _costBorder.sprite = _spriteConfig.CostFrameBorder;
                _costBorder.color = costTint;
            }

            if (_costGradient != null)
            {
                _costGradient.sprite = _spriteConfig.CostFrameGradient;
                _costGradient.color = costTint;
            }

            if (_costInnerBorder != null)
            {
                _costInnerBorder.sprite = _spriteConfig.CostFrameInnerBorder;
                _costInnerBorder.color = costTint;
            }
        }

        /// <summary>
        /// Get frame color based on card type.
        /// </summary>
        private Color GetFrameColor(CardType type)
        {
            return type switch
            {
                CardType.Strike => _strikeColor,
                CardType.Guard => _guardColor,
                CardType.Skill => _skillColor,
                CardType.Power => _powerColor,
                _ => Color.white
            };
        }

        // ============================================
        // State Management
        // ============================================

        /// <summary>
        /// Set selection state with visual feedback.
        /// </summary>
        /// <param name="selected">Whether card is selected</param>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (_selectionGlow != null)
            {
                _selectionGlow.SetActive(selected);
            }
        }

        /// <summary>
        /// Set whether card is playable (has enough AP).
        /// </summary>
        /// <param name="playable">Whether card can be played</param>
        public void SetPlayable(bool playable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = playable ? 1f : 0.5f;
            }
        }

        /// <summary>
        /// Set rendering sort order.
        /// </summary>
        /// <param name="order">Sort order value</param>
        public void SetSortOrder(int order)
        {
            _sortOrder = order;

            if (_canvas != null)
            {
                _canvas.sortingOrder = order;
            }
        }

        // ============================================
        // Event System Handlers
        // ============================================

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCardClicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnCardHoverEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnCardHoverExit?.Invoke(this);
        }

        // ============================================
        // IPoolable Implementation
        // ============================================

        public void OnSpawnFromPool()
        {
            gameObject.SetActive(true);
            _cardInstance = null;
            SetSelected(false);
            SetPlayable(true);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        public void OnReturnToPool()
        {
            _cardInstance = null;
            SetSelected(false);
            OnCardClicked = null;
            OnCardHoverEnter = null;
            OnCardHoverExit = null;
            gameObject.SetActive(false);
        }
    }
}
