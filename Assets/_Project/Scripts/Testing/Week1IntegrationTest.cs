// ============================================
// Week1IntegrationTest.cs
// Integration tests for Week 1 core systems
// ============================================

using System;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Core.GameStates;

namespace HNR.Testing
{
    /// <summary>
    /// Integration tests for Week 1 implementation.
    /// Attach to a GameObject in scene and press [T] to run tests.
    /// </summary>
    /// <remarks>
    /// Tests:
    /// - ServiceLocator: Initialize, Register, Get, Has, Unregister, Clear
    /// - EventBus: Subscribe, Publish, Unsubscribe, Clear
    /// - GameManager: State machine, transitions, events
    /// </remarks>
    public class Week1IntegrationTest : MonoBehaviour
    {
        // ============================================
        // Test Tracking
        // ============================================

        private int _passCount;
        private int _failCount;
        private bool _eventReceived;
        private GameState _receivedPreviousState;
        private GameState _receivedNewState;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Update()
        {
            // Press T to run tests
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunAllTests();
            }
        }

        // ============================================
        // Test Orchestration
        // ============================================

        /// <summary>
        /// Run all Week 1 integration tests.
        /// </summary>
        public void RunAllTests()
        {
            Debug.Log("========================================");
            Debug.Log("WEEK 1 INTEGRATION TESTS");
            Debug.Log("========================================");

            // Reset counters
            _passCount = 0;
            _failCount = 0;

            // Run test suites
            TestServiceLocator();
            TestEventBus();
            TestGameManager();

            // Summary
            Debug.Log("========================================");
            Debug.Log($"Week 1 Tests: {_passCount}/{_passCount + _failCount} passed");
            if (_failCount == 0)
            {
                Debug.Log("<color=green>ALL TESTS PASSED!</color>");
            }
            else
            {
                Debug.Log($"<color=red>{_failCount} TESTS FAILED</color>");
            }
            Debug.Log("========================================");
        }

        // ============================================
        // ServiceLocator Tests
        // ============================================

        private void TestServiceLocator()
        {
            Debug.Log("--- ServiceLocator Tests ---");

            // Cleanup any existing state
            ServiceLocator.Clear();

            // Test 1: Initialize
            try
            {
                ServiceLocator.Initialize();
                Log("ServiceLocator.Initialize()", ServiceLocator.IsInitialized);
            }
            catch (Exception e)
            {
                Log("ServiceLocator.Initialize()", false, e.Message);
            }

            // Test 2: Register and Get
            try
            {
                var mockService = new MockService { Value = 42 };
                ServiceLocator.Register<IMockService>(mockService);
                var retrieved = ServiceLocator.Get<IMockService>();
                bool passed = retrieved != null && retrieved.Value == 42;
                Log("ServiceLocator.Register/Get<T>()", passed, passed ? "" : "Retrieved service mismatch");
            }
            catch (Exception e)
            {
                Log("ServiceLocator.Register/Get<T>()", false, e.Message);
            }

            // Test 3: Has<T> returns true for registered
            try
            {
                bool hasService = ServiceLocator.Has<IMockService>();
                Log("ServiceLocator.Has<T>() returns true", hasService);
            }
            catch (Exception e)
            {
                Log("ServiceLocator.Has<T>() returns true", false, e.Message);
            }

            // Test 4: Has<T> returns false for unregistered
            try
            {
                bool hasUnregistered = ServiceLocator.Has<IUnregisteredService>();
                Log("ServiceLocator.Has<T>() returns false for unregistered", !hasUnregistered);
            }
            catch (Exception e)
            {
                Log("ServiceLocator.Has<T>() returns false for unregistered", false, e.Message);
            }

            // Test 5: Unregister
            try
            {
                ServiceLocator.Unregister<IMockService>();
                bool hasAfterUnregister = ServiceLocator.Has<IMockService>();
                Log("ServiceLocator.Unregister<T>()", !hasAfterUnregister);
            }
            catch (Exception e)
            {
                Log("ServiceLocator.Unregister<T>()", false, e.Message);
            }

            // Test 6: Clear
            try
            {
                ServiceLocator.Register<IMockService>(new MockService());
                ServiceLocator.Clear();
                bool isCleared = !ServiceLocator.Has<IMockService>() && !ServiceLocator.IsInitialized;
                Log("ServiceLocator.Clear()", isCleared);
            }
            catch (Exception e)
            {
                Log("ServiceLocator.Clear()", false, e.Message);
            }
        }

        // ============================================
        // EventBus Tests
        // ============================================

