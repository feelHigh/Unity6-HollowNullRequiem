// ============================================
// CombatVFXController.cs
// Spawns VFX for combat events
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Cards;
using HNR.Combat;
using HNR.UI;

namespace HNR.VFX
{
    /// <summary>
    /// Subscribes to combat events and spawns appropriate VFX.
    /// Uses VFXPoolManager for efficient particle pooling.
    /// </summary>
    public class CombatVFXController : MonoBehaviour
    {
        // ============================================
        // Effect IDs
        // ============================================

        [Header("Effect IDs")]
        [SerializeField, Tooltip("Effect for physical attacks")]
        private string _slashEffectId = "vfx_slash";

        [SerializeField, Tooltip("Effect for blocking")]
        private string _shieldEffectId = "vfx_shield";

        [SerializeField, Tooltip("Effect for healing")]
        private string _healEffectId = "vfx_heal";

        [SerializeField, Tooltip("Effect for corruption gain")]
        private string _corruptionEffectId = "vfx_corruption";

        [SerializeField, Tooltip("Effect for Null State trigger")]
        private string _nullBurstEffectId = "vfx_null_burst";

        [Header("Hit Effect Prefix")]
        [SerializeField, Tooltip("Prefix for aspect-based hit effects (e.g., hit_flame)")]
        private string _hitEffectPrefix = "hit_";

        // ============================================
        // Private Fields
        // ============================================

        private VFXPoolManager _vfxPool;
        private SoulAspect _lastPlayedAspect = SoulAspect.Flame;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.TryGet(out _vfxPool);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Subscribe<HealingReceivedEvent>(OnHealing);
            EventBus.Subscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Unsubscribe<HealingReceivedEvent>(OnHealing);
            EventBus.Unsubscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            if (evt.Target == null || evt.Amount <= 0) return;

            // Get hit effect based on last played card's aspect
            string effectId = GetHitEffectId(_lastPlayedAspect);
            var instance = _vfxPool?.Spawn(effectId, evt.Target.Position, Quaternion.identity);

            if (instance != null)
            {
                // Apply aspect color
                instance.SetColor(UIColors.GetAspectColor(_lastPlayedAspect));

                // Scale based on damage (0.5x to 2x based on 20 damage baseline)
                float scale = Mathf.Clamp(evt.Amount / 20f, 0.5f, 2f);
                instance.SetScale(scale);
            }
        }

        private void OnBlockGained(BlockGainedEvent evt)
        {
            if (evt.Target == null || evt.Amount <= 0) return;

            var instance = _vfxPool?.Spawn(_shieldEffectId, evt.Target.Position, Quaternion.identity);
            instance?.SetColor(UIColors.SoulCyan);
        }

        private void OnHealing(HealingReceivedEvent evt)
        {
            if (evt.Target == null || evt.Amount <= 0) return;

            var instance = _vfxPool?.Spawn(_healEffectId, evt.Target.Position, Quaternion.identity);
            instance?.SetColor(UIColors.NatureAspect);
        }

        private void OnCorruptionChanged(CorruptionChangedEvent evt)
        {
            // Only spawn VFX when corruption is gained (not lost)
            if (evt.Requiem == null || evt.Delta <= 0) return;

            var instance = _vfxPool?.SpawnAttached(_corruptionEffectId, evt.Requiem.transform);
            instance?.SetColor(UIColors.HollowViolet);
        }

        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            if (evt.Requiem == null) return;

            var instance = _vfxPool?.Spawn(_nullBurstEffectId, evt.Requiem.Position, Quaternion.identity);
            if (instance != null)
            {
                instance.SetColor(UIColors.CorruptionGlow);
                instance.SetScale(2f);
            }
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            if (evt.Card?.Data == null) return;

            // Track aspect for damage events
            _lastPlayedAspect = evt.Card.Data.SoulAspect;

            // Spawn slash effect for Strike cards with targets
            if (evt.Card.Data.CardType == CardType.Strike && evt.Target != null)
            {
                var instance = _vfxPool?.Spawn(_slashEffectId, evt.Target.Position, Quaternion.identity);
                instance?.SetColor(UIColors.GetAspectColor(_lastPlayedAspect));
            }
        }

        // ============================================
        // Helper Methods
        // ============================================

        private string GetHitEffectId(SoulAspect aspect)
        {
            return aspect switch
            {
                SoulAspect.Flame => $"{_hitEffectPrefix}flame",
                SoulAspect.Shadow => $"{_hitEffectPrefix}shadow",
                SoulAspect.Nature => $"{_hitEffectPrefix}nature",
                SoulAspect.Arcane => $"{_hitEffectPrefix}arcane",
                SoulAspect.Light => $"{_hitEffectPrefix}light",
                _ => $"{_hitEffectPrefix}flame"
            };
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Spawn VFX at position (for manual spawning).
        /// </summary>
        /// <param name="effectId">Effect ID from pool config</param>
        /// <param name="position">World position</param>
        /// <param name="color">Optional color override</param>
        /// <param name="scale">Scale multiplier (default 1)</param>
        public void SpawnEffect(string effectId, Vector3 position, Color? color = null, float scale = 1f)
        {
            var instance = _vfxPool?.Spawn(effectId, position, Quaternion.identity);
            if (instance != null)
            {
                if (color.HasValue)
                    instance.SetColor(color.Value);
                instance.SetScale(scale);
            }
        }

        /// <summary>
        /// Spawn VFX attached to a transform.
        /// </summary>
        /// <param name="effectId">Effect ID from pool config</param>
        /// <param name="target">Transform to attach to</param>
        /// <param name="color">Optional color override</param>
        public void SpawnAttachedEffect(string effectId, Transform target, Color? color = null)
        {
            var instance = _vfxPool?.SpawnAttached(effectId, target);
            if (instance != null && color.HasValue)
            {
                instance.SetColor(color.Value);
            }
        }
    }
}
