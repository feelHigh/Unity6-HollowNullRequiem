// ============================================
// GameManager.cs
// Master game state machine controller
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core.Events;
using HNR.Core.GameStates;
using HNR.Core.Interfaces;
using HNR.Characters;

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
        // Pending Team Selection
        // ============================================

        private List<RequiemDataSO> _pendingTeam = new();

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

        private void OnEnable()
        {
            // Subscribe to events
            EventBus.Subscribe<TeamSelectedEvent>(OnTeamSelected);
            Debug.Log($"[GameManager] OnEnable - Subscribed to TeamSelectedEvent. Subscriber count: {EventBus.GetSubscriberCount<TeamSelectedEvent>()}");
        }

        private void OnTeamSelected(TeamSelectedEvent evt)
        {
            Debug.Log($"[GameManager] OnTeamSelected received! Event has {evt.SelectedTeam?.Count ?? 0} Requiems");
            _pendingTeam.Clear();
            if (evt.SelectedTeam != null)
            {
                _pendingTeam.AddRange(evt.SelectedTeam);
            }
            Debug.Log($"[GameManager] _pendingTeam now has {_pendingTeam.Count} Requiems");
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

        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<TeamSelectedEvent>(OnTeamSelected);
            Debug.Log("[GameManager] OnDisable - Unsubscribed from TeamSelectedEvent");
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
            Debug.Log($"[GameManager] Starting new run... Pending team count: {_pendingTeam.Count}");

            // Initialize run data via RunManager
            if (_pendingTeam.Count > 0)
            {
                var runManager = ServiceLocator.Get<IRunManager>();
                if (runManager != null)
                {
                    Debug.Log($"[GameManager] Calling RunManager.InitializeNewRun with {_pendingTeam.Count} Requiems: {string.Join(", ", _pendingTeam.ConvertAll(r => r?.RequiemName ?? "null"))}");
                    runManager.InitializeNewRun(_pendingTeam);
                    Debug.Log($"[GameManager] After init - RunManager.IsRunActive: {runManager.IsRunActive}, Team.Count: {runManager.Team?.Count ?? -1}");
                }
                else
                {
                    Debug.LogError("[GameManager] RunManager not found - cannot initialize run!");
                }
            }
            else
            {
                Debug.LogError("[GameManager] No team selected - _pendingTeam is empty! Did TeamSelectedEvent fire?");
            }

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

}
