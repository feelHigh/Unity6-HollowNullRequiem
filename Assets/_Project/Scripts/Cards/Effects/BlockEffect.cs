// ============================================
// BlockEffect.cs
// Block effect implementation
// ============================================

using UnityEngine;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.Cards
{
    /// <summary>
    /// Gain Block for the team.
    /// Block absorbs damage until the start of next turn.
    /// </summary>
    public class BlockEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            // Get base block and apply card modifiers
            int block = data.Value;
            if (context.Card != null)
            {
                block = context.Card.GetModifiedBlock(block);
            }

            // Apply context multiplier (from buffs like Dexterity)
            block = context.CalculateBlock(block);

            // Add block to team
            if (context.TurnManager != null)
            {
                context.TurnManager.AddTeamBlock(block);
            }
            else if (context.CombatContext != null)
            {
                int previousBlock = context.CombatContext.TeamBlock;
                context.CombatContext.TeamBlock += block;
                EventBus.Publish(new BlockChangedEvent(context.CombatContext.TeamBlock, previousBlock));
            }

            // Publish block gained event (null target indicates team-wide block)
            EventBus.Publish(new BlockGainedEvent(null, block));

            Debug.Log($"[BlockEffect] Gained {block} block");
        }
    }
}
