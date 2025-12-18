// ============================================
// EchoEventManager.cs
// Manages Echo (narrative) event execution
// ============================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Characters;
using HNR.Cards;
using HNR.Combat;
using HNR.Progression;

namespace HNR.Map
{
    /// <summary>
    /// Manages Echo events - narrative encounters with player choices.
    /// Registers with ServiceLocator for global access.
    /// </summary>
    public class EchoEventManager : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Event Pool")]
        [SerializeField, Tooltip("Available Echo events for random selection")]
        private EchoEventDataSO[] _availableEvents;

        // ============================================
        // Runtime State
        // ============================================

        private EchoEventDataSO _currentEvent;
        private EchoChoice _selectedChoice;

        // Caches for card/relic lookups
        private Dictionary<string, CardDataSO> _cardCache = new();
        private Dictionary<string, RelicDataSO> _relicCache = new();

        // ============================================
        // Properties
        // ============================================

        /// <summary>Currently active event.</summary>
        public EchoEventDataSO CurrentEvent => _currentEvent;

        /// <summary>Whether an event is currently active.</summary>
        public bool IsEventActive => _currentEvent != null;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
            CacheResources();
        }

        /// <summary>
        /// Cache card and relic assets for ID-based lookups.
        /// </summary>
        private void CacheResources()
        {
            // Cache all cards
            var allCards = Resources.LoadAll<CardDataSO>("Data/Cards");
            foreach (var card in allCards)
            {
                if (card != null && !string.IsNullOrEmpty(card.CardId))
                {
                    _cardCache[card.CardId] = card;
                }
            }

            // Cache all relics
            var allRelics = Resources.LoadAll<RelicDataSO>("Data/Relics");
            foreach (var relic in allRelics)
            {
                if (relic != null && !string.IsNullOrEmpty(relic.RelicId))
                {
                    _relicCache[relic.RelicId] = relic;
                }
            }

            Debug.Log($"[EchoEventManager] Cached {_cardCache.Count} cards, {_relicCache.Count} relics");
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<EchoEventManager>();
        }

        // ============================================
        // Event Selection
        // ============================================

        /// <summary>
        /// Gets a random event from the available pool.
        /// </summary>
        public EchoEventDataSO GetRandomEvent()
        {
            if (_availableEvents == null || _availableEvents.Length == 0)
            {
                Debug.LogWarning("[EchoEventManager] No available events");
                return null;
            }

            return _availableEvents[Random.Range(0, _availableEvents.Length)];
        }

        /// <summary>
        /// Gets a random event valid for the specified zone.
        /// </summary>
        public EchoEventDataSO GetRandomEventForZone(int zone)
        {
            if (_availableEvents == null || _availableEvents.Length == 0)
                return null;

            // Filter by zone
            var validEvents = System.Array.FindAll(_availableEvents, e => e.CanAppearInZone(zone));

            if (validEvents.Length == 0)
                return GetRandomEvent(); // Fallback to any event

            return validEvents[Random.Range(0, validEvents.Length)];
        }

        // ============================================
        // Event Flow
        // ============================================

        /// <summary>
        /// Starts an Echo event.
        /// </summary>
        /// <param name="eventData">Event to start, or null for random.</param>
        public void StartEvent(EchoEventDataSO eventData)
        {
            if (eventData == null)
            {
                eventData = GetRandomEvent();
                if (eventData == null)
                {
                    Debug.LogWarning("[EchoEventManager] No event to start");
                    CompleteEvent();
                    return;
                }
            }

            _currentEvent = eventData;
            _selectedChoice = null;

            EventBus.Publish(new EchoEventStartedEvent(eventData));
            Debug.Log($"[EchoEventManager] Started event: {eventData.EventTitle}");
        }

        /// <summary>
        /// Selects a choice and executes its outcomes.
        /// </summary>
        /// <param name="choiceIndex">Index of the choice to select.</param>
        public void SelectChoice(int choiceIndex)
        {
            if (_currentEvent == null)
            {
                Debug.LogWarning("[EchoEventManager] No active event");
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= _currentEvent.Choices.Count)
            {
                Debug.LogWarning($"[EchoEventManager] Invalid choice index: {choiceIndex}");
                return;
            }

            _selectedChoice = _currentEvent.Choices[choiceIndex];

            // Execute all outcomes
            ExecuteOutcomes(_selectedChoice);

            // Publish event
            EventBus.Publish(new EchoChoiceSelectedEvent(_selectedChoice));
            Debug.Log($"[EchoEventManager] Selected: {_selectedChoice.ChoiceText}");
        }

        /// <summary>
        /// Completes the current event and returns to map.
        /// </summary>
        public void CompleteEvent()
        {
            var completedEvent = _currentEvent;
            _currentEvent = null;
            _selectedChoice = null;

            if (completedEvent != null)
            {
                EventBus.Publish(new EchoEventCompletedEvent(completedEvent));
                Debug.Log($"[EchoEventManager] Completed event: {completedEvent.EventTitle}");
            }

            // Mark map node as complete
            if (ServiceLocator.TryGet<MapManager>(out var mapManager))
            {
                mapManager.CompleteCurrentNode();
            }
        }

        // ============================================
        // Outcome Execution
        // ============================================

        private void ExecuteOutcomes(EchoChoice choice)
        {
            foreach (var outcome in choice.Outcomes)
            {
                ExecuteOutcome(outcome);
            }
        }

        private void ExecuteOutcome(EchoOutcome outcome)
        {
            switch (outcome.Type)
            {
                case EchoOutcomeType.None:
                    // No effect
                    break;

                case EchoOutcomeType.GainGold:
                    ApplyGoldChange(outcome.Value);
                    break;

                case EchoOutcomeType.LoseGold:
                    ApplyGoldChange(-outcome.Value);
                    break;

                case EchoOutcomeType.GainHP:
                    ApplyHPChange(outcome.Value);
                    break;

                case EchoOutcomeType.LoseHP:
                    ApplyHPChange(-outcome.Value);
                    break;

                case EchoOutcomeType.GainMaxHP:
                    ApplyMaxHPChange(outcome.Value);
                    break;

                case EchoOutcomeType.LoseMaxHP:
                    ApplyMaxHPChange(-outcome.Value);
                    break;

                case EchoOutcomeType.GainCorruption:
                    ApplyCorruptionChange(outcome.Value);
                    break;

                case EchoOutcomeType.LoseCorruption:
                    ApplyCorruptionChange(-outcome.Value);
                    break;

                case EchoOutcomeType.GainCard:
                    AddCard(outcome.CardId);
                    break;

                case EchoOutcomeType.RemoveCard:
                    RemoveCard(outcome.CardId);
                    break;

                case EchoOutcomeType.UpgradeCard:
                    UpgradeRandomCard();
                    break;

                case EchoOutcomeType.GainRelic:
                    AddRelic(outcome.RelicId);
                    break;

                case EchoOutcomeType.GainRandomCard:
                    AddRandomCard();
                    break;

                case EchoOutcomeType.GainRandomRelic:
                    AddRandomRelic();
                    break;

                case EchoOutcomeType.StartCombat:
                    StartCombatEncounter();
                    break;

                default:
                    Debug.LogWarning($"[EchoEventManager] Unhandled outcome type: {outcome.Type}");
                    break;
            }
        }

        // ============================================
        // Outcome Implementations
        // ============================================

        private void ApplyGoldChange(int amount)
        {
            // VoidShards are the currency in HNR (equivalent to gold)
            if (ServiceLocator.TryGet<IShopManager>(out var shopManager))
            {
                if (amount > 0)
                {
                    shopManager.AddVoidShards(amount);
                    Debug.Log($"[EchoEvent] Gained {amount} Void Shards");
                }
                else if (amount < 0)
                {
                    // Spend shards (doesn't fail if insufficient - just takes what's available)
                    int toSpend = Mathf.Min(-amount, shopManager.VoidShards);
                    if (toSpend > 0)
                    {
                        shopManager.SpendVoidShards(toSpend);
                    }
                    Debug.Log($"[EchoEvent] Lost {toSpend} Void Shards");
                }
            }
            else
            {
                Debug.LogWarning($"[EchoEvent] ShopManager not available - cannot apply gold change: {amount}");
            }
        }

        private void ApplyHPChange(int amount)
        {
            // Use RunManager for persistent HP changes (outside combat)
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                if (amount > 0)
                {
                    runManager.HealTeam(amount);
                    Debug.Log($"[EchoEvent] Healed {amount} HP");
                }
                else if (amount < 0)
                {
                    runManager.DamageTeam(-amount);
                    Debug.Log($"[EchoEvent] Damaged {-amount} HP");
                }
            }
            else
            {
                Debug.LogWarning($"[EchoEvent] RunManager not available - cannot apply HP change: {amount}");
            }
        }

        private void ApplyMaxHPChange(int amount)
        {
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                if (amount > 0)
                {
                    runManager.IncreaseMaxHP(amount);
                    Debug.Log($"[EchoEvent] Max HP increased by {amount}");
                }
                else if (amount < 0)
                {
                    // For negative max HP, we need to reduce it
                    // RunManager doesn't have DecreaseMaxHP, so we'll publish an event
                    // that can be handled by whatever system manages this
                    EventBus.Publish(new MaxHPChangedEvent(amount));
                    Debug.Log($"[EchoEvent] Max HP decreased by {-amount}");
                }
            }
            else
            {
                Debug.LogWarning($"[EchoEvent] RunManager not available - cannot apply max HP change: {amount}");
            }
        }

        private void ApplyCorruptionChange(int amount)
        {
            if (ServiceLocator.TryGet<CorruptionManager>(out var corruptionManager))
            {
                if (amount > 0)
                {
                    corruptionManager.AddCorruptionToTeam(amount);
                    Debug.Log($"[EchoEvent] Team gained {amount} corruption");
                }
                else if (amount < 0)
                {
                    corruptionManager.RemoveCorruptionFromTeam(-amount);
                    Debug.Log($"[EchoEvent] Team purified {-amount} corruption");
                }
            }
            else
            {
                Debug.LogWarning($"[EchoEvent] CorruptionManager not available - cannot apply corruption change: {amount}");
            }
        }

        private void AddCard(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogWarning("[EchoEvent] Cannot add card - cardId is null or empty");
                return;
            }

            if (_cardCache.TryGetValue(cardId, out var card))
            {
                if (ServiceLocator.TryGet<IRunManager>(out var runManager))
                {
                    runManager.AddCardToDeck(card);
                    Debug.Log($"[EchoEvent] Added card to deck: {card.CardName}");
                }
                else
                {
                    Debug.LogWarning($"[EchoEvent] RunManager not available - cannot add card: {cardId}");
                }
            }
            else
            {
                Debug.LogWarning($"[EchoEvent] Card not found in cache: {cardId}");
            }
        }

        private void RemoveCard(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogWarning("[EchoEvent] Cannot remove card - cardId is null or empty");
                return;
            }

            if (_cardCache.TryGetValue(cardId, out var card))
            {
                if (ServiceLocator.TryGet<IRunManager>(out var runManager))
                {
                    runManager.RemoveCardFromDeck(card);
                    Debug.Log($"[EchoEvent] Removed card from deck: {card.CardName}");
                }
                else
                {
                    Debug.LogWarning($"[EchoEvent] RunManager not available - cannot remove card: {cardId}");
                }
            }
            else
            {
                Debug.LogWarning($"[EchoEvent] Card not found in cache: {cardId}");
            }
        }

        private void UpgradeRandomCard()
        {
            if (!ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                Debug.LogWarning("[EchoEvent] RunManager not available - cannot upgrade card");
                return;
            }

            var deck = runManager.Deck;
            if (deck == null || deck.Count == 0)
            {
                Debug.Log("[EchoEvent] No cards in deck to upgrade");
                return;
            }

            // Find cards that aren't already upgraded
            var upgradableCards = deck.Where(c => c != null && !runManager.IsCardUpgraded(c.CardId)).ToList();

            if (upgradableCards.Count == 0)
            {
                Debug.Log("[EchoEvent] All cards already upgraded");
                return;
            }

            // Pick a random card to upgrade
            var cardToUpgrade = upgradableCards[Random.Range(0, upgradableCards.Count)];
            runManager.UpgradeCard(cardToUpgrade);
            Debug.Log($"[EchoEvent] Upgraded card: {cardToUpgrade.CardName}");
        }

        private void AddRelic(string relicId)
        {
            if (string.IsNullOrEmpty(relicId))
            {
                Debug.LogWarning("[EchoEvent] Cannot add relic - relicId is null or empty");
                return;
            }

            if (_relicCache.TryGetValue(relicId, out var relic))
            {
                if (ServiceLocator.TryGet<IRelicManager>(out var relicManager))
                {
                    relicManager.AddRelic(relic);
                    Debug.Log($"[EchoEvent] Added relic: {relic.RelicName}");
                }
                else
                {
                    Debug.LogWarning($"[EchoEvent] RelicManager not available - cannot add relic: {relicId}");
                }
            }
            else
            {
                Debug.LogWarning($"[EchoEvent] Relic not found in cache: {relicId}");
            }
        }

        private void AddRandomCard()
        {
            if (_cardCache.Count == 0)
            {
                Debug.LogWarning("[EchoEvent] No cards in cache for random selection");
                return;
            }

            if (!ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                Debug.LogWarning("[EchoEvent] RunManager not available - cannot add random card");
                return;
            }

            // Get cards not already in deck (or allow duplicates for deckbuilders)
            var availableCards = _cardCache.Values.ToList();
            if (availableCards.Count == 0)
            {
                Debug.Log("[EchoEvent] No cards available for random selection");
                return;
            }

            var randomCard = availableCards[Random.Range(0, availableCards.Count)];
            runManager.AddCardToDeck(randomCard);
            Debug.Log($"[EchoEvent] Added random card: {randomCard.CardName}");
        }

        private void AddRandomRelic()
        {
            if (_relicCache.Count == 0)
            {
                Debug.LogWarning("[EchoEvent] No relics in cache for random selection");
                return;
            }

            if (!ServiceLocator.TryGet<IRelicManager>(out var relicManager))
            {
                Debug.LogWarning("[EchoEvent] RelicManager not available - cannot add random relic");
                return;
            }

            // Get relics not already owned
            var availableRelics = _relicCache.Values
                .Where(r => !relicManager.HasRelic(r.RelicId))
                .ToList();

            if (availableRelics.Count == 0)
            {
                Debug.Log("[EchoEvent] All relics already owned");
                return;
            }

            var randomRelic = availableRelics[Random.Range(0, availableRelics.Count)];
            relicManager.AddRelic(randomRelic);
            Debug.Log($"[EchoEvent] Added random relic: {randomRelic.RelicName}");
        }

        private void StartCombatEncounter()
        {
            // Publish event for GameManager to handle combat transition
            // The current event may have specific encounter data attached
            EventBus.Publish(new EchoCombatRequestedEvent(_currentEvent));
            Debug.Log("[EchoEvent] Requested combat encounter from Echo event");
        }
    }

    // ============================================
    // Additional Events for Echo Outcomes
    // ============================================

    /// <summary>
    /// Published when an Echo event requests a max HP change.
    /// Handles negative max HP changes that RunManager doesn't directly support.
    /// </summary>
    public class MaxHPChangedEvent : GameEvent
    {
        public int Delta { get; }
        public MaxHPChangedEvent(int delta) => Delta = delta;
    }

    /// <summary>
    /// Published when an Echo event triggers a combat encounter.
    /// </summary>
    public class EchoCombatRequestedEvent : GameEvent
    {
        public EchoEventDataSO SourceEvent { get; }
        public EchoCombatRequestedEvent(EchoEventDataSO sourceEvent) => SourceEvent = sourceEvent;
    }

    // ============================================
    // Echo Events
    // ============================================

    /// <summary>
    /// Published when an Echo event starts.
    /// </summary>
    public class EchoEventStartedEvent : GameEvent
    {
        public EchoEventDataSO Event { get; }
        public EchoEventStartedEvent(EchoEventDataSO evt) => Event = evt;
    }

    /// <summary>
    /// Published when a player selects a choice.
    /// </summary>
    public class EchoChoiceSelectedEvent : GameEvent
    {
        public EchoChoice Choice { get; }
        public EchoChoiceSelectedEvent(EchoChoice choice) => Choice = choice;
    }

    /// <summary>
    /// Published when an Echo event is completed.
    /// </summary>
    public class EchoEventCompletedEvent : GameEvent
    {
        public EchoEventDataSO Event { get; }
        public EchoEventCompletedEvent(EchoEventDataSO evt) => Event = evt;
    }
}
