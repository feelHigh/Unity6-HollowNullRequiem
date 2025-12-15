// ============================================
// SimpleCharacterVisual.cs
// Fallback ICharacterVisual for simple sprite-based characters
// ============================================

using System.Collections;
using UnityEngine;

namespace HNR.Characters.Visuals
{
    /// <summary>
    /// Simple ICharacterVisual implementation for characters without HeroEditor.
    /// Uses basic sprite operations for visual feedback.
    /// </summary>
    public class SimpleCharacterVisual : MonoBehaviour, ICharacterVisual
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("References")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Animator _animator;

        [Header("Animation Parameters")]
        [SerializeField] private string _attackTrigger = "Attack";
        [SerializeField] private string _hitTrigger = "Hit";
        [SerializeField] private string _deathTrigger = "Death";
        [SerializeField] private string _blockTrigger = "Block";
        [SerializeField] private string _skillTrigger = "Skill";

        [Header("Settings")]
        [SerializeField] private float _attackShakeAmount = 0.1f;
        [SerializeField] private float _hitShakeAmount = 0.15f;

        // ============================================
        // Private State
        // ============================================

        private bool _isAnimating;
        private Coroutine _flashCoroutine;
        private Coroutine _shakeCoroutine;
        private Vector3 _originalPosition;
        private Color _originalColor;

        // ============================================
        // Properties
        // ============================================

        public bool IsAnimating => _isAnimating;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            _originalPosition = transform.localPosition;

            if (_renderer != null)
            {
                _originalColor = _renderer.color;
            }
        }

        // ============================================
        // ICharacterVisual Implementation
        // ============================================

        public void PlayAttack(AttackType type = AttackType.Slash)
        {
            TriggerAnimation(_attackTrigger);
            ShakePosition(_attackShakeAmount, 0.2f);
        }

        public void PlayHit()
        {
            TriggerAnimation(_hitTrigger);
            ShakePosition(_hitShakeAmount, 0.15f);
            FlashColor(Color.red, 0.1f);
        }

        public void PlayDeath(bool forward = false)
        {
            TriggerAnimation(_deathTrigger);
            _isAnimating = true;
        }

        public void SetIdle()
        {
            _isAnimating = false;
            // Animator will return to idle automatically if using trigger-based animations
        }

        public void SetExpression(string expression)
        {
            // Simple visuals don't support expressions
        }

        public void FlashColor(Color color, float duration)
        {
            if (_renderer == null) return;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashColorCoroutine(color, duration));
        }

        public void PlaySkill()
        {
            TriggerAnimation(_skillTrigger);
            ShakePosition(_attackShakeAmount * 1.5f, 0.3f);
        }

        public void PlayBlock()
        {
            TriggerAnimation(_blockTrigger);
        }

        public void SetFacing(bool faceRight)
        {
            if (_renderer != null)
            {
                _renderer.flipX = !faceRight;
            }
            else
            {
                Vector3 scale = transform.localScale;
                scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        // ============================================
        // Private Methods
        // ============================================

        private void TriggerAnimation(string trigger)
        {
            if (_animator != null && !string.IsNullOrEmpty(trigger))
            {
                _animator.SetTrigger(trigger);
            }
        }

        private void ShakePosition(float amount, float duration)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                transform.localPosition = _originalPosition;
            }

            _shakeCoroutine = StartCoroutine(ShakeCoroutine(amount, duration));
        }

        private IEnumerator ShakeCoroutine(float amount, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-amount, amount);
                float y = Random.Range(-amount, amount);
                transform.localPosition = _originalPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originalPosition;
            _shakeCoroutine = null;
        }

        private IEnumerator FlashColorCoroutine(Color color, float duration)
        {
            if (_renderer == null) yield break;

            _renderer.color = color;
            yield return new WaitForSeconds(duration);
            _renderer.color = _originalColor;

            _flashCoroutine = null;
        }

        // ============================================
        // Public Utilities
        // ============================================

        /// <summary>
        /// Set the sprite to display.
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            if (_renderer != null)
            {
                _renderer.sprite = sprite;
            }
        }

        /// <summary>
        /// Set the base color.
        /// </summary>
        public void SetColor(Color color)
        {
            if (_renderer != null)
            {
                _renderer.color = color;
                _originalColor = color;
            }
        }
    }
}
