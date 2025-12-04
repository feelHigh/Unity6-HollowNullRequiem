// ============================================
// RelicManager.cs
// Manages owned relics and their triggered effects
// ============================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;

namespace HNR.Progression
{
    /// <summary>
    /// Manages relic collection and effect triggering.
    /// Subscribes to game events to trigger relics automatically.
    /// </summary>
    public class RelicManager : MonoBehaviour, IRelicManager
    {
        // ============================================
        // Private Fields
        // ============================================

        private List<RelicDataSO> _ownedRelics = new();
        private Dictionary<string, RelicDataSO> _relicCache = new();
        private bool _isSubscribed;

        // ============================================
        // Properties
        // ============================================

        /// <summary>List of all owned relics.</summary>
        public IReadOnlyList<RelicDataSO> OwnedRelics => _ownedRelics.AsReadOnly();

        /// <summary>Number of owned relics.</summary>
        public int RelicCount => _ownedRelics.Count;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register<IRelicManager>(this);
            DontDestroyOnLoad(gameObject);
            CacheAllRelics();
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
            if (ServiceLocator.Has<IRelicManager>())
            {
                ServiceLocator.Unregister<IRelicManager>();
            }
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Cache all relic assets from Resources for quick lookup.
        /// </summary>
        private void CacheAllRelics()
        {
            var allRelics = Resources.LoadAll<RelicDataSO>("Data/Relics");

            foreach (var relic in allRelics)
            {
                if (relic != null && !string.IsNullOrEmpty(relic.RelicId))
                {
                    _relicCache[relic.RelicId] = relic;
                }
            }

            Debug.Log($"[RelicManager] Cached {_relicCache.Count} relics");
        }

        // ============================================
        // Event Subscriptions
        // ============================================

        private void SubscribeToEvents()
        {
            if (_isSubscribed) return;

            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);

            _isSubscribed = true;
            Debug.Log("[RelicManager] Subscribed to events");
        }

        private void UnsubscribeFromEvents()
        {
            if (!_isSubscribed) return;

            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);

