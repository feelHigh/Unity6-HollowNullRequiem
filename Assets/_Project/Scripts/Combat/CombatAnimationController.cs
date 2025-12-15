// ============================================
// CombatAnimationController.cs
// Event-driven combat animation orchestrator
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;
using HNR.Characters.Visuals;
using HNR.Cards;

namespace HNR.Combat
{
    /// <summary>
    /// Subscribes to combat events and triggers appropriate animations on character visuals.
    /// Coordinates attack, hit, death, and skill animations based on game events.
    /// </summary>
    public class CombatAnimationController : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        private static CombatAnimationController _instance;
        public static CombatAnimationController Instance => _instance;

        // ============================================
        // Configuration
        // ============================================

        [Header("Settings")]
        [SerializeField, Tooltip("Delay between attack animation and hit animation")]
        private float _attackToHitDelay = 0.15f;

        [SerializeField, Tooltip("Flash color for damage")]
        private Color _damageFlashColor = Color.red;

        [SerializeField, Tooltip("Flash duration")]
        private float _flashDuration = 0.15f;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // ============================================
        // Event Subscriptions
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Subscribe<RequiemArtUsedEvent>(OnRequiemArtUsed);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Unsubscribe<RequiemArtUsedEvent>(OnRequiemArtUsed);
        }

        // ============================================
        // Event Handlers
        // ============================================

        /// <summary>
        /// Handle damage dealt event - trigger attack and hit animations.
        /// </summary>
        private void OnDamageDealt(DamageDealtEvent evt)
        {
            // Get attacker's visual and play attack animation
            ICharacterVisual attackerVisual = GetVisualFromSource(evt.Source);
            AttackType attackType = GetAttackTypeFromSource(evt.Source);

            if (attackerVisual != null)
            {
                attackerVisual.PlayAttack(attackType);
            }

            // Get target's visual and play hit animation (with slight delay)
            ICharacterVisual targetVisual = GetVisualFromTarget(evt.Target);
            if (targetVisual != null)
            {
                StartCoroutine(PlayHitDelayed(targetVisual, _attackToHitDelay));
            }
        }

        /// <summary>
        /// Handle card played event - trigger attack animation for attack cards.
        /// </summary>
        private void OnCardPlayed(CardPlayedEvent evt)
        {
            // Only play attack animation for attack cards (damage events handle the rest)
            if (evt.Card?.Data?.Type != CardType.Attack)
            {
                // For skill cards, play skill animation on the caster
                if (evt.Card?.Data?.Type == CardType.Skill && evt.Requiem?.Visual != null)
                {
                    evt.Requiem.Visual.PlaySkill();
                }
            }
        }

        /// <summary>
        /// Handle combat ended - reset all characters to idle.
        /// </summary>
        private void OnCombatEnded(CombatEndedEvent evt)
        {
            // Reset all team members to idle
            if (ServiceLocator.TryGet<TurnManager>(out var tm) && tm.Context?.Team != null)
            {
                foreach (var requiem in tm.Context.Team)
                {
                    requiem?.Visual?.SetIdle();
                }
            }
        }

        /// <summary>
        /// Handle enemy defeated - play death animation.
        /// </summary>
        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            evt.Enemy?.Visual?.PlayDeath(forward: false);
        }

        /// <summary>
        /// Handle Null State entered - could trigger special animation/expression.
        /// </summary>
        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            // Set an intense expression when entering Null State
            evt.Requiem?.Visual?.SetExpression("Interrupted");
            evt.Requiem?.Visual?.FlashColor(Color.magenta, 0.5f);
        }

        /// <summary>
        /// Handle Requiem Art used - play skill animation.
        /// </summary>
        private void OnRequiemArtUsed(RequiemArtUsedEvent evt)
        {
            evt.Requiem?.Visual?.PlaySkill();
        }

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Get visual component from a damage source object.
        /// </summary>
        private ICharacterVisual GetVisualFromSource(object source)
        {
            return source switch
            {
                RequiemInstance requiem => requiem.Visual,
                EnemyInstance enemy => enemy.Visual,
                _ => null
            };
        }

        /// <summary>
        /// Get visual component from an ICombatTarget.
        /// </summary>
        private ICharacterVisual GetVisualFromTarget(ICombatTarget target)
        {
            return target switch
            {
                RequiemInstance requiem => requiem.Visual,
                EnemyInstance enemy => enemy.Visual,
                _ => null
            };
        }

        /// <summary>
        /// Get preferred attack type from source.
        /// </summary>
        private AttackType GetAttackTypeFromSource(object source)
        {
            return source switch
            {
                RequiemInstance requiem => requiem.Data?.PreferredAttackType ?? AttackType.Slash,
                EnemyInstance enemy => enemy.Data?.PreferredAttackType ?? AttackType.Slash,
                _ => AttackType.Slash
            };
        }

        /// <summary>
        /// Play hit animation with delay.
        /// </summary>
        private System.Collections.IEnumerator PlayHitDelayed(ICharacterVisual visual, float delay)
        {
            yield return new WaitForSeconds(delay);
            visual?.PlayHit();
        }

        // ============================================
        // Public Animation Triggers
        // ============================================

        /// <summary>
        /// Manually trigger an attack animation on a Requiem.
        /// </summary>
        public void TriggerAttack(RequiemInstance requiem)
        {
            var attackType = requiem?.Data?.PreferredAttackType ?? AttackType.Slash;
            requiem?.Visual?.PlayAttack(attackType);
        }

        /// <summary>
        /// Manually trigger an attack animation on an Enemy.
        /// </summary>
        public void TriggerAttack(EnemyInstance enemy)
        {
            var attackType = enemy?.Data?.PreferredAttackType ?? AttackType.Slash;
            enemy?.Visual?.PlayAttack(attackType);
        }

        /// <summary>
        /// Manually trigger a hit animation on a target.
        /// </summary>
        public void TriggerHit(ICombatTarget target)
        {
            GetVisualFromTarget(target)?.PlayHit();
        }

        /// <summary>
        /// Manually trigger a death animation on a target.
        /// </summary>
        public void TriggerDeath(ICombatTarget target, bool forward = false)
        {
            GetVisualFromTarget(target)?.PlayDeath(forward);
        }

        /// <summary>
        /// Set all characters to idle state.
        /// </summary>
        public void ResetAllToIdle()
        {
            if (ServiceLocator.TryGet<TurnManager>(out var tm) && tm.Context != null)
            {
                // Reset team
                if (tm.Context.Team != null)
                {
                    foreach (var requiem in tm.Context.Team)
                    {
                        requiem?.Visual?.SetIdle();
                    }
                }

                // Reset enemies
                if (tm.Context.Enemies != null)
                {
                    foreach (var enemy in tm.Context.Enemies)
                    {
                        if (enemy != null && !enemy.IsDead)
                        {
                            enemy.Visual?.SetIdle();
                        }
                    }
                }
            }
        }
    }
}
