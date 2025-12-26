// ============================================
// CombatCard.cs
// Card visual for fan layout with drag-to-play targeting
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Cards;
using HNR.Combat;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Card visual component for fan layout with drag-to-play targeting.
    /// Implements IPoolable for object pooling, drag handlers for card play.
    /// </summary>
    /// <remarks>
    /// Works with CardFanLayout for visual positioning.
    /// Uses TargetingSystem for target validation.
    /// Publishes events via EventBus for card play flow.
    /// </remarks>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class CombatCard : MonoBehaviour, IPoolable,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        // ============================================
        // Card Visual Elements
        // ============================================

        [Header("Card Elements")]
        [SerializeField, Tooltip("AP cost text")]
        private TMP_Text _costText;

        [SerializeField, Tooltip("Cost orb background")]
        private Image _costBackground;

        [SerializeField, Tooltip("Card type icon")]
        private Image _typeIcon;

        [SerializeField, Tooltip("Card name text")]
        private TMP_Text _cardNameText;

        [SerializeField, Tooltip("Card description text")]
        private TMP_Text _descriptionText;

        [SerializeField, Tooltip("Card illustration")]
        private Image _cardArt;

        [SerializeField, Tooltip("Card frame/border")]
        private Image _cardFrame;

        [SerializeField, Tooltip("Rarity glow effect")]
        private Image _rarityGlow;

        // ============================================
        // Owner Indicator
        // ============================================

        [Header("Owner Indicator")]
        [SerializeField, Tooltip("Owner portrait image")]
        private Image _ownerPortrait;

        [SerializeField, Tooltip("Owner portrait container")]
        private GameObject _ownerPortraitContainer;

        // ============================================
        // Interaction Feedback
        // ============================================

        [Header("Interaction")]
        [SerializeField, Tooltip("Glow outline for hover/selection")]
        private Image _glowOutline;

        [SerializeField, Tooltip("Selection highlight")]
        private GameObject _selectionHighlight;

        [SerializeField, Tooltip("Unplayable overlay")]
        private CanvasGroup _unplayableOverlay;

        // ============================================
        // Targeting Line
        // ============================================

        [Header("Targeting Line")]
        [SerializeField, Tooltip("Line renderer for bezier targeting")]
        private LineRenderer _targetingLine;

        [SerializeField, Tooltip("Number of segments for bezier curve")]
        private int _bezierSegments = 20;

        [SerializeField, Tooltip("Height of bezier curve arc")]
        private float _bezierArcHeight = 100f;

        [SerializeField, Tooltip("Valid target line color")]
        private Color _validTargetColor = new Color(0f, 0.8f, 0.3f, 1f);

        [SerializeField, Tooltip("Invalid target line color")]
        private Color _invalidTargetColor = new Color(0.8f, 0.2f, 0.2f, 1f);

        // ============================================
        // Type Icons
        // ============================================

        [Header("Type Icons")]
        [SerializeField] private Sprite _strikeIcon;
        [SerializeField] private Sprite _guardIcon;
        [SerializeField] private Sprite _skillIcon;
        [SerializeField] private Sprite _powerIcon;

        // ============================================
        // Animation Settings
        // ============================================

        [Header("Animation")]
        [SerializeField, Tooltip("Duration of glow fade")]
        private float _glowFadeDuration = 0.15f;

        [SerializeField, Tooltip("Glow intensity on hover")]
        private float _glowIntensity = 0.8f;

        [SerializeField, Tooltip("Duration of return animation")]
        private float _returnDuration = 0.2f;

        // ============================================
        // Cached Components
        // ============================================

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;

        // ============================================
        // Runtime State
        // ============================================

        private CardInstance _cardData;
        private Vector2 _originalPosition;
        private Quaternion _originalRotation;
        private Vector3 _originalScale;
        private bool _isDragging;
        private bool _isPlayable = true;
        private ICombatTarget _currentTarget;

        // ============================================
        // Properties
        // ============================================

        /// <summary>RectTransform for positioning.</summary>
        public RectTransform RectTransform => _rectTransform;

        /// <summary>Card instance data.</summary>
        public CardInstance CardData => _cardData;

        /// <summary>True if currently being dragged.</summary>
        public bool IsDragging => _isDragging;

        /// <summary>True if card can be played (has enough AP).</summary>
        public bool IsPlayable => _isPlayable;

        // ============================================
        // Events
        // ============================================

        /// <summary>Fired when pointer enters card.</summary>
        public event Action<CombatCard> OnHoverEnter;

        /// <summary>Fired when pointer exits card.</summary>
        public event Action<CombatCard> OnHoverExit;

        /// <summary>Fired when card is selected (clicked or drag started).</summary>
        public event Action<CombatCard> OnSelected;

        /// <summary>Fired when card drag is completed.</summary>
        public event Action<CombatCard, ICombatTarget> OnDragComplete;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize card with instance data.
        /// </summary>
        /// <param name="data">Card instance to display</param>
        public void Initialize(CardInstance data)
        {
            if (data == null)
            {
                Debug.LogWarning("[CombatCard] Cannot initialize with null data");
                return;
            }

            _cardData = data;
            UpdateVisuals();
        }

        /// <summary>
        /// Update all visual elements from card data.
        /// </summary>
        public void UpdateVisuals()
        {
            if (_cardData == null) return;

            var cardData = _cardData.Data;

            // Cost
            if (_costText != null)
            {
                _costText.text = _cardData.CurrentCost.ToString();
            }

            // Name
            if (_cardNameText != null)
            {
                _cardNameText.text = cardData.CardName;
            }

            // Description
            if (_descriptionText != null)
            {
                _descriptionText.text = cardData.GetFormattedDescription();
            }

            // Art
            if (_cardArt != null && cardData.CardArt != null)
            {
                _cardArt.sprite = cardData.CardArt;
            }

            // Type icon
            if (_typeIcon != null)
            {
                _typeIcon.sprite = GetTypeSprite(cardData.CardType);
            }

            // Frame color based on type
            if (_cardFrame != null)
            {
                _cardFrame.color = GetFrameColor(cardData.CardType);
            }

            // Rarity glow
            if (_rarityGlow != null)
            {
                _rarityGlow.color = UIColors.GetRarityColor(cardData.Rarity);
            }

            // Owner portrait
            if (_ownerPortraitContainer != null)
            {
                if (cardData.Owner != null && _ownerPortrait != null)
                {
                    _ownerPortrait.sprite = cardData.Owner.Portrait;
                    _ownerPortraitContainer.SetActive(true);
                }
                else
                {
                    _ownerPortraitContainer.SetActive(false);
                }
            }

            // Aspect-colored glow outline
            if (_glowOutline != null)
            {
                _glowOutline.color = UIColors.GetAspectColor(cardData.SoulAspect);
                var color = _glowOutline.color;
                color.a = 0f;
                _glowOutline.color = color;
            }

            // Selection highlight off
            if (_selectionHighlight != null)
            {
                _selectionHighlight.SetActive(false);
            }
        }

        /// <summary>
        /// Update playability state based on available AP.
        /// </summary>
        /// <param name="availableAP">Current available Action Points</param>
        public void UpdatePlayability(int availableAP)
        {
            if (_cardData == null)
            {
                _isPlayable = false;
                return;
            }

            _isPlayable = _cardData.CanPlay(availableAP);

            // Visual feedback for unplayable cards
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _isPlayable ? 1f : 0.5f;
            }

            if (_unplayableOverlay != null)
            {
                _unplayableOverlay.alpha = _isPlayable ? 0f : 0.3f;
            }
        }

        // ============================================
        // Drag Handlers
        // ============================================

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isPlayable) return;

            _isDragging = true;

            // Store original transform
            _originalPosition = _rectTransform.anchoredPosition;
            _originalRotation = _rectTransform.localRotation;
            _originalScale = transform.localScale;

            // Disable raycast blocking while dragging
            _canvasGroup.blocksRaycasts = false;

            // Reset rotation and boost scale
            _rectTransform.localRotation = Quaternion.identity;
            transform.localScale = _originalScale * 1.1f;

            // Enable targeting line
            if (_targetingLine != null)
            {
                _targetingLine.gameObject.SetActive(true);
                _targetingLine.positionCount = _bezierSegments;
            }

            // Bring to front
            transform.SetAsLastSibling();

            // Fire events
            OnSelected?.Invoke(this);

            Debug.Log($"[CombatCard] Begin drag: {_cardData?.Data?.CardName}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            // Move card to pointer position
            if (_canvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform,
                    eventData.position,
                    _canvas.worldCamera,
                    out var localPoint);

                _rectTransform.anchoredPosition = localPoint;
            }
            else
            {
                _rectTransform.position = eventData.position;
            }

            // Update targeting line and check for target
            UpdateTargetingLine(eventData.position);
            CheckForTarget(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            _isDragging = false;
            _canvasGroup.blocksRaycasts = true;

            // Hide targeting line
            if (_targetingLine != null)
            {
                _targetingLine.gameObject.SetActive(false);
            }

            // Check if we have a valid target
            if (_currentTarget != null && IsValidTarget(_currentTarget))
            {
                // Successful play - notify listeners
                Debug.Log($"[CombatCard] Play card on target: {_currentTarget.Name}");
                OnDragComplete?.Invoke(this, _currentTarget);

                // Publish event for TurnManager
                EventBus.Publish(new CardTargetConfirmedEvent(_currentTarget));
            }
            else
            {
                // Return to original position
                ReturnToOriginalPosition();
            }

            _currentTarget = null;

            Debug.Log("[CombatCard] End drag");
        }

        // ============================================
        // Pointer Handlers
        // ============================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isDragging) return;

            OnHoverEnter?.Invoke(this);

            // Show aspect-colored glow
            if (_glowOutline != null)
            {
                _glowOutline.DOFade(_glowIntensity, _glowFadeDuration).SetLink(gameObject);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isDragging) return;

            OnHoverExit?.Invoke(this);

            // Hide glow
            if (_glowOutline != null)
            {
                _glowOutline.DOFade(0f, _glowFadeDuration).SetLink(gameObject);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging) return;

            OnSelected?.Invoke(this);
        }

        // ============================================
        // Targeting
        // ============================================

        private void UpdateTargetingLine(Vector2 touchPosition)
        {
            if (_targetingLine == null) return;

            Vector3 start = transform.position;
            Vector3 end = touchPosition;

            // Calculate bezier control point for arc
            Vector3 control = (start + end) / 2f + Vector3.up * _bezierArcHeight;

            // Generate bezier curve points
            for (int i = 0; i < _bezierSegments; i++)
            {
                float t = i / (float)(_bezierSegments - 1);
                Vector3 point = CalculateBezierPoint(t, start, control, end);
                _targetingLine.SetPosition(i, point);
            }

            // Update line color based on target validity
            Color lineColor = _currentTarget != null ? _validTargetColor : _invalidTargetColor;
            _targetingLine.startColor = lineColor;
            _targetingLine.endColor = lineColor;
        }

        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            // Quadratic bezier: B(t) = (1-t)^2*P0 + 2*(1-t)*t*P1 + t^2*P2
            float u = 1f - t;
            return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
        }

        private void CheckForTarget(Vector2 screenPosition)
        {
            _currentTarget = null;

            if (Camera.main == null) return;

            // Convert screen position to world position for 2D raycast
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);

            // Raycast for combat targets
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                var target = hit.collider.GetComponent<ICombatTarget>();
                if (target != null && IsValidTarget(target))
                {
                    _currentTarget = target;
                    target.ShowTargetHighlight(true);
                }
            }
        }

        private bool IsValidTarget(ICombatTarget target)
        {
            if (_cardData == null || target == null) return false;
            if (target.IsDead) return false;

            // Get valid targets from TargetingSystem
            if (ServiceLocator.TryGet<TargetingSystem>(out var targetingSystem))
            {
                var validTargets = targetingSystem.GetAllTargets(_cardData.Data.TargetType);
                return validTargets.Contains(target);
            }

            // Fallback: validate based on target type
            var targetType = _cardData.Data.TargetType;
            return targetType == TargetType.SingleEnemy ||
                   targetType == TargetType.AllEnemies ||
                   targetType == TargetType.Random;
        }

        // ============================================
        // Animation
        // ============================================

        private void ReturnToOriginalPosition()
        {
            _rectTransform.DOAnchorPos(_originalPosition, _returnDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
            _rectTransform.DOLocalRotateQuaternion(_originalRotation, _returnDuration).SetLink(gameObject);
            transform.DOScale(_originalScale, _returnDuration).SetLink(gameObject);

            // Notify fan layout
            ServiceLocator.TryGet<CardFanLayout>(out var fanLayout);
            fanLayout?.DeselectCard();
        }

        /// <summary>
        /// Set selection visual state.
        /// </summary>
        /// <param name="selected">True to show selection highlight</param>
        public void SetSelected(bool selected)
        {
            if (_selectionHighlight != null)
            {
                _selectionHighlight.SetActive(selected);
            }
        }

        // ============================================
        // IPoolable Implementation
        // ============================================

        public void OnSpawnFromPool()
        {
            gameObject.SetActive(true);
            _cardData = null;
            _isDragging = false;
            _isPlayable = true;
            _currentTarget = null;

            // Ensure components are cached (Awake may not have run if object was inactive)
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();

            // Kill any running tweens from previous use
            DOTween.Kill(transform);
            if (_rectTransform != null) DOTween.Kill(_rectTransform);

            // Reset transform
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            if (_rectTransform != null)
            {
                _rectTransform.localRotation = Quaternion.identity;
                _rectTransform.anchoredPosition = Vector2.zero;
            }

            // Reset visuals
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_glowOutline != null)
            {
                DOTween.Kill(_glowOutline);
                var color = _glowOutline.color;
                color.a = 0f;
                _glowOutline.color = color;
            }

            if (_selectionHighlight != null)
            {
                _selectionHighlight.SetActive(false);
            }

            if (_targetingLine != null)
            {
                _targetingLine.gameObject.SetActive(false);
            }

            // Reset unplayable overlay
            if (_unplayableOverlay != null)
            {
                _unplayableOverlay.alpha = 0f;
            }
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
            _cardData = null;

            // Kill any running tweens
            DOTween.Kill(transform);
            DOTween.Kill(_rectTransform);
            if (_glowOutline != null) DOTween.Kill(_glowOutline);

            // Clear event subscriptions
            OnHoverEnter = null;
            OnHoverExit = null;
            OnSelected = null;
            OnDragComplete = null;
        }

        // ============================================
        // Helper Methods
        // ============================================

        private Sprite GetTypeSprite(CardType type) => type switch
        {
            CardType.Strike => _strikeIcon,
            CardType.Guard => _guardIcon,
            CardType.Skill => _skillIcon,
            CardType.Power => _powerIcon,
            _ => _skillIcon
        };

        private Color GetFrameColor(CardType type) => type switch
        {
            CardType.Strike => new Color(0.8f, 0.2f, 0.2f),
            CardType.Guard => new Color(0.2f, 0.4f, 0.8f),
            CardType.Skill => new Color(0.2f, 0.7f, 0.3f),
            CardType.Power => new Color(0.6f, 0.2f, 0.7f),
            _ => Color.white
        };
    }
}
