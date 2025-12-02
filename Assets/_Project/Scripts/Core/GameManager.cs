// ============================================
// GameManager.cs
// Master game state machine controller
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core.Events;
using HNR.Core.GameStates;
using HNR.Core.Interfaces;

namespace HNR.Core
{
    /// <summary>
    /// Master game state controller. Manages high-level game flow
    /// using the state machine pattern.
    /// </summary>
    /// <remarks>
    /// Singleton via ServiceLocator. Created in Boot scene and persists
    /// across all scene loads via DontDestroyOnLoad.
    ///
    /// State flow: Boot → MainMenu → Bastion → Run ↔ Combat → Results
    /// </remarks>
    public class GameManager : MonoBehaviour, IGameManager
    {
        // ============================================
        // State Machine
        // ============================================

        private Dictionary<GameState, IGameState> _states;
        private IGameState _currentStateObject;

        /// <summary>
        /// Gets the current high-level game state.
        /// </summary>
        public GameState CurrentState { get; private set; }

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Persist across scene loads
            DontDestroyOnLoad(gameObject);

            // Initialize ServiceLocator if not already done
            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.Initialize();
            }

            // Register self with ServiceLocator
            ServiceLocator.Register<IGameManager>(this);

            // Initialize state objects
            InitializeStates();

