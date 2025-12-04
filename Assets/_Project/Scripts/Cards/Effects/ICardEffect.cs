// ============================================
// ICardEffect.cs
// Interface for card effect handlers
// ============================================

namespace HNR.Cards
{
    /// <summary>
    /// Interface for card effect handlers.
    /// Each effect type has a corresponding handler implementing this interface.
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// Execute this effect with the given data and context.
        /// </summary>
        /// <param name="data">Effect data from CardDataSO</param>
        /// <param name="context">Runtime context with source, target, managers</param>
        void Execute(CardEffectData data, EffectContext context);
    }
}
