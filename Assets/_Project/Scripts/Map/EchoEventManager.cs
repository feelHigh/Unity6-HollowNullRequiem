// ============================================
// EchoEventManager.cs
// Manages Echo (narrative) event execution
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;
using HNR.Combat;

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
            // TODO: Integrate with RunManager/CurrencyManager
            Debug.Log($"[EchoEvent] Gold {(amount >= 0 ? "+" : "")}{amount}");
        }

        private void ApplyHPChange(int amount)
        {
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                if (amount > 0)
                {
                    turnManager.HealTeam(amount);
                    Debug.Log($"[EchoEvent] Healed {amount} HP");
                }
                else
                {
                    turnManager.DamageTeam(-amount);
                    Debug.Log($"[EchoEvent] Damaged {-amount} HP");
                }
            }
            else
            {
                Debug.Log($"[EchoEvent] HP {(amount >= 0 ? "+" : "")}{amount} (TurnManager not available)");
            }
        }

        private void ApplyMaxHPChange(int amount)
        {
            // TODO: Integrate with team max HP system
            Debug.Log($"[EchoEvent] Max HP {(amount >= 0 ? "+" : "")}{amount}");
        }

        private void ApplyCorruptionChange(int amount)
        {
            if (ServiceLocator.TryGet<CorruptionManager>(out var corruptionManager))
            {
                if (amount > 0)
                {
                    // Apply to random team member or spread
                    Debug.Log($"[EchoEvent] Corruption +{amount}");
                }
                else
                {
                    Debug.Log($"[EchoEvent] Corruption {amount}");
                }
            }
            else
            {
                Debug.Log($"[EchoEvent] Corruption {(amount >= 0 ? "+" : "")}{amount} (CorruptionManager not available)");
            }
        }

        private void AddCard(string cardId)
        {
            // TODO: Integrate with DeckManager/RunManager
            Debug.Log($"[EchoEvent] Added card: {cardId}");
        }

        private void RemoveCard(string cardId)
        {
            // TODO: Integrate with DeckManager/RunManager
            Debug.Log($"[EchoEvent] Removed card: {cardId}");
        }

        private void UpgradeRandomCard()
        {
            // TODO: Integrate with card upgrade system
            Debug.Log("[EchoEvent] Upgraded random card");
        }

        private void AddRelic(string relicId)
        {
            // TODO: Integrate with RelicManager
            Debug.Log($"[EchoEvent] Added relic: {relicId}");
        }

        private void AddRandomCard()
        {
            // TODO: Integrate with card reward system
            Debug.Log("[EchoEvent] Added random card");
        }

        private void AddRandomRelic()
        {
            // TODO: Integrate with relic reward system
            Debug.Log("[EchoEvent] Added random relic");
        }

        private void StartCombatEncounter()
        {
            // TODO: Setup special combat from Echo event
            Debug.Log("[EchoEvent] Starting combat encounter");
        }
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
