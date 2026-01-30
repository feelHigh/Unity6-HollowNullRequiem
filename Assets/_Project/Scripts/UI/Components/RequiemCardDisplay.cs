// ============================================
// RequiemCardDisplay.cs
// Display component for Requiem's card collection
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Cards;
using HNR.Characters;
using HNR.UI.Config;

namespace HNR.UI.Components
{
    /// <summary>
    /// Displays a list of cards for a Requiem character.
    /// Used in the Requiem detail panel to show starting cards.
    /// </summary>
    public class RequiemCardDisplay : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Layout")]
        [SerializeField, Tooltip("Container for card items")]
        private Transform _cardContainer;

        [SerializeField, Tooltip("Card item prefab")]
        private GameObject _cardItemPrefab;

        [SerializeField, Tooltip("Section title text")]
        private TMP_Text _sectionTitle;

        [Header("Configuration")]
        [SerializeField, Tooltip("Maximum cards to display")]
        private int _maxCardsToDisplay = 8;

        [SerializeField, Tooltip("Animation stagger delay")]
        private float _staggerDelay = 0.05f;

        // ============================================
        // State
        // ============================================

        private List<CardDisplayItem> _cardItems = new();

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void OnDestroy()
        {
            // Only clear cards when destroyed, not when disabled
            // This prevents cards from disappearing when switching tabs
            ClearCards();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Displays the starting cards for a Requiem.
        /// </summary>
        public void DisplayStartingCards(RequiemDataSO requiemData)
        {
            if (requiemData == null)
            {
                ClearCards();
                return;
            }

            // Update title
            if (_sectionTitle != null)
            {
                _sectionTitle.text = "Starting Cards";
            }

            // Get starting cards
            var startingCards = requiemData.StartingCards;
            if (startingCards == null || startingCards.Count == 0)
            {
                ClearCards();
                return;
            }

            // Display cards
            DisplayCards(startingCards);
        }

        /// <summary>
        /// Displays a list of cards.
        /// </summary>
        public void DisplayCards(IReadOnlyList<CardDataSO> cards)
        {
            ClearCards();

            if (cards == null || cards.Count == 0)
            {
                return;
            }

            // Limit cards to display
            int count = Mathf.Min(cards.Count, _maxCardsToDisplay);

            for (int i = 0; i < count; i++)
            {
                var cardData = cards[i];
                if (cardData == null) continue;

                CreateCardItem(cardData, i * _staggerDelay);
            }
        }

        /// <summary>
        /// Clears all displayed cards.
        /// </summary>
        public void ClearCards()
        {
            foreach (var item in _cardItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _cardItems.Clear();
        }

        /// <summary>
        /// Sets the section title.
        /// </summary>
        public void SetTitle(string title)
        {
            if (_sectionTitle != null)
            {
                _sectionTitle.text = title;
            }
        }

        // ============================================
        // Card Item Creation
        // ============================================

        private void CreateCardItem(CardDataSO cardData, float animationDelay)
        {
            if (_cardContainer == null) return;

            // Use local prefab or fall back to config
            var prefab = _cardItemPrefab ?? RuntimeUIPrefabConfigSO.Instance?.SimpleCardDisplayItemPrefab;

            if (prefab == null)
            {
                Debug.LogError("[RequiemCardDisplay] Card item prefab not assigned. Check RuntimeUIPrefabConfig.");
                return;
            }

            var cardObj = Instantiate(prefab, _cardContainer);
            cardObj.name = $"Card_{cardData.CardName}";

            // Try Card component first
            var cardComponent = cardObj.GetComponent<Card>();
            if (cardComponent != null)
            {
                cardComponent.SetCard(cardData);

                // Add wrapper item for tracking
                var item = cardObj.GetComponent<CardDisplayItem>();
                if (item == null)
                {
                    item = cardObj.AddComponent<CardDisplayItem>();
                }
                item.SetCardData(cardData);
                _cardItems.Add(item);
            }
            else
            {
                // Fallback to CardDisplayItem
                var displayItem = cardObj.GetComponent<CardDisplayItem>();
                if (displayItem != null)
                {
                    displayItem.SetCardData(cardData);
                    _cardItems.Add(displayItem);
                }
                else
                {
                    // Add one if missing
                    displayItem = cardObj.AddComponent<CardDisplayItem>();
                    displayItem.SetCardData(cardData);
                    _cardItems.Add(displayItem);
                }
            }

            // Animate entrance
            cardObj.transform.localScale = Vector3.zero;
            cardObj.transform.DOScale(1f, 0.3f)
                .SetDelay(animationDelay)
                .SetEase(Ease.OutBack)
                .SetLink(cardObj);
        }
    }

    /// <summary>
    /// Individual card display item component.
    /// </summary>
    public class CardDisplayItem : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [SerializeField] private Image _cardImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _typeText;
        [SerializeField] private TMP_Text _descriptionText;

        // ============================================
        // State
        // ============================================

        private CardDataSO _cardData;
        private Tween _currentTween;

        // ============================================
        // Properties
        // ============================================

        public CardDataSO CardData => _cardData;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void OnDestroy()
        {
            _currentTween?.Kill();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Sets the card data and updates display.
        /// </summary>
        public void SetCardData(CardDataSO cardData)
        {
            _cardData = cardData;

            if (cardData == null) return;

            // Update card image
            if (_cardImage != null && cardData.CardArt != null)
            {
                _cardImage.sprite = cardData.CardArt;
            }

            // Update name
            if (_nameText != null)
            {
                _nameText.text = cardData.CardName;
            }

            // Update cost
            if (_costText != null)
            {
                _costText.text = cardData.APCost.ToString();
            }

            // Update type
            if (_typeText != null)
            {
                _typeText.text = cardData.CardType.ToString();
            }

            // Update description
            if (_descriptionText != null)
            {
                _descriptionText.text = cardData.Description;
            }
        }

        /// <summary>
        /// Plays entrance animation.
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
    }
}
