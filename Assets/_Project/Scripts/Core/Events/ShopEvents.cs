// ============================================
// ShopEvents.cs
// Shop-related event definitions for EventBus
// ============================================

using HNR.Cards;
using HNR.Progression;

namespace HNR.Core.Events
{
    // ============================================
    // CURRENCY EVENTS
    // ============================================

    /// <summary>
    /// Published when Void Shards currency amount changes.
    /// </summary>
    public class VoidShardsChangedEvent : GameEvent
    {
        /// <summary>Amount before the change.</summary>
        public int PreviousValue { get; }

        /// <summary>Amount after the change.</summary>
        public int NewValue { get; }

        /// <summary>The change in shards (positive = gained, negative = spent).</summary>
        public int Delta => NewValue - PreviousValue;

        public VoidShardsChangedEvent(int previousValue, int newValue)
        {
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }

    // ============================================
    // SHOP EVENTS
    // ============================================

    /// <summary>
    /// Published when a shop is opened.
    /// </summary>
    public class ShopOpenedEvent : GameEvent
    {
        /// <summary>Zone number where shop was opened.</summary>
        public int ZoneNumber { get; }

        /// <summary>Number of items in the shop.</summary>
        public int ItemCount { get; }

        public ShopOpenedEvent(int zoneNumber, int itemCount)
        {
            ZoneNumber = zoneNumber;
            ItemCount = itemCount;
        }
    }

    /// <summary>
    /// Published when a shop is closed.
    /// </summary>
    public class ShopClosedEvent : GameEvent
    {
        public ShopClosedEvent() { }
    }

    /// <summary>
    /// Published when an item is purchased from the shop.
    /// </summary>
    public class ShopItemPurchasedEvent : GameEvent
    {
        /// <summary>The purchased item.</summary>
        public ShopItem Item { get; }

        /// <summary>Type of item purchased.</summary>
        public ShopItemType ItemType => Item?.Type ?? ShopItemType.Card;

        /// <summary>Price paid for the item.</summary>
        public int Price => Item?.Price ?? 0;

        public ShopItemPurchasedEvent(ShopItem item)
        {
            Item = item;
        }
    }

    // ============================================
    // DECK MODIFICATION EVENTS
    // ============================================

    /// <summary>
    /// Published when a card is added to the deck (from shop, rewards, events).
    /// </summary>
    public class CardAddedToDeckEvent : GameEvent
    {
        /// <summary>The card data that was added.</summary>
        public CardDataSO Card { get; }

        /// <summary>Card ID for serialization.</summary>
        public string CardId => Card?.CardId;

        public CardAddedToDeckEvent(CardDataSO card)
        {
            Card = card;
        }
    }

    /// <summary>
    /// Published when a card is removed from the deck (from shop service, events).
    /// </summary>
    public class CardRemovedFromDeckEvent : GameEvent
    {
        /// <summary>The card data that was removed.</summary>
        public CardDataSO Card { get; }

        /// <summary>Card ID for serialization.</summary>
        public string CardId => Card?.CardId;

        public CardRemovedFromDeckEvent(CardDataSO card)
        {
            Card = card;
        }
    }

    // ============================================
    // RELIC EVENTS
    // ============================================

    /// <summary>
    /// Published when a relic is acquired (from shop, rewards, events).
    /// </summary>
    public class RelicAcquiredEvent : GameEvent
    {
        /// <summary>The relic data that was acquired.</summary>
        public RelicDataSO Relic { get; }

        /// <summary>Relic ID for serialization.</summary>
        public string RelicId => Relic?.RelicId;

        public RelicAcquiredEvent(RelicDataSO relic)
        {
            Relic = relic;
        }
    }

    /// <summary>
    /// Published when a relic effect is triggered.
    /// </summary>
    public class RelicTriggeredEvent : GameEvent
    {
        /// <summary>The relic that was triggered.</summary>
        public RelicDataSO Relic { get; }

        /// <summary>The trigger type that activated the relic.</summary>
        public RelicTrigger Trigger => Relic?.Trigger ?? RelicTrigger.Passive;

        /// <summary>The effect that was applied.</summary>
        public RelicEffectType EffectType => Relic?.EffectType ?? RelicEffectType.Healing;

        /// <summary>The value of the effect.</summary>
        public int EffectValue => Relic?.EffectValue ?? 0;

        public RelicTriggeredEvent(RelicDataSO relic)
        {
            Relic = relic;
        }
    }

    // ============================================
    // CONSUMABLE EVENTS
    // ============================================

    /// <summary>
    /// Published when a consumable is purchased from the shop.
    /// RunManager/InventoryManager should handle adding to inventory.
    /// </summary>
    public class ConsumablePurchasedEvent : GameEvent
    {
        /// <summary>The shop item containing consumable data.</summary>
        public ShopItem Item { get; }

        public ConsumablePurchasedEvent(ShopItem item)
        {
            Item = item;
        }
    }
}
