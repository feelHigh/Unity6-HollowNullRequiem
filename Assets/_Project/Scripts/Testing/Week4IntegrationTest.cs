// ============================================
// Week4IntegrationTest.cs
// Integration tests for Week 4: Card Effects & Targeting
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Cards;
using HNR.Combat;
using HNR.Characters;
using System.Collections.Generic;

// Resolve ambiguity: use real EnemyInstance from Combat
using EnemyInstance = HNR.Combat.EnemyInstance;

namespace HNR.Testing
{
    /// <summary>
    /// Integration tests for Week 4 systems.
    /// Press T to run all tests, Y to run card play test.
    /// </summary>
    public class Week4IntegrationTest : MonoBehaviour
    {
        // ============================================
        // Test Data
        // ============================================

        [Header("Test Cards")]
        [SerializeField, Tooltip("Strike card for damage tests")]
        private CardDataSO _testStrikeCard;

        [SerializeField, Tooltip("Guard card for block tests")]
        private CardDataSO _testGuardCard;

        [SerializeField, Tooltip("Skill card for utility tests")]
        private CardDataSO _testSkillCard;

        [Header("Test Enemies")]
        [SerializeField, Tooltip("Enemy data for combat tests")]
        private EnemyDataSO _testEnemy;

        [Header("Settings")]
        [SerializeField, Tooltip("Log detailed output")]
        private bool _verboseLogging = true;

        // ============================================
        // Runtime State
        // ============================================

        private int _passCount;
        private int _failCount;
        private List<string> _testResults = new();

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Start()
        {
            Debug.Log("[Week4Test] Press T to run all tests, Y to test card play flow");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunAllTests();
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                TestFullCardPlayFlow();
            }
        }

        // ============================================
        // Test Runner
        // ============================================

        public void RunAllTests()
        {
            _passCount = 0;
            _failCount = 0;
            _testResults.Clear();

            Debug.Log("========================================");
            Debug.Log("[Week4Test] Starting Week 4 Integration Tests");
            Debug.Log("========================================");

            // Service Tests
            TestSection("SERVICE REGISTRATION");
            TestCardExecutorExists();
            TestTargetingSystemExists();
            TestHandManagerExists();
            TestDeckManagerExists();
            TestTurnManagerExists();

            // Effect Tests
            TestSection("EFFECT HANDLERS");
            TestDamageEffectCreation();
            TestBlockEffectCreation();
            TestHealEffectCreation();
            TestDrawCardsEffectCreation();
            TestEffectHandlerRegistration();

            // Targeting Tests
            TestSection("TARGETING SYSTEM");
            TestTargetingRequirements();
            TestValidTargetRetrieval();

            // Combat Context Tests
            TestSection("COMBAT CONTEXT");
            TestAPTracking();
            TestTeamHPTracking();
            TestBlockTracking();

            // Card Instance Tests
            TestSection("CARD INSTANCES");
            TestCardInstanceCreation();
            TestCardCanPlayCheck();
            TestCardModifiers();

            // Print Summary
            Debug.Log("========================================");
            Debug.Log($"[Week4Test] RESULTS: {_passCount} PASSED, {_failCount} FAILED");
            Debug.Log("========================================");

            if (_failCount == 0)
            {
                Debug.Log("<color=green>[Week4Test] ALL TESTS PASSED!</color>");
            }
            else
            {
                Debug.LogWarning($"[Week4Test] {_failCount} tests failed. Check logs above.");
            }
        }

        private void TestSection(string sectionName)
        {
            Debug.Log($"\n--- {sectionName} ---");
        }

        // ============================================
        // Service Registration Tests
        // ============================================

        private void TestCardExecutorExists()
        {
            bool exists = ServiceLocator.TryGet<CardExecutor>(out _);
            Log("CardExecutor registered", exists);
        }

        private void TestTargetingSystemExists()
        {
            bool exists = ServiceLocator.TryGet<TargetingSystem>(out _);
            Log("TargetingSystem registered", exists);
        }