            Debug.Log("[GameManager] Initialized.");
        }

        private void Start()
        {
            // Begin in Boot state
            ChangeState(GameState.Boot);
        }

        private void Update()
        {
            // Delegate update to current state
            _currentStateObject?.Update();
        }

        private void OnDestroy()
        {
            // Cleanup current state
            _currentStateObject?.Exit();

            // Unregister from ServiceLocator
            if (ServiceLocator.Has<IGameManager>())
            {
                ServiceLocator.Unregister<IGameManager>();
            }

            Debug.Log("[GameManager] Destroyed.");
        }

        // ============================================
        // State Machine Methods
        // ============================================

        /// <summary>
        /// Initialize all state objects.
        /// States hold a reference to this GameManager for callbacks.
        /// </summary>
        private void InitializeStates()
        {
            _states = new Dictionary<GameState, IGameState>
            {
                { GameState.Boot, new BootState(this) },
                { GameState.MainMenu, new MainMenuState(this) },
                { GameState.Bastion, new BastionState(this) },
                { GameState.Run, new RunState(this) },
                { GameState.Combat, new CombatState(this) },
                { GameState.Results, new ResultsState(this) }
            };

            Debug.Log($"[GameManager] Initialized {_states.Count} states.");
        }

        /// <summary>
        /// Transition to a new game state.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        public void ChangeState(GameState newState)
        {
            // Prevent redundant transitions
            if (_currentStateObject != null && CurrentState == newState)
            {
                Debug.LogWarning($"[GameManager] Already in state: {newState}");
                return;
            }

            // Validate state exists
            if (!_states.ContainsKey(newState))
            {
                Debug.LogError($"[GameManager] State not found: {newState}");
                return;
            }

            var previousState = CurrentState;

            // Exit current state
            _currentStateObject?.Exit();

            // Update state
            CurrentState = newState;
            _currentStateObject = _states[newState];

            // Enter new state
            _currentStateObject.Enter();

            // Publish state change event
            EventBus.Publish(new GameStateChangedEvent(previousState, newState));

            Debug.Log($"[GameManager] State: {previousState} → {newState}");
        }

        // ============================================
        // IGameManager Implementation
        // ============================================

        /// <summary>
        /// Begin a new run with the currently selected team.
        /// </summary>
        public void StartNewRun()
        {
            Debug.Log("[GameManager] Starting new run...");

            // TODO: Initialize run data via RunManager
            // var runManager = ServiceLocator.Get<IRunManager>();
            // runManager.InitializeNewRun();

            ChangeState(GameState.Run);
        }

        /// <summary>
        /// End the current run.
        /// </summary>
        /// <param name="victory">True if the run was completed successfully</param>
        public void EndRun(bool victory)
        {
            Debug.Log($"[GameManager] Ending run. Victory: {victory}");

            // TODO: Gather run statistics
            // var runManager = ServiceLocator.Get<IRunManager>();
            // var stats = runManager.GetRunStats();
            // EventBus.Publish(new RunEndedEvent(victory, stats.FloorsCleared, stats.EnemiesDefeated));

            ChangeState(GameState.Results);
        }

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Get the current state object (for debugging).
        /// </summary>
        public IGameState GetCurrentStateObject() => _currentStateObject;

        /// <summary>
        /// Check if we're in a specific state.
        /// </summary>
        public bool IsInState(GameState state) => CurrentState == state;
    }

    // ============================================
    // STATE IMPLEMENTATIONS
    // Placeholder implementations - expand as needed
    // ============================================

    /// <summary>
    /// Boot state - initializes core systems and transitions to MainMenu.
    /// </summary>
    public class BootState : IGameState
    {
        private readonly GameManager _manager;

        public BootState(GameManager manager) => _manager = manager;

        public void Enter()
        {
            Debug.Log("[BootState] Initializing core systems...");

            // TODO: Initialize remaining services
            // - SaveManager
            // - AudioManager
            // - PoolManager
            // - UIManager

            // Transition to MainMenu after initialization
            _manager.ChangeState(GameState.MainMenu);
        }

        public void Update() { }
        public void Exit() { }
    }

    /// <summary>
    /// MainMenu state - title screen and menu navigation.
    /// </summary>
    public class MainMenuState : IGameState
    {
        private readonly GameManager _manager;

        public MainMenuState(GameManager manager) => _manager = manager;

        public void Enter()
        {
            Debug.Log("[MainMenuState] Showing main menu...");

            // TODO: Load MainMenu scene
            // TODO: Show MainMenuScreen via UIManager
            // TODO: Play menu music via AudioManager
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log("[MainMenuState] Exiting main menu...");
        }
    }

    /// <summary>
    /// Bastion state - hub area for run preparation.
    /// </summary>
    public class BastionState : IGameState
    {
        private readonly GameManager _manager;

        public BastionState(GameManager manager) => _manager = manager;

        public void Enter()
        {
            Debug.Log("[BastionState] Entering Bastion hub...");

            // TODO: Load Bastion scene
            // TODO: Show BastionScreen via UIManager
            // TODO: Play bastion music
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log("[BastionState] Leaving Bastion...");
        }
    }

    /// <summary>
    /// Run state - active run, navigating the Null Rift map.
    /// </summary>
    public class RunState : IGameState
    {
        private readonly GameManager _manager;

        public RunState(GameManager manager) => _manager = manager;

        public void Enter()
        {
            Debug.Log("[RunState] Starting run navigation...");

            // TODO: Load NullRift scene
            // TODO: Show MapScreen via UIManager
            // TODO: Generate or load map
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log("[RunState] Exiting run navigation...");
        }
    }

    /// <summary>
    /// Combat state - active card combat encounter.
    /// </summary>
    public class CombatState : IGameState
    {
        private readonly GameManager _manager;

        public CombatState(GameManager manager) => _manager = manager;

        public void Enter()
        {
            Debug.Log("[CombatState] Entering combat...");

            // TODO: Load Combat scene
            // TODO: Show CombatScreen via UIManager
            // TODO: Initialize CombatManager
            // TODO: Play combat music
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log("[CombatState] Exiting combat...");

            // TODO: Cleanup combat systems
        }
    }

    /// <summary>
    /// Results state - post-run statistics and rewards.
    /// </summary>
    public class ResultsState : IGameState
    {
        private readonly GameManager _manager;

        public ResultsState(GameManager manager) => _manager = manager;

        public void Enter()
        {
            Debug.Log("[ResultsState] Showing run results...");

            // TODO: Show ResultsScreen via UIManager
            // TODO: Display statistics
            // TODO: Clear saved run data
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log("[ResultsState] Exiting results...");
        }
    }
}
