// ============================================
// ICombatPhase.cs
// Interface for combat phase state machine
// ============================================

namespace HNR.Combat
{
    /// <summary>
    /// Interface for combat phase state machine implementations.
    /// Each phase handles its own logic and determines transitions.
    /// </summary>
    public interface ICombatPhase
    {
        /// <summary>
        /// The type of phase this implementation represents.
        /// </summary>
        CombatPhase PhaseType { get; }

        /// <summary>
        /// Called when entering this phase.
        /// </summary>
        /// <param name="context">Shared combat context</param>
        void Enter(CombatContext context);

        /// <summary>
        /// Called every frame while in this phase.
        /// </summary>
        /// <param name="context">Shared combat context</param>
        void Update(CombatContext context);

        /// <summary>
        /// Called when exiting this phase.
        /// </summary>
        /// <param name="context">Shared combat context</param>
        void Exit(CombatContext context);

        /// <summary>
        /// Determine the next phase after this one completes.
        /// </summary>
        /// <param name="context">Shared combat context</param>
        /// <returns>The next combat phase to transition to</returns>
        CombatPhase GetNextPhase(CombatContext context);
    }
}