        private void TestEventBus()
        {
            Debug.Log("--- EventBus Tests ---");

            // Cleanup
            EventBus.Clear();

            // Test 1: Subscribe and Publish
            try
            {
                _eventReceived = false;
                EventBus.Subscribe<TestEvent>(OnTestEvent);
                EventBus.Publish(new TestEvent { Message = "Hello" });
                Log("EventBus.Subscribe/Publish()", _eventReceived);
            }
            catch (Exception e)
            {
                Log("EventBus.Subscribe/Publish()", false, e.Message);
            }

            // Test 2: HasSubscribers
            try
            {
                bool hasSubscribers = EventBus.HasSubscribers<TestEvent>();
                Log("EventBus.HasSubscribers<T>()", hasSubscribers);
            }
            catch (Exception e)
            {
                Log("EventBus.HasSubscribers<T>()", false, e.Message);
            }

            // Test 3: GetSubscriberCount
            try
            {
                int count = EventBus.GetSubscriberCount<TestEvent>();
                Log("EventBus.GetSubscriberCount<T>()", count == 1, $"Expected 1, got {count}");
            }
            catch (Exception e)
            {
                Log("EventBus.GetSubscriberCount<T>()", false, e.Message);
            }

            // Test 4: Unsubscribe
            try
            {
                _eventReceived = false;
                EventBus.Unsubscribe<TestEvent>(OnTestEvent);
                EventBus.Publish(new TestEvent { Message = "Should not receive" });
                Log("EventBus.Unsubscribe<T>()", !_eventReceived);
            }
            catch (Exception e)
            {
                Log("EventBus.Unsubscribe<T>()", false, e.Message);
            }

            // Test 5: Clear
            try
            {
                EventBus.Subscribe<TestEvent>(OnTestEvent);
                EventBus.Clear();
                bool hasAfterClear = EventBus.HasSubscribers<TestEvent>();
                Log("EventBus.Clear()", !hasAfterClear);
            }
            catch (Exception e)
            {
                Log("EventBus.Clear()", false, e.Message);
            }
        }

        // ============================================
        // GameManager Tests
        // ============================================

        private void TestGameManager()
        {
            Debug.Log("--- GameManager Tests ---");

            // Setup - ensure clean state
            ServiceLocator.Clear();
            EventBus.Clear();
            ServiceLocator.Initialize();

            // Test 1: GameManager exists (if in scene)
            var gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager == null)
            {
                // Create temporary GameManager for testing
                var go = new GameObject("[TestGameManager]");
                gameManager = go.AddComponent<GameManager>();
                Log("GameManager created for testing", true);
            }
            else
            {
                Log("GameManager exists in scene", true);
            }

            // Test 2: GameManager registered with ServiceLocator
            try
            {
                // GameManager registers itself in Awake, but we may have just created it
                // Give it a frame or check directly
                bool isRegistered = ServiceLocator.Has<IGameManager>();
                Log("GameManager registered with ServiceLocator", isRegistered);
            }
            catch (Exception e)
            {
                Log("GameManager registered with ServiceLocator", false, e.Message);
            }

            // Test 3: Get GameManager via ServiceLocator
            try
            {
                var retrieved = ServiceLocator.Get<IGameManager>();
                Log("ServiceLocator.Get<IGameManager>()", retrieved != null);
            }
            catch (Exception e)
            {
                Log("ServiceLocator.Get<IGameManager>()", false, e.Message);
            }

            // Test 4: GameStateChangedEvent fires on transition
            try
            {
                _eventReceived = false;
                EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

                var gm = ServiceLocator.Get<IGameManager>();
                gm.ChangeState(GameState.MainMenu);

                bool eventFired = _eventReceived;
                bool correctTransition = _receivedNewState == GameState.MainMenu;

                EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);

                Log("GameStateChangedEvent fires on transition", eventFired && correctTransition,
                    eventFired ? "" : "Event not received");
            }
            catch (Exception e)
            {
                Log("GameStateChangedEvent fires on transition", false, e.Message);
            }

            // Test 5: CurrentState property updates
            try
            {
                var gm = ServiceLocator.Get<IGameManager>();
                gm.ChangeState(GameState.Bastion);
                Log("CurrentState updates correctly", gm.CurrentState == GameState.Bastion);
            }
            catch (Exception e)
            {
                Log("CurrentState updates correctly", false, e.Message);
            }

            // Test 6-10: Test all 6 states can be entered
            TestStateTransition(GameState.Boot);
            TestStateTransition(GameState.MainMenu);
            TestStateTransition(GameState.Bastion);
            TestStateTransition(GameState.Run);
            TestStateTransition(GameState.Combat);
            TestStateTransition(GameState.Results);
        }

        /// <summary>
        /// Test transition to a specific state.
        /// </summary>
        private void TestStateTransition(GameState state)
        {
            try
            {
                var gm = ServiceLocator.Get<IGameManager>();
                gm.ChangeState(state);
                bool passed = gm.CurrentState == state;
                Log($"ChangeState({state})", passed);
            }
            catch (Exception e)
            {
                Log($"ChangeState({state})", false, e.Message);
            }
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnTestEvent(TestEvent evt)
        {
            _eventReceived = true;
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            _eventReceived = true;
            _receivedPreviousState = evt.PreviousState;
            _receivedNewState = evt.NewState;
        }

        // ============================================
        // Logging
        // ============================================

        /// <summary>
        /// Log test result with consistent formatting.
        /// </summary>
        private void Log(string testName, bool passed, string reason = "")
        {
            if (passed)
            {
                _passCount++;
                Debug.Log($"<color=green>[TEST PASS]</color> {testName}");
            }
            else
            {
                _failCount++;
                string message = string.IsNullOrEmpty(reason)
                    ? $"<color=red>[TEST FAIL]</color> {testName}"
                    : $"<color=red>[TEST FAIL]</color> {testName}: {reason}";
                Debug.Log(message);
            }
        }
    }

    // ============================================
    // Test Support Classes
    // ============================================

    /// <summary>
    /// Test event for EventBus testing.
    /// </summary>
    public class TestEvent : GameEvent
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// Mock service interface for testing.
    /// </summary>
    public interface IMockService
    {
        int Value { get; set; }
    }

    /// <summary>
    /// Mock service implementation for testing.
    /// </summary>
    public class MockService : IMockService
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Unregistered service interface for negative testing.
    /// </summary>
    public interface IUnregisteredService { }
}
