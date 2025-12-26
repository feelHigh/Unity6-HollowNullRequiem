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

            // Try to use Card.prefab if available
            if (_cardItemPrefab != null)
            {
                var cardComponent = _cardItemPrefab.GetComponent<Card>();
                if (cardComponent != null)
                {
                    // Using Card.prefab - instantiate and setup
                    var cardObj = Instantiate(_cardItemPrefab, _cardContainer);
                    var card = cardObj.GetComponent<Card>();
                    if (card != null)
                    {
                        card.SetCard(cardData);

                        // Add wrapper item for tracking
                        var item = cardObj.AddComponent<CardDisplayItem>();
                        item.SetCardData(cardData);
                        _cardItems.Add(item);

                        // Animate entrance
                        cardObj.transform.localScale = Vector3.zero;
                        cardObj.transform.DOScale(1f, 0.3f)
                            .SetDelay(animationDelay)
                            .SetEase(Ease.OutBack)
                            .SetLink(cardObj);
                        return;
                    }
                }

                // Fallback to CardDisplayItem prefab
                var itemObj = Instantiate(_cardItemPrefab, _cardContainer);
                var displayItem = itemObj.GetComponent<CardDisplayItem>();
                if (displayItem != null)
                {
                    displayItem.SetCardData(cardData);
                    displayItem.PlayEntranceAnimation(animationDelay);
                    _cardItems.Add(displayItem);
                    return;
                }
            }

            // Final fallback: Create simple text display
            CreateSimpleCardItem(cardData, animationDelay);
        }

        private void CreateSimpleCardItem(CardDataSO cardData, float animationDelay)
        {
            if (_cardContainer == null) return;

            // Create a simple GameObject with card info
            var itemObj = new GameObject($"Card_{cardData.CardName}");
            itemObj.transform.SetParent(_cardContainer, false);

            // Add layout element for grid
            var layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 170;
            layoutElement.preferredWidth = 120;

            // Add background
            var image = itemObj.AddComponent<Image>();
            image.color = GetCardTypeColor(cardData.CardType);

            // Create cost badge
            var costBadge = new GameObject("CostBadge");
            costBadge.transform.SetParent(itemObj.transform, false);
            var costRect = costBadge.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 1);
            costRect.anchorMax = new Vector2(0, 1);
            costRect.pivot = new Vector2(0, 1);
            costRect.sizeDelta = new Vector2(30, 30);
            costRect.anchoredPosition = new Vector2(5, -5);

            var costBg = costBadge.AddComponent<Image>();
            costBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            var costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(costBadge.transform, false);
            var costTextRect = costTextObj.AddComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.offsetMin = Vector2.zero;
            costTextRect.offsetMax = Vector2.zero;

            var costText = costTextObj.AddComponent<TextMeshProUGUI>();
            costText.text = cardData.APCost.ToString();
            costText.fontSize = 16;
            costText.fontStyle = FontStyles.Bold;
            costText.color = Color.white;
            costText.alignment = TextAlignmentOptions.Center;

            // Create card name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.7f);
            nameRect.anchorMax = new Vector2(1, 0.85f);
            nameRect.offsetMin = new Vector2(5, 0);
            nameRect.offsetMax = new Vector2(-5, 0);

            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = cardData.CardName;
            nameText.fontSize = 12;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;

            // Create card type
            var typeObj = new GameObject("Type");
            typeObj.transform.SetParent(itemObj.transform, false);
            var typeRect = typeObj.AddComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0, 0.55f);
            typeRect.anchorMax = new Vector2(1, 0.7f);
            typeRect.offsetMin = new Vector2(5, 0);
            typeRect.offsetMax = new Vector2(-5, 0);

            var typeText = typeObj.AddComponent<TextMeshProUGUI>();
            typeText.text = cardData.CardType.ToString();
            typeText.fontSize = 10;
            typeText.color = new Color(0.8f, 0.8f, 0.8f);
            typeText.alignment = TextAlignmentOptions.Center;

            // Create description
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(itemObj.transform, false);
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.05f);
            descRect.anchorMax = new Vector2(1, 0.55f);
            descRect.offsetMin = new Vector2(8, 5);
            descRect.offsetMax = new Vector2(-8, -5);

            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = cardData.Description;
            descText.fontSize = 9;
            descText.color = Color.white;
            descText.alignment = TextAlignmentOptions.Center;
            descText.textWrappingMode = TextWrappingModes.Normal;

            // Add simple item component
            var item = itemObj.AddComponent<CardDisplayItem>();
            item.SetCardData(cardData);
            _cardItems.Add(item);

            // Animate entrance
            itemObj.transform.localScale = Vector3.zero;
            itemObj.transform.DOScale(1f, 0.3f)
                .SetDelay(animationDelay)
                .SetEase(Ease.OutBack)
                .SetLink(itemObj);
        }

        private Color GetCardTypeColor(CardType cardType)
        {
            return cardType switch
            {
                CardType.Strike => new Color(0.6f, 0.2f, 0.2f, 0.9f),
                CardType.Guard => new Color(0.2f, 0.35f, 0.6f, 0.9f),
                CardType.Skill => new Color(0.2f, 0.5f, 0.25f, 0.9f),
                CardType.Power => new Color(0.45f, 0.2f, 0.55f, 0.9f),
                _ => new Color(0.25f, 0.25f, 0.3f, 0.9f)
            };
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
