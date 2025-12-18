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

        [Header("Animation Parameters (HeroEditor)")]
        [SerializeField] private string _attackTrigger = "Slash";
        [SerializeField] private string _jabTrigger = "Jab";
        [SerializeField] private string _hitTrigger = "Hit";
        [SerializeField] private string _deathBoolBack = "DieBack";
        [SerializeField] private string _deathBoolFront = "DieFront";
        [SerializeField] private string _skillTrigger = "Cast";

        [Header("Settings")]
        [SerializeField] private float _attackShakeAmount = 0f; // Disabled - causes position issues
        [SerializeField] private float _hitShakeAmount = 0f; // Disabled - causes position issues

        // ============================================
        // Private State
        // ============================================

        private bool _isAnimating;
        private Coroutine _flashCoroutine;
        private Coroutine _shakeCoroutine;
        private Vector3 _originalPosition;
        private Color _originalColor;

        [Header("Position Lock")]
        [SerializeField] private bool _lockPosition = true;

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
                // HeroEditor prefabs have animator in "Animation" child
                var animationChild = transform.Find("Animation");
                if (animationChild != null)
                {
                    _animator = animationChild.GetComponent<Animator>();
                }

                // Fallback to searching all children
                if (_animator == null)
                {
                    _animator = GetComponentInChildren<Animator>();
                }
            }

            // Disable root motion to prevent animations from moving the character
            if (_animator != null)
            {
                _animator.applyRootMotion = false;
            }

            // For combat visuals, we want to stay at local origin
            // The parent (RequiemInstance) handles world positioning
            _originalPosition = Vector3.zero;
            transform.localPosition = Vector3.zero;

            if (_renderer != null)
            {
                _originalColor = _renderer.color;
            }

            Debug.Log($"[SimpleCharacterVisual] Initialized - Animator: {(_animator != null ? _animator.name : "NULL")}");
        }

        private void LateUpdate()
        {
            // Lock position to prevent animation from moving the character
            if (_lockPosition)
            {
                transform.localPosition = _originalPosition;
            }
        }

        // ============================================
        // ICharacterVisual Implementation
        // ============================================

        public void PlayAttack(AttackType type = AttackType.Slash)
        {
            // Use appropriate attack trigger based on type
            string trigger = type == AttackType.Jab ? _jabTrigger : _attackTrigger;
            TriggerAnimation(trigger);

            if (_attackShakeAmount > 0)
            {
                ShakePosition(_attackShakeAmount, 0.2f);
            }
        }

        public void PlayHit()
        {
            TriggerAnimation(_hitTrigger);

            if (_hitShakeAmount > 0)
            {
                ShakePosition(_hitShakeAmount, 0.15f);
            }
            FlashColor(Color.red, 0.1f);
        }

        public void PlayDeath(bool forward = false)
        {
            // HeroEditor uses bool parameters for death, not triggers
            if (_animator != null)
            {
                string deathBool = forward ? _deathBoolFront : _deathBoolBack;
                _animator.SetBool(deathBool, true);
            }
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
            // HeroEditor doesn't have a dedicated block animation
            // Use Cast as a defensive stance alternative
            TriggerAnimation(_skillTrigger);
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
                Debug.Log($"[SimpleCharacterVisual] Triggered animation: {trigger} on {_animator.name}");
            }
            else
            {
                Debug.LogWarning($"[SimpleCharacterVisual] Cannot trigger animation '{trigger}' - Animator: {(_animator != null ? _animator.name : "NULL")}");
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