        private void TestHandManagerExists()
        {
            bool exists = ServiceLocator.TryGet<HandManager>(out _);
            Log("HandManager registered", exists);
        }

        private void TestDeckManagerExists()
        {
            bool exists = ServiceLocator.TryGet<DeckManager>(out _);
            Log("DeckManager registered", exists);
        }

        private void TestTurnManagerExists()
        {
            bool exists = ServiceLocator.TryGet<TurnManager>(out _);
            Log("TurnManager registered", exists);
        }

        // ============================================
        // Effect Handler Tests
        // ============================================

        private void TestDamageEffectCreation()
        {
            var effect = new DamageEffect();
            Log("DamageEffect instantiates", effect != null);
        }

        private void TestBlockEffectCreation()
        {
            var effect = new BlockEffect();
            Log("BlockEffect instantiates", effect != null);
        }

        private void TestHealEffectCreation()
        {
            var effect = new HealEffect();
            Log("HealEffect instantiates", effect != null);
        }

        private void TestDrawCardsEffectCreation()
        {
            var effect = new DrawCardsEffect();
            Log("DrawCardsEffect instantiates", effect != null);
        }

        private void TestEffectHandlerRegistration()
        {
            if (!ServiceLocator.TryGet<CardExecutor>(out var executor))
            {
                Log("Effect handlers registered", false, "CardExecutor not found");
                return;
            }

            bool hasDamage = executor.HasHandler(EffectType.Damage);
            bool hasBlock = executor.HasHandler(EffectType.Block);
            bool hasHeal = executor.HasHandler(EffectType.Heal);
            bool hasDraw = executor.HasHandler(EffectType.DrawCards);

            Log("DamageEffect handler registered", hasDamage);
            Log("BlockEffect handler registered", hasBlock);
            Log("HealEffect handler registered", hasHeal);
            Log("DrawCardsEffect handler registered", hasDraw);
        }

        // ============================================
        // Targeting System Tests
        // ============================================

        private void TestTargetingRequirements()
        {
            if (!ServiceLocator.TryGet<TargetingSystem>(out var targeting))
            {
                Log("Targeting requirements", false, "TargetingSystem not found");
                return;
            }

            bool singleEnemyRequires = targeting.RequiresTargeting(TargetType.SingleEnemy);
            bool singleAllyRequires = targeting.RequiresTargeting(TargetType.SingleAlly);
            bool noneDoesNot = !targeting.RequiresTargeting(TargetType.None);
            bool allEnemiesDoesNot = !targeting.RequiresTargeting(TargetType.AllEnemies);

            Log("SingleEnemy requires targeting", singleEnemyRequires);
            Log("SingleAlly requires targeting", singleAllyRequires);
            Log("None does not require targeting", noneDoesNot);
            Log("AllEnemies does not require targeting", allEnemiesDoesNot);
        }

        private void TestValidTargetRetrieval()
        {
            if (!ServiceLocator.TryGet<TargetingSystem>(out var targeting))
            {
                Log("Valid target retrieval", false, "TargetingSystem not found");
                return;
            }

            var allEnemyTargets = targeting.GetAllTargets(TargetType.AllEnemies);
            Log("GetAllTargets returns list", allEnemyTargets != null);
        }

        // ============================================
        // Combat Context Tests
        // ============================================

        private void TestAPTracking()
        {
            if (!ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                Log("AP Tracking", false, "TurnManager not found");
                return;
            }

            var context = turnManager.Context;
            Log("CombatContext exists", context != null);

            if (context != null)
            {
                Log("CurrentAP accessible", context.CurrentAP >= 0);
                Log("MaxAP accessible", context.MaxAP >= 0);
            }
        }

        private void TestTeamHPTracking()
        {
            if (!ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                Log("Team HP Tracking", false, "TurnManager not found");
                return;
            }

            var context = turnManager.Context;
            if (context != null)
            {
                Log("TeamHP accessible", context.TeamHP >= 0);
                Log("TeamMaxHP accessible", context.TeamMaxHP >= 0);
            }
        }

