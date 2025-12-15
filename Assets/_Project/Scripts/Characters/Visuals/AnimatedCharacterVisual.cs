// ============================================
// AnimatedCharacterVisual.cs
// HeroEditor Character wrapper implementing ICharacterVisual
// ============================================

using System.Collections;
using UnityEngine;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;

namespace HNR.Characters.Visuals
{
    /// <summary>
    /// ICharacterVisual implementation wrapping HeroEditor's Character component.
    /// Provides high-level animation control through HeroEditor's built-in methods.
    /// </summary>
    public class AnimatedCharacterVisual : MonoBehaviour, ICharacterVisual
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("HeroEditor Reference")]
        [SerializeField] private Character _character;

        [Header("Settings")]
        [SerializeField] private float _flashDuration = 0.15f;

        // ============================================
        // Private State
        // ============================================

        private bool _isAnimating;
        private Coroutine _flashCoroutine;
        private Coroutine _animationCoroutine;

        // ============================================
        // Properties
        // ============================================

        public bool IsAnimating => _isAnimating;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_character == null)
            {
                _character = GetComponentInChildren<Character>();
            }

            if (_character == null)
            {
                Debug.LogError($"[AnimatedCharacterVisual] No Character component found on {gameObject.name}");
            }
        }

        private void OnEnable()
        {
            SetIdle();
        }

        // ============================================
        // ICharacterVisual Implementation
        // ============================================

        public void PlayAttack(AttackType type = AttackType.Slash)
        {
            if (_character == null) return;

            StopCurrentAnimation();

            switch (type)
            {
                case AttackType.Slash:
                    _character.Slash();
                    break;
                case AttackType.Jab:
                    _character.Jab();
                    break;
                case AttackType.Shoot:
                    _character.Shoot();
                    break;
            }

            _animationCoroutine = StartCoroutine(WaitForAnimation(0.5f));
        }

        public void PlayHit()
        {
            if (_character == null) return;

            _character.Hit();
            FlashColor(Color.red, _flashDuration);
        }

        public void PlayDeath(bool forward = false)
        {
            if (_character == null) return;

            StopCurrentAnimation();
            _character.SetState(forward ? CharacterState.DeathF : CharacterState.DeathB);
            _isAnimating = true; // Death is a final state
        }

        public void SetIdle()
        {
            if (_character == null) return;

            StopCurrentAnimation();
            _character.SetState(CharacterState.Idle);
        }

        public void SetExpression(string expression)
        {
            if (_character == null) return;

            _character.SetExpression(expression);
        }

        public void FlashColor(Color color, float duration)
        {
            if (_character == null) return;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashColorCoroutine(color, duration));
        }

        public void PlaySkill()
        {
            if (_character == null) return;

            StopCurrentAnimation();
            // Use slash with different timing for skill
            _character.Slash();
            _animationCoroutine = StartCoroutine(WaitForAnimation(0.7f));
        }

        public void PlayBlock()
        {
            if (_character == null) return;

            // HeroEditor doesn't have a dedicated block, use Ready state
            _character.SetState(CharacterState.Ready);
        }

        public void SetFacing(bool faceRight)
        {
            // HeroEditor uses negative scale for facing
            Vector3 scale = transform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // ============================================
        // Private Methods
        // ============================================

        private void StopCurrentAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
            _isAnimating = false;
        }

        private IEnumerator WaitForAnimation(float duration)
        {
            _isAnimating = true;
            yield return new WaitForSeconds(duration);
            _isAnimating = false;
            SetIdle();
        }

        private IEnumerator FlashColorCoroutine(Color color, float duration)
        {
            // Use HeroEditor's built-in hit flash if color is red
            if (color == Color.red)
            {
                yield return _character.HitAsRed();
            }
            else
            {
                // For other colors, manually tint sprites
                var renderers = GetComponentsInChildren<SpriteRenderer>();
                var originalColors = new Color[renderers.Length];

                for (int i = 0; i < renderers.Length; i++)
                {
                    originalColors[i] = renderers[i].color;
                    renderers[i].color = color;
                }

                yield return new WaitForSeconds(duration);

                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].color = originalColors[i];
                    }
                }
            }

            _flashCoroutine = null;
        }

        // ============================================
        // Public Utilities
        // ============================================

        /// <summary>
        /// Get the underlying HeroEditor Character component.
        /// </summary>
        public Character GetCharacter() => _character;

        /// <summary>
        /// Initialize with a Character component reference.
        /// </summary>
        public void Initialize(Character character)
        {
            _character = character;
        }
    }
}