            _isSubscribed = false;
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            TriggerRelics(RelicTrigger.OnCombatStart, evt);
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            TriggerRelics(RelicTrigger.OnCombatEnd, evt);
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            if (evt.IsPlayerTurn)
            {
                TriggerRelics(RelicTrigger.OnTurnStart, evt);
            }
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            TriggerRelics(RelicTrigger.OnCardPlay, evt);
        }

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            TriggerRelics(RelicTrigger.OnDamageDealt, evt);

            // Check for kill trigger
            // TODO: Check if target died from this damage
        }

        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            TriggerRelics(RelicTrigger.OnNullStateEntered, evt);
        }

        // ============================================
        // Public Methods - Relic Management
        // ============================================

        /// <summary>
        /// Add a relic to the collection.
        /// </summary>
        public void AddRelic(RelicDataSO relic)
        {
            if (relic == null)
            {
                Debug.LogWarning("[RelicManager] Cannot add null relic");
                return;
            }

            if (HasRelic(relic.RelicId))
            {
                Debug.Log($"[RelicManager] Already owns relic: {relic.RelicName}");
                return;
            }

            _ownedRelics.Add(relic);
            EventBus.Publish(new RelicAcquiredEvent(relic));
            Debug.Log($"[RelicManager] Acquired: {relic.RelicName}");

            // Apply passive effects immediately
            if (relic.Trigger == RelicTrigger.Passive)
            {
                ApplyRelicEffect(relic, null);
            }
        }

        /// <summary>
        /// Check if a relic is owned by ID.
        /// </summary>
        public bool HasRelic(string relicId)
        {
            return _ownedRelics.Exists(r => r.RelicId == relicId);
        }

        /// <summary>
        /// Get an owned relic by ID.
        /// </summary>
        public RelicDataSO GetRelic(string relicId)
        {
            return _ownedRelics.Find(r => r.RelicId == relicId);
        }

        /// <summary>
        /// Trigger all relics matching the specified trigger type.
        /// </summary>
        public void TriggerRelics(RelicTrigger trigger, object context = null)
        {
            foreach (var relic in _ownedRelics)
            {
                if (relic.Trigger == trigger)
                {
                    ApplyRelicEffect(relic, context);
                }
            }
        }

        /// <summary>
        /// Clear all owned relics.
        /// </summary>
        public void ClearRelics()
        {
            _ownedRelics.Clear();
            Debug.Log("[RelicManager] Cleared all relics");
        }

        // ============================================
        // Public Methods - Save/Load
        // ============================================

        /// <summary>
        /// Load relics from saved IDs.
        /// </summary>
        public void LoadRelics(List<string> relicIds)
        {
            _ownedRelics.Clear();

            if (relicIds == null) return;

            foreach (var id in relicIds)
            {
                if (_relicCache.TryGetValue(id, out var relic))
                {
                    _ownedRelics.Add(relic);
                    Debug.Log($"[RelicManager] Loaded relic: {relic.RelicName}");

                    // Reapply passive effects
                    if (relic.Trigger == RelicTrigger.Passive)
                    {
                        ApplyRelicEffect(relic, null);
                    }
                }
                else
                {
                    Debug.LogWarning($"[RelicManager] Relic not found in cache: {id}");
                }
            }

            Debug.Log($"[RelicManager] Loaded {_ownedRelics.Count} relics");
        }

        /// <summary>
        /// Get list of owned relic IDs for saving.
        /// </summary>
        public List<string> GetRelicIds()
        {
            return _ownedRelics.Select(r => r.RelicId).ToList();
        }

        // ============================================
        // Private Methods - Effect Application
        // ============================================

        /// <summary>
        /// Apply a relic's effect.
        /// </summary>
        private void ApplyRelicEffect(RelicDataSO relic, object context)
        {
            Debug.Log($"[RelicManager] Triggering: {relic.RelicName} ({relic.EffectType}: {relic.EffectValue})");

            switch (relic.EffectType)
            {
                case RelicEffectType.ModifyMaxHP:
                    // TODO: Get IRunManager and increase max HP
                    Debug.Log($"[RelicManager] ModifyMaxHP by {relic.EffectValue}");
                    break;

                case RelicEffectType.ModifyDamage:
                    // Damage modification is checked during damage calculation
                    Debug.Log($"[RelicManager] ModifyDamage by {relic.EffectValue}");
                    break;

                case RelicEffectType.ModifyBlock:
                    if (ServiceLocator.TryGet<HNR.Combat.TurnManager>(out var turnManager))
                    {
                        turnManager.AddTeamBlock(relic.EffectValue);
                    }
                    break;

                case RelicEffectType.GainSoulEssence:
                    // TODO: Add SoulEssence method to TurnManager/CombatContext
                    Debug.Log($"[RelicManager] GainSoulEssence: +{relic.EffectValue} SE");
                    break;

                case RelicEffectType.ReduceCorruption:
                    // TODO: Get CorruptionManager and reduce corruption
                    Debug.Log($"[RelicManager] ReduceCorruption by {relic.EffectValue}");
                    break;

                case RelicEffectType.DrawCard:
                    // TODO: Add DrawCards method to TurnManager
                    Debug.Log($"[RelicManager] DrawCard: +{relic.EffectValue} cards");
                    break;

                case RelicEffectType.Healing:
                    if (ServiceLocator.TryGet<HNR.Combat.TurnManager>(out var healTm))
                    {
                        healTm.HealTeam(relic.EffectValue);
                    }
                    break;

                case RelicEffectType.GainVoidShards:
                    if (ServiceLocator.TryGet<IShopManager>(out var shopManager))
                    {
                        shopManager.AddVoidShards(relic.EffectValue);
                    }
                    break;

                case RelicEffectType.GainAP:
                    // TODO: Add AddAP method to TurnManager/CombatContext
                    Debug.Log($"[RelicManager] GainAP: +{relic.EffectValue} AP");
                    break;
            }

            // Publish triggered event
            EventBus.Publish(new RelicTriggeredEvent(relic));
        }

        /// <summary>
        /// Get total damage modifier from all owned relics.
        /// Called during damage calculation.
        /// </summary>
        public int GetDamageModifier()
        {
            int modifier = 0;
            foreach (var relic in _ownedRelics)
            {
                if (relic.EffectType == RelicEffectType.ModifyDamage)
                {
                    modifier += relic.EffectValue;
                }
            }
            return modifier;
        }

        /// <summary>
        /// Get total block modifier from all owned relics.
        /// Called during block calculation.
        /// </summary>
        public int GetBlockModifier()
        {
            int modifier = 0;
            foreach (var relic in _ownedRelics)
            {
                if (relic.EffectType == RelicEffectType.ModifyBlock)
                {
                    modifier += relic.EffectValue;
                }
            }
            return modifier;
        }
    }
}
