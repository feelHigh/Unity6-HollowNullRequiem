// ============================================
// IShopManager.cs
// Shop service interface for Void Market
// ============================================

using HNR.Progression;

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Shop service for managing Void Market transactions.
    /// Handles currency, inventory generation, and purchases.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: ShopManager (MonoBehaviour)
    /// </remarks>
    public interface IShopManager
    {
        /// <summary>
        /// Current Void Shard balance.
        /// </summary>
        int VoidShards { get; }

        /// <summary>
        /// Currently open shop inventory.
        /// Null if no shop is open.
        /// </summary>
        ShopInventory CurrentInventory { get; }

        /// <summary>
        /// Add Void Shards to the player's balance.
        /// Publishes VoidShardsChangedEvent.
        /// </summary>
        /// <param name="amount">Amount to add (positive value).</param>
        void AddVoidShards(int amount);

        /// <summary>
        /// Attempt to spend Void Shards.
        /// Publishes VoidShardsChangedEvent on success.
        /// </summary>
        /// <param name="amount">Amount to spend.</param>
        /// <returns>True if sufficient shards were available.</returns>
        bool SpendVoidShards(int amount);

        /// <summary>
        /// Open a shop for the specified zone.
        /// Generates new inventory based on zone number.
        /// </summary>
        /// <param name="zoneNumber">Current zone (1-3).</param>
        void OpenShop(int zoneNumber);

        /// <summary>
        /// Close the current shop.
        /// </summary>
        void CloseShop();

        /// <summary>
        /// Attempt to purchase an item from the shop.
        /// Deducts price from Void Shards and applies item effect.
        /// </summary>
        /// <param name="item">Item to purchase.</param>
        /// <returns>True if purchase was successful.</returns>
        bool PurchaseItem(ShopItem item);

        /// <summary>
        /// Set the Void Shard balance directly.
        /// Used for save/load operations.
        /// </summary>
        /// <param name="amount">New balance.</param>
        void SetVoidShards(int amount);
    }
}
