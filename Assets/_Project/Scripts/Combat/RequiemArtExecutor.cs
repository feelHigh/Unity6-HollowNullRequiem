// ============================================
// RequiemArtExecutor.cs
// Executes Requiem Art effects using card effect system
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Cards;
using HNR.Characters;
using HNR.VFX;

namespace HNR.Combat
{
    /// <summary>
    /// Executes Requiem Art effects by reusing the card effect system.
    /// Handles targeting, VFX, audio, and effect execution for ultimate abilities.
    /// </summary>
    public class RequiemArtExecutor : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        private static RequiemArtExecutor _instance;
        public static RequiemArtExecutor Instance => _instance;

        // ============================================
        // References
        // ============================================

        private CardExecutor _cardExecutor;
        private TurnManager _turnManager;

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
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            _cardExecutor = ServiceLocator.TryGet<CardExecutor>(out var ce) ? ce : null;
            _turnManager = ServiceLocator.TryGet<TurnManager>(out var tm) ? tm : null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ServiceLocator.Unregister<RequiemArtExecutor>();
                _instance = null;
            }
        }

        // ============================================
        // Art Execution
        // ============================================

        /// <summary>
        /// Executes a Requiem Art's effects.
        /// </summary>
        /// <param name="requiem">The Requiem activating the Art.</param>
        /// <param name="art">The Art data to execute.</param>
        /// <param name="target">Optional specific target (for single-target Arts).</param>
        /// <returns>True if execution succeeded.</returns>
        public bool ExecuteArt(RequiemInstance requiem, RequiemArtDataSO art, ICombatTarget target = null)
        {
            if (requiem == null || art == null)
            {
                Debug.LogWarning("[RequiemArtExecutor] Invalid requiem or art data");
                return false;
            }

            if (_cardExecutor == null)
            {
                _cardExecutor = ServiceLocator.TryGet<CardExecutor>(out var ce) ? ce : null;
                if (_cardExecutor == null)
                {
                    Debug.LogError("[RequiemArtExecutor] CardExecutor not found");
                    return false;
                }
            }

            Debug.Log($"[RequiemArtExecutor] Executing {art.ArtName} for {requiem.Name}");

            // Build effect context
            var context = BuildEffectContext(requiem, art, target);

            // Play VFX
            PlayArtVFX(art, requiem, context.Target);

            // Play Audio
            PlayArtAudio(art);

            // Execute each effect
            foreach (var effectData in art.Effects)
            {
                ExecuteEffect(effectData, context);
            }

            Debug.Log($"[RequiemArtExecutor] {art.ArtName} executed successfully");
            return true;
        }

        // ============================================
        // Context Building
        // ============================================

        private EffectContext BuildEffectContext(RequiemInstance requiem, RequiemArtDataSO art, ICombatTarget explicitTarget)
        {
            var context = new EffectContext
            {
                Card = null, // Not a card
                Source = requiem.Data,
                TurnManager = _turnManager,
                CombatContext = _turnManager?.Context,
                DeckManager = _turnManager?.Context?.DeckManager
            };

            // Resolve targets based on art's target type
            ResolveTargets(art.TargetType, explicitTarget, context);

            // Apply Null State damage bonus
            if (requiem.InNullState)
            {
                context.DamageMultiplier = 1.5f; // +50% damage in Null State
            }

            return context;
        }

        private void ResolveTargets(TargetType targetType, ICombatTarget explicitTarget, EffectContext context)
        {
            var combatContext = context.CombatContext;
            if (combatContext == null) return;

            switch (targetType)
            {
                case TargetType.SingleEnemy:
                    if (explicitTarget != null)
                    {
                        context.Target = explicitTarget;
                        context.AllTargets.Add(explicitTarget);
                    }
                    else if (combatContext.Enemies.Count > 0)
                    {
                        // Default to first alive enemy
                        var firstEnemy = combatContext.Enemies.Find(e => !e.IsDead);
                        if (firstEnemy != null)
                        {
                            context.Target = firstEnemy;
                            context.AllTargets.Add(firstEnemy);
                        }
                    }
                    break;

                case TargetType.AllEnemies:
                    foreach (var enemy in combatContext.Enemies)
                    {
                        if (!enemy.IsDead)
                        {
                            context.AllTargets.Add(enemy);
                        }
                    }
                    if (context.AllTargets.Count > 0)
                    {
                        context.Target = context.AllTargets[0];
                    }
                    break;

                case TargetType.SingleAlly:
                    if (explicitTarget != null)
                    {
                        context.Target = explicitTarget;
                        context.AllTargets.Add(explicitTarget);
                    }
                    else if (combatContext.Team.Count > 0)
                    {
                        context.Target = combatContext.Team[0];
                        context.AllTargets.Add(combatContext.Team[0]);
                    }
                    break;

                case TargetType.AllAllies:
                    foreach (var ally in combatContext.Team)
                    {
                        context.AllTargets.Add(ally);
                    }
                    if (context.AllTargets.Count > 0)
                    {
                        context.Target = context.AllTargets[0];
                    }
                    break;

                case TargetType.Self:
                    // Self-targeting not applicable for team-based effects
                    break;

                case TargetType.Random:
                    if (combatContext.Enemies.Count > 0)
                    {
                        var aliveEnemies = combatContext.Enemies.FindAll(e => !e.IsDead);
                        if (aliveEnemies.Count > 0)
                        {
                            var randomEnemy = aliveEnemies[Random.Range(0, aliveEnemies.Count)];
                            context.Target = randomEnemy;
                            context.AllTargets.Add(randomEnemy);
                        }
                    }
                    break;

                case TargetType.None:
                default:
                    // No targeting needed (self-effects, team-wide buffs)
                    break;
            }
        }

        // ============================================
        // Effect Execution
        // ============================================

        private void ExecuteEffect(CardEffectData effectData, EffectContext context)
        {
            if (_cardExecutor == null) return;

            // Use CardExecutor's effect handler system
            _cardExecutor.ExecuteSingleEffect(effectData, context);
        }

        // ============================================
        // VFX & Audio
        // ============================================

        private void PlayArtVFX(RequiemArtDataSO art, RequiemInstance requiem, ICombatTarget target)
        {
            if (art.VFXPrefab == null) return;

            // Determine spawn position
            Vector3 spawnPos = Vector3.zero;
            if (target != null && target is MonoBehaviour mb)
            {
                spawnPos = mb.transform.position;
            }
            else if (requiem != null)
            {
                spawnPos = requiem.transform.position;
            }

            // Try to use VFX pool
            if (ServiceLocator.TryGet<VFXPoolManager>(out var vfxPool))
            {
                // For arts, we instantiate directly since they may be unique
                var vfx = Instantiate(art.VFXPrefab, spawnPos, Quaternion.identity);
                Destroy(vfx, art.EffectDuration + 1f);
            }
            else
            {
                var vfx = Instantiate(art.VFXPrefab, spawnPos, Quaternion.identity);
                Destroy(vfx, art.EffectDuration + 1f);
            }

            // Screen flash
            if (art.FlashColor != Color.clear)
            {
                // Publish screen flash event if needed
                // Could integrate with CombatFeedbackIntegrator
            }
        }

        private void PlayArtAudio(RequiemArtDataSO art)
        {
            // Play activation sound using Unity's built-in method for AudioClip
            if (art.ActivationSound != null)
            {
                AudioSource.PlayClipAtPoint(art.ActivationSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            }

            // Play voice line
            if (art.VoiceLine != null)
            {
                AudioSource.PlayClipAtPoint(art.VoiceLine, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            }
        }
    }
}