        private void TestBlockTracking()
        {
            if (!ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                Log("Block Tracking", false, "TurnManager not found");
                return;
            }

            var context = turnManager.Context;
            if (context != null)
            {
                Log("TeamBlock accessible", context.TeamBlock >= 0);
            }
        }

        // ============================================
        // Card Instance Tests
        // ============================================

        private void TestCardInstanceCreation()
        {
            if (_testStrikeCard == null)
            {
                Log("CardInstance creation", false, "No test card assigned");
                return;
            }

            var instance = new CardInstance(_testStrikeCard);
            Log("CardInstance created from CardDataSO", instance != null);
            Log("CardInstance has Data reference", instance?.Data != null);
            Log("CardInstance has valid cost", instance?.CurrentCost >= 0);
        }

        private void TestCardCanPlayCheck()
        {
            if (_testStrikeCard == null)
            {
                Log("Card CanPlay check", false, "No test card assigned");
                return;
            }

            var instance = new CardInstance(_testStrikeCard);
            bool canPlayWith3AP = instance.CanPlay(3);
            bool cannotPlayWith0AP = !instance.CanPlay(0);

            Log("Card playable with sufficient AP", canPlayWith3AP || instance.CurrentCost > 3);
            Log("Card not playable with 0 AP", cannotPlayWith0AP || instance.CurrentCost == 0);
        }

        private void TestCardModifiers()
        {
            if (_testStrikeCard == null)
            {
                Log("Card modifiers", false, "No test card assigned");
                return;
            }

            var instance = new CardInstance(_testStrikeCard);
            int baseCost = instance.CurrentCost;

            instance.AddModifier(new CardModifier(ModifierType.Cost, -1, 1, "Test"));
            int modifiedCost = instance.CurrentCost;

            Log("Cost modifier applied", modifiedCost == baseCost - 1 || baseCost == 0);
        }

        // ============================================
        // Full Card Play Flow Test
        // ============================================

