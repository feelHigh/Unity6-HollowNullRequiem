// ============================================
// Week8IntegrationTest.cs
// Integration tests for Shop and Save systems
// ============================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Progression;
using HNR.Characters;

namespace HNR.Testing
{
    /// <summary>
    /// Integration tests for Week 8: Shop and Save Systems.
    /// Press T to run all tests.
    /// </summary>
    public class Week8IntegrationTest : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Test Configuration")]
        [SerializeField, Tooltip("Shop config for testing")]
        private ShopConfigSO _testShopConfig;

        [SerializeField, Tooltip("Test Requiem for run tests")]
        private RequiemDataSO _testRequiem;

        // ============================================
        // Test State
        // ============================================

        private int _passCount;
        private int _failCount;
        private List<string> _testResults = new();

        // Cached initial states for cleanup
        private int _initialVoidShards;
        private bool _hadSavedRun;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunAllTests();
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
            Debug.Log("WEEK 8 INTEGRATION TESTS - Shop & Save");
            Debug.Log("========================================");

            // Cache initial state for cleanup
            CacheInitialState();

            // Shop Manager Tests
            TestShopManagerExists();
            TestVoidShardsCurrency();
            TestShopGeneration();
            TestShopPurchase();

            // Relic Manager Tests
            TestRelicManagerExists();
            TestRelicAcquisition();
            TestRelicTriggers();

            // Save Manager Tests
            TestSaveManagerExists();
            TestRunSaveLoad();
            TestSettingsSaveLoad();
            TestMetaSaveLoad();

            // Run Manager Tests
            TestRunManagerExists();
            TestRunInitialization();

            // Cleanup
            CleanupTestData();

