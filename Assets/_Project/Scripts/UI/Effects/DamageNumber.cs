// ============================================
// DamageNumber.cs
// Pooled floating damage/heal number effect
// ============================================

using UnityEngine;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.UI
{
    /// <summary>
    /// Displays floating damage/heal numbers with animation.
    /// Implements IPoolable for efficient reuse.
    /// </summary>
    public class DamageNumber : MonoBehaviour, IPoolable
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("References")]
        [SerializeField, Tooltip("Text component for displaying value")]
        private TextMeshProUGUI _text;

        [Header("Animation")]
        [SerializeField, Tooltip("Distance to float upward")]
        private float _floatDistance = 50f;

        [SerializeField, Tooltip("Total animation duration")]
        private float _duration = 0.8f;

        [SerializeField, Tooltip("Initial scale punch")]
        private float _punchScale = 1.3f;

        [SerializeField, Tooltip("Random horizontal offset range")]
        private float _horizontalRandomness = 20f;

        [Header("Colors")]
        [SerializeField]
        private Color _damageColor = new Color(1f, 0.3f, 0.3f, 1f);

        [SerializeField]
        private Color _healColor = new Color(0.3f, 1f, 0.3f, 1f);

        [SerializeField]
        private Color _blockColor = new Color(0.5f, 0.7f, 1f, 1f);

        [SerializeField]
        private Color _criticalColor = new Color(1f, 0.8f, 0f, 1f);

        // ============================================
        // Runtime
        // ============================================

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Sequence _sequence;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (_text == null)
            {
                _text = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Display a damage/heal number at a world position.
        /// </summary>
        /// <param name="value">Numeric value to display</param>
        /// <param name="type">Type affects color and prefix</param>
        /// <param name="worldPosition">World position to display at</param>
        /// <param name="isCritical">True for critical hit styling</param>
        public void Show(int value, DamageNumberType type, Vector3 worldPosition, bool isCritical = false)
        {
            // Position at world location
            transform.position = worldPosition;

            // Add random horizontal offset
            float randomX = Random.Range(-_horizontalRandomness, _horizontalRandomness);
            _rectTransform.anchoredPosition += new Vector2(randomX, 0);

            // Set text content
            _text.text = type switch
            {
                DamageNumberType.Heal => $"+{value}",
                DamageNumberType.Block => $"[{value}]",
                _ => value.ToString()
            };

            // Set color
            _text.color = isCritical ? _criticalColor : GetColor(type);

            // Set font size for criticals
            _text.fontSize = isCritical ? 28 : 22;

            // Reset state
            _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            // Animate
            PlayAnimation();
        }

        /// <summary>
        /// Display at a screen position (for UI-based numbers).
        /// </summary>
        public void ShowAtScreenPosition(int value, DamageNumberType type, Vector2 screenPosition, bool isCritical = false)
        {
            _rectTransform.position = screenPosition;

            _text.text = type switch
            {
                DamageNumberType.Heal => $"+{value}",
                DamageNumberType.Block => $"[{value}]",
                _ => value.ToString()
            };

            _text.color = isCritical ? _criticalColor : GetColor(type);
            _text.fontSize = isCritical ? 28 : 22;

            _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            PlayAnimation();
        }

        // ============================================
        // Animation
        // ============================================

        private void PlayAnimation()
        {
            // Kill any existing animation
            _sequence?.Kill();

            // Create animation sequence
            _sequence = DOTween.Sequence();

            // Scale punch at start
            _sequence.Append(transform.DOScale(_punchScale, _duration * 0.15f).SetEase(Ease.OutBack));
            _sequence.Append(transform.DOScale(1f, _duration * 0.1f));

            // Float upward
            _sequence.Join(
                _rectTransform.DOAnchorPosY(
                    _rectTransform.anchoredPosition.y + _floatDistance,
                    _duration
                ).SetEase(Ease.OutQuad)
            );

            // Fade out in second half
            _sequence.Insert(
                _duration * 0.5f,
                _canvasGroup.DOFade(0f, _duration * 0.5f)
            );

            // Return to pool when complete
            _sequence.OnComplete(ReturnToPool);
        }

        private Color GetColor(DamageNumberType type)
        {
            return type switch
            {
                DamageNumberType.Damage => _damageColor,
                DamageNumberType.Heal => _healColor,
                DamageNumberType.Block => _blockColor,
                _ => Color.white
            };
        }

        private void ReturnToPool()
        {
            if (ServiceLocator.TryGet<IPoolManager>(out var poolManager))
            {
                poolManager.Return(this);
            }
            else
            {
                // Fallback if no pool manager
                gameObject.SetActive(false);
            }
        }

        // ============================================
        // IPoolable Implementation
        // ============================================

        public void OnSpawnFromPool()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
        }

        public void OnReturnToPool()
        {
            // Kill any running tweens
            _sequence?.Kill();
            _sequence = null;

            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Type of damage number, affects color and formatting.
    /// </summary>
    public enum DamageNumberType
    {
        Damage,
        Heal,
        Block
    }
}