        private void TestFullCardPlayFlow()
        {
            Debug.Log("========================================");
            Debug.Log("[Week4Test] Testing Full Card Play Flow");
            Debug.Log("========================================");

            if (!ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                Debug.LogError("[Week4Test] TurnManager not found - cannot test card play");
                return;
            }

            if (!ServiceLocator.TryGet<HandManager>(out var handManager))
            {
                Debug.LogError("[Week4Test] HandManager not found - cannot test card play");
                return;
            }

            if (!ServiceLocator.TryGet<DeckManager>(out var deckManager))
            {
                Debug.LogError("[Week4Test] DeckManager not found - cannot test card play");
                return;
            }

            // Check if we're in combat
            if (!turnManager.IsCombatActive)
            {
                Debug.LogWarning("[Week4Test] Combat not active. Starting test combat...");
                StartTestCombat(turnManager, deckManager, handManager);
            }

            // Ensure we're in player phase with AP
            var context = turnManager.Context;
            if (turnManager.CurrentPhase != CombatPhase.PlayerPhase)
            {
                Debug.LogWarning($"[Week4Test] Not in PlayerPhase (current: {turnManager.CurrentPhase}). Transitioning...");
                turnManager.TransitionToPhase(CombatPhase.PlayerPhase);
            }

            // Ensure AP is available
            if (context.CurrentAP <= 0)
            {
                context.CurrentAP = context.MaxAP;
                Debug.Log($"[Week4Test] Set AP to {context.CurrentAP}");
            }

            // Add test card to hand if empty
            if (handManager.CardCount == 0 && _testGuardCard != null)
            {
                // Use Guard card (TargetType.None) for simpler testing
                var testInstance = new CardInstance(_testGuardCard);
                handManager.AddCard(testInstance);
                Debug.Log($"[Week4Test] Added test card to hand: {_testGuardCard.CardName}");
            }
            else if (handManager.CardCount == 0 && _testStrikeCard != null)
            {
                var testInstance = new CardInstance(_testStrikeCard);
                handManager.AddCard(testInstance);
                Debug.Log($"[Week4Test] Added test card to hand: {_testStrikeCard.CardName}");
            }

            int initialAP = context.CurrentAP;
            int initialHandCount = handManager.CardCount;
            int initialDiscardCount = deckManager.DiscardPileCount;

            Debug.Log($"[Week4Test] Initial State: AP={initialAP}, Hand={initialHandCount}, Discard={initialDiscardCount}");

            // Try to play a card
            if (handManager.CardCount > 0)
            {
                var card = handManager.Hand[0];
                var cardName = card.CardInstance?.Data?.CardName ?? "Unknown";
                var cardCost = card.CardInstance?.CurrentCost ?? 0;
                var targetType = card.CardInstance?.Data?.TargetType ?? TargetType.None;

                Debug.Log($"[Week4Test] Attempting to play: {cardName} (Cost: {cardCost}, TargetType: {targetType})");

                // For SingleEnemy cards, we need to provide a target or skip
                if (targetType == TargetType.SingleEnemy || targetType == TargetType.SingleAlly)
                {
                    Debug.LogWarning($"[Week4Test] Card requires targeting - testing TryPlayCard directly with null target");
                    // For testing, play directly with first enemy as target
                    ICombatTarget target = context.Enemies.Count > 0 ? context.Enemies[0] : null;
                    turnManager.TryPlayCard(card, target);
                }
                else
                {
                    handManager.HandleCardPlayAttempt(card);
                }

                // Check results
                int finalAP = context.CurrentAP;
                int finalHandCount = handManager.CardCount;
                int finalDiscardCount = deckManager.DiscardPileCount;

                Debug.Log($"[Week4Test] Final State: AP={finalAP}, Hand={finalHandCount}, Discard={finalDiscardCount}");

                bool apDeducted = finalAP == initialAP - cardCost;
                bool cardRemoved = finalHandCount == initialHandCount - 1;
                bool cardDiscarded = finalDiscardCount == initialDiscardCount + 1;

                Log("AP deducted correctly", apDeducted);
                Log("Card removed from hand", cardRemoved);
                Log("Card added to discard", cardDiscarded);
            }
            else
            {
                Debug.LogWarning("[Week4Test] No cards in hand to test - assign test cards in inspector");
            }

            Debug.Log("========================================");
        }

        private void StartTestCombat(TurnManager turnManager, DeckManager deckManager, HandManager handManager)
        {
            // Create minimal test team
            var team = new List<RequiemInstance>
            {
                new RequiemInstance { MaxHP = 100, CurrentHP = 100 }
            };

            // Create test enemy
            var enemies = new List<EnemyInstance>();
            if (_testEnemy != null)
            {
                var enemyGO = new GameObject("TestEnemy");
                var enemy = enemyGO.AddComponent<EnemyInstance>();
                enemy.Initialize(_testEnemy, 1);
                enemies.Add(enemy);
            }

            // Start combat
            turnManager.StartCombat(team, enemies);

            // Manually set up for testing (skip normal phase flow)
            var context = turnManager.Context;
            context.CurrentAP = context.MaxAP;
            context.IsPlayerTurn = true;
            context.TurnNumber = 1;

            Debug.Log($"[Week4Test] Combat started with AP={context.CurrentAP}, Enemies={enemies.Count}");
        }

        // ============================================
        // Logging
        // ============================================

        private void Log(string testName, bool passed, string reason = "")
        {
            string result;

            if (passed)
            {
                result = $"<color=green>[PASS]</color> {testName}";
                _passCount++;
            }
            else
            {
                result = $"<color=red>[FAIL]</color> {testName}";
                if (!string.IsNullOrEmpty(reason))
                {
                    result += $" - {reason}";
                }
                _failCount++;
            }

            if (_verboseLogging || !passed)
            {
                Debug.Log(result);
            }

            _testResults.Add(result);
        }
    }
}