            // Summary
            Debug.Log("========================================");
            Debug.Log($"RESULTS: {_passCount}/{_passCount + _failCount} tests passed");
            if (_failCount > 0)
                Debug.LogWarning($"FAILURES: {_failCount} tests failed");
            else
                Debug.Log("ALL TESTS PASSED!");
            Debug.Log("========================================");
        }

        // ============================================
        // State Management
        // ============================================

        private void CacheInitialState()
        {
            var shopManager = ServiceLocator.Get<IShopManager>();
            _initialVoidShards = shopManager?.VoidShards ?? 0;

            var saveManager = ServiceLocator.Get<ISaveManager>();
            _hadSavedRun = saveManager?.HasSavedRun ?? false;
        }

        private void CleanupTestData()
        {
            Debug.Log("[Week8Test] Cleaning up test data...");

            // Restore void shards
            var shopManager = ServiceLocator.Get<IShopManager>();
            if (shopManager != null)
            {
                shopManager.SetVoidShards(_initialVoidShards);
                shopManager.CloseShop();
            }

            // Delete test save if we didn't have one before
            var saveManager = ServiceLocator.Get<ISaveManager>();
            if (saveManager != null && !_hadSavedRun && saveManager.HasSavedRun)
            {
                saveManager.DeleteRun();
            }

            // Clear test relics
            var relicManager = ServiceLocator.Get<IRelicManager>();
            relicManager?.ClearRelics();

            Debug.Log("[Week8Test] Cleanup complete");
        }

        // ============================================
        // Shop Manager Tests
        // ============================================

        private void TestShopManagerExists()
        {
            var shopManager = ServiceLocator.Get<IShopManager>();
            Log("ShopManager - Registered", shopManager != null);
        }

        private void TestVoidShardsCurrency()
        {
            var shopManager = ServiceLocator.Get<IShopManager>();
            if (shopManager == null)
            {
                Log("Currency - ShopManager Available", false, "Not found");
                return;
            }

            // Reset to known state
            shopManager.SetVoidShards(0);

            // Test adding shards
            shopManager.AddVoidShards(100);
            Log("Currency - AddVoidShards", shopManager.VoidShards == 100, $"Expected 100, got {shopManager.VoidShards}");

            // Test spending shards (success)
            bool spentSuccess = shopManager.SpendVoidShards(50);
            Log("Currency - SpendVoidShards Success", spentSuccess && shopManager.VoidShards == 50,
                $"Expected 50, got {shopManager.VoidShards}");

            // Test spending shards (insufficient)
            bool spentFail = shopManager.SpendVoidShards(100);
            Log("Currency - SpendVoidShards Insufficient", !spentFail && shopManager.VoidShards == 50,
                "Should fail with insufficient shards");

            // Test event publishing
            bool eventReceived = false;
            void OnShardsChanged(VoidShardsChangedEvent e) => eventReceived = true;

            EventBus.Subscribe<VoidShardsChangedEvent>(OnShardsChanged);
            shopManager.AddVoidShards(10);
            EventBus.Unsubscribe<VoidShardsChangedEvent>(OnShardsChanged);

            Log("Currency - VoidShardsChangedEvent Published", eventReceived);
        }

        private void TestShopGeneration()
        {
            var shopManager = ServiceLocator.Get<IShopManager>();
            if (shopManager == null) return;

            // Test shop opening
            shopManager.OpenShop(1);
            Log("Shop - Opens Successfully", shopManager.CurrentInventory != null);

            if (shopManager.CurrentInventory != null)
            {
                Log("Shop - Has Items", shopManager.CurrentInventory.ItemCount > 0,
                    $"Count: {shopManager.CurrentInventory.ItemCount}");

                // Check for variety of item types
                bool hasCards = shopManager.CurrentInventory.GetItemsByType(ShopItemType.Card).Count > 0;
                Log("Shop - Contains Cards", hasCards);
            }

            // Test shop closing
            shopManager.CloseShop();
            Log("Shop - Closes Successfully", shopManager.CurrentInventory == null);

            // Test event publishing
            bool openEventReceived = false;
            void OnShopOpened(ShopOpenedEvent e) => openEventReceived = true;

            EventBus.Subscribe<ShopOpenedEvent>(OnShopOpened);
            shopManager.OpenShop(1);
            EventBus.Unsubscribe<ShopOpenedEvent>(OnShopOpened);

            Log("Shop - ShopOpenedEvent Published", openEventReceived);
        }

        private void TestShopPurchase()
        {
            var shopManager = ServiceLocator.Get<IShopManager>();
            if (shopManager == null) return;

            // Setup
            shopManager.SetVoidShards(1000);
            shopManager.OpenShop(1);

            if (shopManager.CurrentInventory == null || shopManager.CurrentInventory.ItemCount == 0)
            {
                Log("Purchase - Inventory Available", false, "No items in shop");
                return;
            }

            var testItem = shopManager.CurrentInventory.Items[0];
            int priceBeforePurchase = testItem.Price;
            int shardsBeforePurchase = shopManager.VoidShards;

            // Test successful purchase
            bool purchased = shopManager.PurchaseItem(testItem);
            Log("Purchase - Item Purchased", purchased);
            Log("Purchase - Item Marked Sold", testItem.IsPurchased);
            Log("Purchase - Shards Deducted", shopManager.VoidShards == shardsBeforePurchase - priceBeforePurchase,
                $"Expected {shardsBeforePurchase - priceBeforePurchase}, got {shopManager.VoidShards}");

            // Test double purchase (should fail)
            bool doublePurchase = shopManager.PurchaseItem(testItem);
            Log("Purchase - Cannot Buy Twice", !doublePurchase);

            // Test purchase event
            bool purchaseEventReceived = false;
            void OnPurchased(ShopItemPurchasedEvent e) => purchaseEventReceived = true;

            if (shopManager.CurrentInventory.AvailableCount > 0)
            {
                var nextItem = shopManager.CurrentInventory.GetAvailableItems()[0];
                EventBus.Subscribe<ShopItemPurchasedEvent>(OnPurchased);
                shopManager.PurchaseItem(nextItem);
                EventBus.Unsubscribe<ShopItemPurchasedEvent>(OnPurchased);

                Log("Purchase - ShopItemPurchasedEvent Published", purchaseEventReceived);
            }
        }

        // ============================================
        // Relic Manager Tests
        // ============================================

        private void TestRelicManagerExists()
        {
            var relicManager = ServiceLocator.Get<IRelicManager>();
            Log("RelicManager - Registered", relicManager != null);
        }

        private void TestRelicAcquisition()
        {
            var relicManager = ServiceLocator.Get<IRelicManager>();
            if (relicManager == null)
            {
                Log("Relics - Manager Available", false, "Not found");
                return;
            }

            // Clear existing relics
            relicManager.ClearRelics();

            // Try to load a test relic
            var testRelic = Resources.Load<RelicDataSO>("Data/Relics/Relic_VoidHeart");
            if (testRelic == null)
            {
                Log("Relics - Test Relic Exists", false, "Relic_VoidHeart not found in Resources");
                return;
            }

            // Test adding relic
            relicManager.AddRelic(testRelic);
            Log("Relics - AddRelic", relicManager.HasRelic(testRelic.RelicId),
                $"Relic: {testRelic.RelicName}");

            // Test owned relics list
            Log("Relics - OwnedRelics Contains", relicManager.OwnedRelics.Contains(testRelic));

            // Test duplicate prevention
            int countBefore = relicManager.OwnedRelics.Count;
            relicManager.AddRelic(testRelic);
            Log("Relics - No Duplicates", relicManager.OwnedRelics.Count == countBefore);

            // Test event publishing
            bool eventReceived = false;
            void OnRelicAcquired(RelicAcquiredEvent e) => eventReceived = true;

            var anotherRelic = Resources.Load<RelicDataSO>("Data/Relics/Relic_SoulAnchor");
            if (anotherRelic != null)
            {
                EventBus.Subscribe<RelicAcquiredEvent>(OnRelicAcquired);
                relicManager.AddRelic(anotherRelic);
                EventBus.Unsubscribe<RelicAcquiredEvent>(OnRelicAcquired);

                Log("Relics - RelicAcquiredEvent Published", eventReceived);
            }
        }

        private void TestRelicTriggers()
        {
            var relicManager = ServiceLocator.Get<IRelicManager>();
            if (relicManager == null) return;

            // Test trigger processing (relics should respond to events)
            // This is a basic check that TriggerRelics doesn't throw
            bool triggeredWithoutError = true;
            try
            {
                relicManager.TriggerRelics(RelicTrigger.OnCombatStart);
            }
            catch
            {
                triggeredWithoutError = false;
            }

            Log("Relics - TriggerRelics No Errors", triggeredWithoutError);
        }

        // ============================================
        // Save Manager Tests
        // ============================================

        private void TestSaveManagerExists()
        {
            var saveManager = ServiceLocator.Get<ISaveManager>();
            Log("SaveManager - Registered", saveManager != null);
        }

        private void TestRunSaveLoad()
        {
            var saveManager = ServiceLocator.Get<ISaveManager>();
            if (saveManager == null)
            {
                Log("Save - Manager Available", false, "Not found");
                return;
            }

            // Create test data
            var testData = new RunSaveData
            {
                RunSeed = 12345,
                Progression = new ProgressionSaveData { CurrentZone = 2, VoidShards = 500 },
                Stats = new StatsSaveData { EnemiesDefeated = 10, CardsPlayed = 50 }
            };

            // Test save
            saveManager.SaveRun(testData);
            Log("Save - SaveRun Creates File", saveManager.HasSavedRun);

            // Test load
            var loadedData = saveManager.LoadRun();
            Log("Save - LoadRun Returns Data", loadedData != null);

            if (loadedData != null)
            {
                Log("Save - RunSeed Preserved", loadedData.RunSeed == 12345,
                    $"Expected 12345, got {loadedData.RunSeed}");
                Log("Save - Progression Preserved", loadedData.Progression.CurrentZone == 2,
                    $"Expected zone 2, got {loadedData.Progression.CurrentZone}");
                Log("Save - Stats Preserved", loadedData.Stats.EnemiesDefeated == 10,
                    $"Expected 10 enemies, got {loadedData.Stats.EnemiesDefeated}");
            }

            // Test delete
            saveManager.DeleteRun();
            Log("Save - DeleteRun Removes File", !saveManager.HasSavedRun);
        }

        private void TestSettingsSaveLoad()
        {
            var saveManager = ServiceLocator.Get<ISaveManager>();
            if (saveManager == null) return;

            // Create test settings
            var testSettings = new SettingsData
            {
                MusicVolume = 0.5f,
                SFXVolume = 0.75f,
                ScreenShakeEnabled = false
            };

            // Save and load
            saveManager.SaveSettings(testSettings);
            var loadedSettings = saveManager.LoadSettings();

            Log("Settings - Load Returns Data", loadedSettings != null);
            if (loadedSettings != null)
            {
                Log("Settings - MusicVolume Preserved",
                    Mathf.Approximately(loadedSettings.MusicVolume, 0.5f),
                    $"Expected 0.5, got {loadedSettings.MusicVolume}");
            }
        }

        private void TestMetaSaveLoad()
        {
            var saveManager = ServiceLocator.Get<ISaveManager>();
            if (saveManager == null) return;

            // Load current meta (or defaults)
            var meta = saveManager.LoadMeta();
            Log("Meta - Load Returns Data", meta != null);

            if (meta != null)
            {
                // Modify and save
                int originalRuns = meta.TotalRunsStarted;
                meta.TotalRunsStarted += 1;
                saveManager.SaveMeta(meta);

                // Reload and verify
                var reloadedMeta = saveManager.LoadMeta();
                Log("Meta - Changes Persisted",
                    reloadedMeta.TotalRunsStarted == originalRuns + 1,
                    $"Expected {originalRuns + 1}, got {reloadedMeta.TotalRunsStarted}");

                // Restore original
                meta.TotalRunsStarted = originalRuns;
                saveManager.SaveMeta(meta);
            }
        }

        // ============================================
        // Run Manager Tests
        // ============================================

        private void TestRunManagerExists()
        {
            var runManager = ServiceLocator.Get<IRunManager>();
            Log("RunManager - Registered", runManager != null);
        }

        private void TestRunInitialization()
        {
            var runManager = ServiceLocator.Get<IRunManager>();
            if (runManager == null)
            {
                Log("Run - Manager Available", false, "Not found");
                return;
            }

            // Check initial state
            Log("Run - Initially Inactive", !runManager.IsRunActive);

            // Test with a mock team if we have test data
            if (_testRequiem != null)
            {
                var testTeam = new List<RequiemDataSO> { _testRequiem };
                runManager.InitializeNewRun(testTeam);

                Log("Run - IsRunActive After Init", runManager.IsRunActive);
                Log("Run - Team Has Members", runManager.Team.Count > 0,
                    $"Team size: {runManager.Team.Count}");
                Log("Run - Has Starting Deck", runManager.Deck.Count > 0,
                    $"Deck size: {runManager.Deck.Count}");
                Log("Run - Has Team HP", runManager.TeamMaxHP > 0,
                    $"Max HP: {runManager.TeamMaxHP}");

                // Clean up - end the test run
                runManager.EndRun(false);
                Log("Run - EndRun Deactivates", !runManager.IsRunActive);
            }
        }

        // ============================================
        // Utility
        // ============================================

        private void Log(string testName, bool passed, string info = "")
        {
            string status = passed ? "PASS" : "FAIL";
            string message = string.IsNullOrEmpty(info) ? testName : $"{testName} ({info})";

            if (passed)
            {
                Debug.Log($"[{status}] {message}");
                _passCount++;
            }
            else
            {
                Debug.LogError($"[{status}] {message}");
                _failCount++;
            }

            _testResults.Add($"[{status}] {message}");
        }
    }
}
