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

namespace HNR.Cards
{
    /// <summary>
    /// Visual representation of a card in hand/play.
    /// Handles display, interaction, and pooling.
    /// </summary>
    public class Card : MonoBehaviour, IPoolable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // ============================================
        // Visual References
        // ============================================

        [Header("Visual References")]
        [SerializeField, Tooltip("Card illustration")]
        private Image _cardArt;

        [SerializeField, Tooltip("Card frame/border")]
        private Image _cardFrame;

        [SerializeField, Tooltip("Card name display")]
        private TextMeshProUGUI _nameText;

        [SerializeField, Tooltip("AP cost display")]
        private TextMeshProUGUI _costText;

        [SerializeField, Tooltip("Card effect description")]
        private TextMeshProUGUI _descriptionText;

        [SerializeField, Tooltip("Selection highlight effect")]
        private GameObject _selectionGlow;

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

            if (_cardFrame != null)
            {
                _cardFrame.color = GetFrameColor(data.CardType);
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
