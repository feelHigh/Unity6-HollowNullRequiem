// ============================================
// NodeEventHandler.cs
// Handles events from map node screens
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Characters;
using HNR.Combat;
using HNR.UI;
using HNR.UI.Components;

namespace HNR.Map
{
    /// <summary>
    /// Handles events from map node screens (Sanctuary, Shop).
    /// Subscribes to node events and coordinates with RunManager, CorruptionManager.
    /// Should be attached to a persistent manager object or instantiated by GameBootstrap.
    /// </summary>
    public class NodeEventHandler : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        private static NodeEventHandler _instance;
        public static NodeEventHandler Instance => _instance;

        // ============================================
        // References
        // ============================================

        [Header("References")]
        [SerializeField, Tooltip("Deck viewer modal for card removal")]
        private DeckViewerModal _deckViewerModal;

        // ============================================
        // Lifecycle
        // ============================================

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Register with ServiceLocator
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                ServiceLocator.Unregister<NodeEventHandler>();
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        // ============================================
        // Event Subscriptions
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<SanctuaryHealEvent>(OnSanctuaryHeal);
            EventBus.Subscribe<SanctuaryPurifyEvent>(OnSanctuaryPurify);
            EventBus.Subscribe<ShopRemoveCardRequestedEvent>(OnShopRemoveCardRequested);
            EventBus.Subscribe<ShopPurifyRequestedEvent>(OnShopPurifyRequested);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<SanctuaryHealEvent>(OnSanctuaryHeal);
            EventBus.Unsubscribe<SanctuaryPurifyEvent>(OnSanctuaryPurify);
            EventBus.Unsubscribe<ShopRemoveCardRequestedEvent>(OnShopRemoveCardRequested);
            EventBus.Unsubscribe<ShopPurifyRequestedEvent>(OnShopPurifyRequested);
        }

        // ============================================
        // Sanctuary Events
        // ============================================

        private void OnSanctuaryHeal(SanctuaryHealEvent evt)
        {
            Debug.Log($"[NodeEventHandler] Sanctuary heal: {evt.HealAmount} HP");

            // Heal the team via RunManager
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                runManager.HealTeam(evt.HealAmount);

                // Publish TeamHPChangedEvent for UI updates
                EventBus.Publish(new TeamHPChangedEvent(
                    runManager.TeamCurrentHP,
                    runManager.TeamMaxHP,
                    evt.HealAmount
                ));

                Debug.Log($"[NodeEventHandler] Team HP after heal: {runManager.TeamCurrentHP}/{runManager.TeamMaxHP}");
            }
            else
            {
                Debug.LogWarning("[NodeEventHandler] RunManager not available for sanctuary heal");
            }
        }

        private void OnSanctuaryPurify(SanctuaryPurifyEvent evt)
        {
            Debug.Log($"[NodeEventHandler] Sanctuary purify: -{evt.PurifyAmount} corruption");
            PurifyTeamCorruption(evt.PurifyAmount);
        }

        // ============================================
        // Shop Events
        // ============================================

        private void OnShopRemoveCardRequested(ShopRemoveCardRequestedEvent evt)
        {
            Debug.Log("[NodeEventHandler] Shop card removal requested");
            ShowCardRemovalModal();
        }

        private void OnShopPurifyRequested(ShopPurifyRequestedEvent evt)
        {
            Debug.Log($"[NodeEventHandler] Shop purify: -{evt.PurifyAmount} corruption");
            PurifyTeamCorruption(evt.PurifyAmount);
        }

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Purifies corruption from all team members.
        /// Uses RunManager's team (not CorruptionManager which is combat-specific).
        /// </summary>
        private void PurifyTeamCorruption(int amount)
        {
            if (amount <= 0) return;

            // Get team from RunManager
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                foreach (var requiem in runManager.Team)
                {
                    if (requiem != null && !requiem.IsDead)
                    {
                        int previousCorruption = requiem.Corruption;
                        requiem.RemoveCorruption(amount);
                        Debug.Log($"[NodeEventHandler] {requiem.Name} purified {amount} corruption. Total: {requiem.Corruption}");

                        // Publish event for UI updates per requiem
                        EventBus.Publish(new CorruptionChangedEvent(requiem, previousCorruption, requiem.Corruption));
                    }
                }
            }
            else
            {
                Debug.LogWarning("[NodeEventHandler] RunManager not available for purification");
            }
        }

        /// <summary>
        /// Shows the deck viewer modal for card removal.
        /// </summary>
        private void ShowCardRemovalModal()
        {
            // Try to find modal if not assigned
            if (_deckViewerModal == null)
            {
                _deckViewerModal = FindAnyObjectByType<DeckViewerModal>(FindObjectsInactive.Include);
            }

            if (_deckViewerModal != null)
            {
                _deckViewerModal.Show(DeckViewerModal.ViewMode.RemoveCard, (removedCard) =>
                {
                    if (removedCard != null)
                    {
                        Debug.Log($"[NodeEventHandler] Card removed from deck: {removedCard.CardName}");
                    }
                    else
                    {
                        Debug.Log("[NodeEventHandler] Card removal cancelled");
                    }
                });
            }
            else
            {
                Debug.LogWarning("[NodeEventHandler] DeckViewerModal not found - card removal unavailable");

                // Fallback: Just log that this feature needs the modal to be set up
                // The shards have already been spent, so we need to handle this gracefully
                // For now, just warn the user
                if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
                {
                    // TODO: Show toast notification about feature unavailability
                }
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Sets the deck viewer modal reference.
        /// </summary>
        public void SetDeckViewerModal(DeckViewerModal modal)
        {
            _deckViewerModal = modal;
        }
    }
}
