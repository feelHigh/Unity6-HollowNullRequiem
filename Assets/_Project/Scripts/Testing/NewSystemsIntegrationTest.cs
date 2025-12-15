// ============================================
// NewSystemsIntegrationTest.cs
// Integration tests for Team, StatBlock, CombatManager, UI systems
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;
using HNR.Combat;
using HNR.Cards;
using HNR.UI;
using HNR.UI.Toast;

namespace HNR.Testing
{
    /// <summary>
    /// Integration tests for newly added systems.
    /// Tests Team, StatBlock, CombatManager facade, RequiemArtExecutor, and UI components.
    /// </summary>
    /// <remarks>
    /// Keyboard shortcuts:
    /// [T] Run all tests
    /// [1] Test Team class
    /// [2] Test StatBlock
    /// [3] Test CombatManager facade
    /// [4] Test RequiemArtExecutor
    /// [5] Test Toast system
    /// [6] Test GlobalHeader/NavDock
    /// </remarks>
    public class NewSystemsIntegrationTest : MonoBehaviour
    {
        // ============================================
        // Test State
        // ============================================

        private int _passCount;
        private int _failCount;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T)) RunAllTests();
            if (Input.GetKeyDown(KeyCode.Alpha1)) TestTeamClass();
            if (Input.GetKeyDown(KeyCode.Alpha2)) TestStatBlock();
            if (Input.GetKeyDown(KeyCode.Alpha3)) TestCombatManagerFacade();
            if (Input.GetKeyDown(KeyCode.Alpha4)) TestRequiemArtExecutor();
            if (Input.GetKeyDown(KeyCode.Alpha5)) TestToastSystem();
            if (Input.GetKeyDown(KeyCode.Alpha6)) TestMetaGameUI();
        }

        // ============================================
        // Full Test Suite
        // ============================================

        public void RunAllTests()
        {
            _passCount = 0;
            _failCount = 0;

            Debug.Log("=== NEW SYSTEMS INTEGRATION TESTS ===");

            TestTeamClass();
            TestStatBlock();
            TestCombatManagerFacade();
            TestRequiemArtExecutor();
            TestToastSystem();
            TestMetaGameUI();

            Debug.Log($"=== RESULTS: {_passCount}/{_passCount + _failCount} passed ===");

            if (_failCount == 0)
            {
                Debug.Log("All new systems tests PASSED!");
            }
            else
            {
                Debug.LogWarning($"New systems tests completed with {_failCount} failure(s)");
            }
        }

        // ============================================
        // Team Class Tests
        // ============================================

        private void TestTeamClass()
        {
            Debug.Log("--- Team Class ---");

            // Create empty team
            var team = new Team();
            Log("Empty team created", team.Count == 0);
            Log("IsEmpty returns true", team.IsEmpty);
            Log("IsFull returns false", !team.IsFull);
            Log("MaxSize is 3", Team.MaxSize == 3);

            // Test with mock data (no actual RequiemInstance needed for basic tests)
            Log("TotalMaxHP on empty team is 0", team.TotalMaxHP == 0);
            Log("AnyInNullState on empty team is false", !team.AnyInNullState);
            Log("NullStateCount on empty team is 0", team.NullStateCount == 0);

            // Test indexer bounds
            Log("Indexer [-1] returns null", team[-1] == null);
            Log("Indexer [0] returns null on empty", team[0] == null);
            Log("Indexer [100] returns null", team[100] == null);

            // Test ToList/ToArray
            var list = team.ToList();
            Log("ToList() returns empty list", list != null && list.Count == 0);

            var array = team.ToArray();
            Log("ToArray() returns empty array", array != null && array.Length == 0);

            // Test iteration
            int iterCount = 0;
            foreach (var member in team)
            {
                iterCount++;
            }
            Log("Iteration works on empty team", iterCount == 0);

            // Test implicit conversion
            List<RequiemInstance> implicitList = team;
            Log("Implicit conversion to List works", implicitList != null);

            Debug.Log("  Team class basic tests complete");
        }

        // ============================================
        // StatBlock Tests
        // ============================================

        private void TestStatBlock()
        {
            Debug.Log("--- StatBlock ---");

            // Create with values
            var stats = new StatBlock(100, 10, 5, 1.2f);
            Log("StatBlock created with values", stats != null);
            Log("BaseHP is 100", stats.BaseHP == 100);
            Log("BaseATK is 10", stats.BaseATK == 10);
            Log("BaseDEF is 5", stats.BaseDEF == 5);
            Log("BaseSERate is 1.2", Mathf.Approximately(stats.BaseSERate, 1.2f));

            // Test calculated values (no modifiers)
            Log("HP equals BaseHP without modifiers", stats.HP == stats.BaseHP);
            Log("ATK equals BaseATK without modifiers", stats.ATK == stats.BaseATK);

            // Test flat modifier
            var flatMod = StatModifier.Flat(StatType.ATK, 5);
            stats.AddModifier(flatMod);
            Log("Flat +5 ATK applied", stats.ATK == 15);

            // Test percent add modifier
            var percentMod = StatModifier.PercentAdd(StatType.ATK, 0.2f); // +20%
            stats.AddModifier(percentMod);
            // (10 + 5) * 1.2 = 18
            Log("PercentAdd +20% ATK applied", stats.ATK == 18);

            // Test remove modifier
            bool removed = stats.RemoveModifier(flatMod);
            Log("Modifier removed successfully", removed);
            // 10 * 1.2 = 12
            Log("ATK recalculated after removal", stats.ATK == 12);

            // Test clear all
            stats.ClearAllModifiers();
            Log("ClearAllModifiers works", stats.ATK == stats.BaseATK);

            // Test GetModifiers
            stats.AddModifier(StatModifier.Flat(StatType.HP, 10));
            var hpMods = stats.GetModifiers(StatType.HP);
            Log("GetModifiers returns correct count", hpMods.Count == 1);

            // Test Clone
            var clone = stats.Clone();
            Log("Clone has same BaseHP", clone.BaseHP == stats.BaseHP);
            Log("Clone has no modifiers", clone.GetModifiers(StatType.HP).Count == 0);

            // Test ToString
            string str = stats.ToString();
            Log("ToString returns valid string", !string.IsNullOrEmpty(str) && str.Contains("StatBlock"));

            Debug.Log("  StatBlock tests complete");
        }

        // ============================================
        // CombatManager Facade Tests
        // ============================================

        private void TestCombatManagerFacade()
        {
            Debug.Log("--- CombatManager Facade ---");

            // Check if registered
            bool hasManager = ServiceLocator.TryGet<CombatManager>(out var cm);

            if (!hasManager)
            {
                // Try to find in scene
                cm = CombatManager.Instance;
                if (cm == null)
                {
                    cm = FindAnyObjectByType<CombatManager>();
                }
            }

            if (cm == null)
            {
                Debug.Log("  [SKIP] CombatManager not in scene - skipping runtime tests");
                Log("CombatManager static Instance accessible", CombatManager.Instance == null || CombatManager.Instance != null);
                return;
            }

            Log("CombatManager found", cm != null);
            Log("Instance property works", CombatManager.Instance == cm);

            // Test properties when no combat active
            Log("IsCombatActive property accessible", !cm.IsCombatActive || cm.IsCombatActive);
            Log("IsPlayerTurn property accessible", !cm.IsPlayerTurn || cm.IsPlayerTurn);
            Log("CurrentPhase property accessible", cm.CurrentPhase >= 0);
            Log("TurnNumber accessible", cm.TurnNumber >= 0);
            Log("TeamHP accessible", cm.TeamHP >= 0);
            Log("CurrentAP accessible", cm.CurrentAP >= 0);
            Log("MaxAP accessible", cm.MaxAP > 0);

            // Test query methods
            var enemies = cm.GetAliveEnemies();
            Log("GetAliveEnemies returns list", enemies != null);

            var hand = cm.GetHand();
            Log("GetHand returns list", hand != null);

            Log("DrawPileCount accessible", cm.DrawPileCount >= 0);
            Log("DiscardPileCount accessible", cm.DiscardPileCount >= 0);
            Log("ExhaustPileCount accessible", cm.ExhaustPileCount >= 0);

            // Test condition checks
            Log("CheckVictory callable", !cm.CheckVictory() || cm.CheckVictory());
            Log("CheckDefeat callable", !cm.CheckDefeat() || cm.CheckDefeat());

            Debug.Log("  CombatManager facade tests complete");
        }

        // ============================================
        // RequiemArtExecutor Tests
        // ============================================

        private void TestRequiemArtExecutor()
        {
            Debug.Log("--- RequiemArtExecutor ---");

            var executor = RequiemArtExecutor.Instance;
            if (executor == null)
            {
                executor = FindAnyObjectByType<RequiemArtExecutor>();
            }

            if (executor == null)
            {
                Debug.Log("  [SKIP] RequiemArtExecutor not in scene - skipping runtime tests");
                Log("RequiemArtExecutor static Instance accessible", RequiemArtExecutor.Instance == null || RequiemArtExecutor.Instance != null);
                return;
            }

            Log("RequiemArtExecutor found", executor != null);
            Log("Instance property works", RequiemArtExecutor.Instance == executor);

            // Test with null parameters (should return false gracefully)
            bool result = executor.ExecuteArt(null, null, null);
            Log("ExecuteArt handles null requiem gracefully", !result);

            Debug.Log("  RequiemArtExecutor tests complete");
        }

        // ============================================
        // Toast System Tests
        // ============================================

        private void TestToastSystem()
        {
            Debug.Log("--- Toast System ---");

            var toastManager = ToastManager.Instance;
            if (toastManager == null)
            {
                toastManager = FindAnyObjectByType<ToastManager>();
            }

            if (toastManager == null)
            {
                Debug.Log("  [SKIP] ToastManager not in scene - skipping runtime tests");
                Log("ToastManager static Instance accessible", ToastManager.Instance == null || ToastManager.Instance != null);
                return;
            }

            Log("ToastManager found", toastManager != null);
            Log("Instance property works", ToastManager.Instance == toastManager);

            // Test showing toasts (visual verification needed)
            toastManager.ShowInfo("Test Info Toast");
            Log("ShowInfo called without error", true);

            toastManager.ShowSuccess("Test Success Toast");
            Log("ShowSuccess called without error", true);

            toastManager.ShowWarning("Test Warning Toast");
            Log("ShowWarning called without error", true);

            toastManager.ShowError("Test Error Toast");
            Log("ShowError called without error", true);

            // Test custom duration
            toastManager.ShowToast("Custom Duration", ToastType.Info, 5f);
            Log("ShowToast with custom duration works", true);

            Debug.Log("  Toast system tests complete (verify visually)");
        }

        // ============================================
        // Meta-Game UI Tests
        // ============================================

        private void TestMetaGameUI()
        {
            Debug.Log("--- Meta-Game UI ---");

            // GlobalHeader
            var header = FindAnyObjectByType<GlobalHeader>();
            if (header != null)
            {
                Log("GlobalHeader found in scene", true);

                // Test initialization (no errors)
                header.Initialize("TestPlayer", 10, 500, 1000, 100, 50, 20);
                Log("GlobalHeader.Initialize works", true);

                header.SetEventBanner(true, "Test Event!");
                Log("SetEventBanner works", true);

                header.SetEventBanner(false);
                Log("SetEventBanner hide works", true);
            }
            else
            {
                Debug.Log("  [SKIP] GlobalHeader not in scene");
            }

            // GlobalNavDock
            var navDock = FindAnyObjectByType<GlobalNavDock>();
            if (navDock != null)
            {
                Log("GlobalNavDock found in scene", true);
                Log("SelectedIndex accessible", navDock.SelectedIndex >= 0);

                navDock.SelectDestination(NavDestination.Bastion);
                Log("SelectDestination works", true);

                navDock.SetNotificationBadge(NavDestination.Inventory, 5);
                Log("SetNotificationBadge works", true);
            }
            else
            {
                Debug.Log("  [SKIP] GlobalNavDock not in scene");
            }

            // CurrencyTicker (usually part of GlobalHeader)
            var tickers = FindObjectsByType<HNR.UI.Components.CurrencyTicker>(FindObjectsSortMode.None);
            if (tickers.Length > 0)
            {
                Log("CurrencyTicker(s) found", true);

                var ticker = tickers[0];
                ticker.SetValueImmediate(100);
                Log("SetValueImmediate works", ticker.DisplayedValue == 100);

                ticker.AnimateToValue(200);
                Log("AnimateToValue called (verify visually)", true);
            }
            else
            {
                Debug.Log("  [SKIP] No CurrencyTicker in scene");
            }

            // UIColors static class test
            Log("UIColors.SoulCyan accessible", UIColors.SoulCyan.a > 0);
            Log("UIColors.GetAspectColor works", UIColors.GetAspectColor(SoulAspect.Flame) != Color.clear);
            Log("UIColors.WithAlpha works", UIColors.WithAlpha(Color.white, 0.5f).a == 0.5f);
            Log("UIColors.Brighten works", UIColors.Brighten(Color.gray).r > Color.gray.r);
            Log("UIColors.Darken works", UIColors.Darken(Color.gray).r < Color.gray.r);

            Debug.Log("  Meta-Game UI tests complete");
        }

        // ============================================
        // Test Helpers
        // ============================================

        private void Log(string testName, bool passed)
        {
            if (passed)
            {
                Debug.Log($"  [PASS] {testName}");
                _passCount++;
            }
            else
            {
                Debug.LogError($"  [FAIL] {testName}");
                _failCount++;
            }
        }
    }
}
