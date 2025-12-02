// ============================================
// IGameState.cs
// Interface for game state implementations
// ============================================

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Interface for game state implementations in the state machine.
    /// Each state handles its own enter/update/exit logic.
    /// </summary>
    /// <remarks>
    /// Implementations: BootState, MainMenuState, BastionState, RunState, CombatState, ResultsState
    ///
    /// State objects are created once and reused. Use Enter() for initialization
    /// and Exit() for cleanup rather than constructor/destructor.
    /// </remarks>
    /// <example>
    /// public class MainMenuState : IGameState
    /// {
    ///     private readonly GameManager _manager;
    ///
    ///     public MainMenuState(GameManager manager) => _manager = manager;
    ///
    ///     public void Enter()
    ///     {
    ///         // Load scene, show UI, play music
    ///     }
    ///
    ///     public void Update()
    ///     {
    ///         // Handle per-frame logic if needed
    ///     }
    ///
    ///     public void Exit()
    ///     {
    ///         // Cleanup before leaving state
    ///     }
    /// }
    /// </example>
    public interface IGameState
    {
        /// <summary>
        /// Called when entering this state.
        /// Initialize state-specific systems, load scenes, show UI.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called every frame while in this state.
        /// Handle per-frame updates if needed (many states may have empty Update).
        /// </summary>
        void Update();

        /// <summary>
        /// Called when exiting this state.
        /// Clean up state-specific resources, hide UI, stop music.
        /// </summary>
        void Exit();
    }
}
