// ============================================
// ShopManager.cs
// Void Market shop service
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Cards;

namespace HNR.Progression
{
    /// <summary>
    /// Manages Void Market shop operations including currency and purchases.
    /// Registers with ServiceLocator as IShopManager.
    /// </summary>
    public class ShopManager : MonoBehaviour, IShopManager
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("Shop generation configuration")]
        private ShopConfigSO _config;

        // ============================================
        // Private Fields
        // ============================================

        private int _voidShards;
        private ShopInventory _currentInventory;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Current Void Shard balance.</summary>
        public int VoidShards => _voidShards;

        /// <summary>Currently open shop inventory (null if closed).</summary>
        public ShopInventory CurrentInventory => _currentInventory;

        /// <summary>Shop configuration asset.</summary>
        public ShopConfigSO Config => _config;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register<IShopManager>(this);
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (ServiceLocator.Has<IShopManager>())
            {
                ServiceLocator.Unregister<IShopManager>();
            }
        }

        // ============================================
        // Currency Methods
        // ============================================

        /// <summary>
        /// Add Void Shards to the player's balance.
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        public void AddVoidShards(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[ShopManager] Invalid add amount: {amount}");
                return;
            }

            int oldAmount = _voidShards;
            _voidShards += amount;

            EventBus.Publish(new VoidShardsChangedEvent(oldAmount, _voidShards));
            Debug.Log($"[ShopManager] VoidShards: {oldAmount} → {_voidShards} (+{amount})");
        }

        /// <summary>
        /// Attempt to spend Void Shards.
        /// </summary>
        /// <param name="amount">Amount to spend.</param>
        /// <returns>True if successful.</returns>
        public bool SpendVoidShards(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[ShopManager] Invalid spend amount: {amount}");
                return false;
            }

            if (_voidShards < amount)
            {
                Debug.Log($"[ShopManager] Insufficient shards: {_voidShards}/{amount}");
                return false;
            }

            int oldAmount = _voidShards;
            _voidShards -= amount;

            EventBus.Publish(new VoidShardsChangedEvent(oldAmount, _voidShards));
            Debug.Log($"[ShopManager] VoidShards: {oldAmount} → {_voidShards} (-{amount})");
            return true;
        }

        /// <summary>
        /// Set Void Shard balance directly (for save/load).
        /// </summary>
        /// <param name="amount">New balance.</param>
        public void SetVoidShards(int amount)
        {
            int oldAmount = _voidShards;
            _voidShards = Mathf.Max(0, amount);

            if (oldAmount != _voidShards)
            {
                EventBus.Publish(new VoidShardsChangedEvent(oldAmount, _voidShards));
            }

            Debug.Log($"[ShopManager] VoidShards set to: {_voidShards}");
        }

        // ============================================
        // Shop Methods
        // ============================================

        /// <summary>
        /// Open a shop for the specified zone.
        /// </summary>
        /// <param name="zoneNumber">Current zone number.</param>
        public void OpenShop(int zoneNumber)
        {
            if (_config == null)
            {
                Debug.LogError("[ShopManager] No ShopConfigSO assigned!");
                return;
            }

            _currentInventory = ShopGenerator.GenerateInventory(_config, zoneNumber);

            EventBus.Publish(new ShopOpenedEvent(zoneNumber, _currentInventory.ItemCount));
            Debug.Log($"[ShopManager] Shop opened for Zone {zoneNumber} with {_currentInventory.ItemCount} items");
        }

        /// <summary>
        /// Close the current shop.
        /// </summary>
        public void CloseShop()
        {
            if (_currentInventory == null)
            {
                Debug.Log("[ShopManager] No shop to close");
                return;
            }

            _currentInventory = null;

            EventBus.Publish(new ShopClosedEvent());
            Debug.Log("[ShopManager] Shop closed");
        }

        /// <summary>
        /// Attempt to purchase an item from the shop.
        /// </summary>
        /// <param name="item">Item to purchase.</param>
        /// <returns>True if purchase was successful.</returns>
        public bool PurchaseItem(ShopItem item)
        {
            if (item == null)
            {
                Debug.LogWarning("[ShopManager] Cannot purchase null item");
                return false;
            }

            if (item.IsPurchased)
            {
                Debug.Log($"[ShopManager] Item already purchased: {item.DisplayName}");
                return false;
            }

            if (!SpendVoidShards(item.Price))
            {
                return false;
            }

            // Apply purchase effect based on item type
            ApplyPurchaseEffect(item);

            // Mark as purchased
            item.IsPurchased = true;

            // Publish purchase event
            EventBus.Publish(new ShopItemPurchasedEvent(item));
            Debug.Log($"[ShopManager] Purchased: {item.DisplayName} for {item.Price} shards");

            return true;
        }

        // ============================================
        // Private Methods
        // ============================================

        /// <summary>
        /// Apply the effect of purchasing an item.
        /// </summary>
        private void ApplyPurchaseEffect(ShopItem item)
        {
            switch (item.Type)
            {
                case ShopItemType.Card:
                    if (item.CardData != null)
                    {
                        // Publish event for RunManager to handle deck addition
                        EventBus.Publish(new CardAddedToDeckEvent(item.CardData));
                        Debug.Log($"[ShopManager] Card added to deck: {item.CardData.CardName}");
                    }
                    break;

                case ShopItemType.Relic:
                    if (item.RelicData != null)
                    {
                        // Publish event for RelicManager to handle
                        EventBus.Publish(new RelicAcquiredEvent(item.RelicData));
                        Debug.Log($"[ShopManager] Relic acquired: {item.RelicData.RelicName}");
                    }
                    break;

                case ShopItemType.CardRemove:
                    // UI will handle card selection, then publish CardRemovedFromDeckEvent
                    Debug.Log("[ShopManager] Card remove purchased - awaiting card selection");
                    break;

                case ShopItemType.Purification:
                    // UI will handle Requiem selection for corruption reduction
                    Debug.Log($"[ShopManager] Purification purchased - reduces corruption by {_config.PurificationAmount}");
                    break;

                case ShopItemType.Consumable:
                    // TODO: Implement consumable system
                    Debug.Log("[ShopManager] Consumable purchased");
                    break;
            }
        }
    }
}
