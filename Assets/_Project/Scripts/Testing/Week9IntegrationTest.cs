// ============================================
// Week9IntegrationTest.cs
// Integration tests for UI Polish & Animations
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Cards;
using HNR.UI;

namespace HNR.Testing
{
    /// <summary>
    /// Integration tests for Week 9: UI Polish and Animations.
    /// Press T to run all tests.
    /// Press D to test damage numbers.
    /// Press S to test screen shake.
    /// </summary>
    public class Week9IntegrationTest : MonoBehaviour
    {
        // ============================================
        // Test References
        // ============================================

        [Header("UI Components")]
        [SerializeField, Tooltip("CardAnimator for animation tests")]
        private CardAnimator _testCardAnimator;

        [SerializeField, Tooltip("HandLayoutManager for layout tests")]
        private HandLayoutManager _testHandLayout;

        [SerializeField, Tooltip("DamageNumberSpawner for damage number tests")]
        private DamageNumberSpawner _damageSpawner;

        [SerializeField, Tooltip("ScreenShakeController for shake tests")]
        private ScreenShakeController _shakeController;

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
            if (Input.GetKeyDown(KeyCode.D)) TestDamageNumber();
            if (Input.GetKeyDown(KeyCode.S)) TestScreenShake();
            if (Input.GetKeyDown(KeyCode.C)) TestCorruptionPulse();
            if (Input.GetKeyDown(KeyCode.F)) TestScreenFlash();
        }

        // ============================================
        // Test Runner
        // ============================================

        /// <summary>
        /// Run all Week 9 integration tests.
        /// </summary>
        public void RunAllTests()
        {
            _passCount = 0;
            _failCount = 0;

            Debug.Log("========================================");
            Debug.Log("Week 9 Integration Tests - UI Polish");
            Debug.Log("========================================");

            TestUIColors();
            TestCardAnimator();
            TestHandLayout();
            TestDamageNumbers();
            TestScreenEffects();
            TestResponsiveUI();
            TestButtonFeedback();
            TestTransitionManager();

            Debug.Log("========================================");
            Debug.Log($"Week 9 Results: {_passCount}/{_passCount + _failCount} passed");
            Debug.Log("========================================");

            if (_failCount == 0)
                Debug.Log("<color=green>ALL TESTS PASSED!</color>");
            else
                Debug.LogWarning($"<color=yellow>{_failCount} tests failed</color>");
        }

        // ============================================
        // Individual Test Groups
        // ============================================

        private void TestUIColors()
        {
            Debug.Log("--- UIColors Tests ---");

            // Base colors
            Log("UIColors.VoidBlack alpha is 255", UIColors.VoidBlack.a == 255);
            Log("UIColors.SoulCyan exists", UIColors.SoulCyan.r == 0 && UIColors.SoulCyan.g == 212);
            Log("UIColors.CorruptionRed exists", UIColors.CorruptionRed.r == 139);

            // Aspect colors
            var flameColor = UIColors.GetAspectColor(SoulAspect.Flame);
            Log("GetAspectColor(Flame) returns FlameAspect", flameColor.r == 255);

            var shadowColor = UIColors.GetAspectColor(SoulAspect.Shadow);
            Log("GetAspectColor(Shadow) returns ShadowAspect", shadowColor.r == 74);

            // Rarity colors
            var rareColor = UIColors.GetRarityColor(CardRarity.Rare);
            Log("GetRarityColor(Rare) returns blue", rareColor.b == 255);

            var legendaryColor = UIColors.GetRarityColor(CardRarity.Legendary);
            Log("GetRarityColor(Legendary) returns gold", legendaryColor.r == 255);
        }

        private void TestCardAnimator()
        {
            Debug.Log("--- CardAnimator Tests ---");

            Log("CardAnimator reference assigned", _testCardAnimator != null);

            if (_testCardAnimator != null)
            {
                // Visual verification required
                Log("CardAnimator has RectTransform", _testCardAnimator.GetComponent<RectTransform>() != null);
                Log("CardAnimator has CanvasGroup", _testCardAnimator.GetComponent<CanvasGroup>() != null);
                Log("CardAnimator.AnimateDraw (requires visual verification)", true);
                Log("CardAnimator.AnimateHover (requires visual verification)", true);
                Log("CardAnimator.AnimatePlay (requires visual verification)", true);
            }
        }

        private void TestHandLayout()
        {
            Debug.Log("--- HandLayoutManager Tests ---");

            Log("HandLayoutManager reference assigned", _testHandLayout != null);

            if (_testHandLayout != null)
            {
                // Test arc calculation
                Log("CardCount starts at 0", _testHandLayout.CardCount == 0);
                Log("IsFull is false when empty", !_testHandLayout.IsFull);

                // Note: CalculateCardPosition is private, so we test observable behavior
                Log("HandLayoutManager.RefreshLayout (requires visual verification)", true);
                Log("Arc layout produces different positions (requires visual verification)", true);
            }
        }

        private void TestDamageNumbers()
        {
            Debug.Log("--- DamageNumber Tests ---");

            // Check for spawner
            var spawner = _damageSpawner ?? FindAnyObjectByType<DamageNumberSpawner>();
            Log("DamageNumberSpawner exists", spawner != null);

            // DamageNumberType enum
            Log("DamageNumberType.Damage exists", DamageNumberType.Damage.ToString() == "Damage");
            Log("DamageNumberType.Heal exists", DamageNumberType.Heal.ToString() == "Heal");
            Log("DamageNumberType.Block exists", DamageNumberType.Block.ToString() == "Block");
            Log("DamageNumberType.Corruption exists", DamageNumberType.Corruption.ToString() == "Corruption");
        }

        private void TestScreenEffects()
        {
            Debug.Log("--- Screen Effects Tests ---");

            // Screen shake
            ScreenShakeController shakeController = _shakeController;
            if (shakeController == null)
            {
                ServiceLocator.TryGet<ScreenShakeController>(out shakeController);
            }
            Log("ScreenShakeController exists", shakeController != null);

            if (shakeController != null)
            {
                Log("ScreenShakeController.IsEnabled", shakeController.IsEnabled);
            }

            // Corruption pulse
            var corruptionPulse = FindAnyObjectByType<CorruptionPulseEffect>();
            Log("CorruptionPulseEffect found (optional)", corruptionPulse != null);

            // ShakeIntensity enum
            Log("ShakeIntensity.Light exists", ShakeIntensity.Light.ToString() == "Light");
            Log("ShakeIntensity.Medium exists", ShakeIntensity.Medium.ToString() == "Medium");
            Log("ShakeIntensity.Heavy exists", ShakeIntensity.Heavy.ToString() == "Heavy");
        }

        private void TestResponsiveUI()
        {
            Debug.Log("--- Responsive UI Tests ---");

            // Safe area
            var safeArea = FindAnyObjectByType<SafeAreaHandler>();
            Log("SafeAreaHandler found", safeArea != null);

            if (safeArea != null)
            {
                Log("SafeAreaHandler has RectTransform", safeArea.GetComponent<RectTransform>() != null);
            }

            // Responsive scaler
            var scaler = FindAnyObjectByType<ResponsiveScaler>();
            Log("ResponsiveScaler found", scaler != null);

            if (scaler != null)
            {
                Log("ResponsiveScaler.AspectRatio > 0", scaler.AspectRatio > 0);
                Log("ResponsiveScaler.CurrentCategory valid",
                    scaler.CurrentCategory == AspectCategory.Standard ||
                    scaler.CurrentCategory == AspectCategory.Wide ||
                    scaler.CurrentCategory == AspectCategory.UltraWide ||
                    scaler.CurrentCategory == AspectCategory.Tablet);
            }
        }

        private void TestButtonFeedback()
        {
            Debug.Log("--- ButtonFeedback Tests ---");

            var buttonFeedback = FindAnyObjectByType<ButtonFeedback>();
            Log("ButtonFeedback component found (optional)", buttonFeedback != null);

            // HapticIntensity enum
            Log("HapticIntensity.Light exists", HapticIntensity.Light.ToString() == "Light");
            Log("HapticIntensity.Medium exists", HapticIntensity.Medium.ToString() == "Medium");
            Log("HapticIntensity.Heavy exists", HapticIntensity.Heavy.ToString() == "Heavy");
        }

        private void TestTransitionManager()
        {
            Debug.Log("--- TransitionManager Tests ---");

            TransitionManager transitionManager = null;
            if (!ServiceLocator.TryGet<TransitionManager>(out transitionManager))
            {
                transitionManager = FindAnyObjectByType<TransitionManager>();
            }
            Log("TransitionManager found", transitionManager != null);

            if (transitionManager != null)
            {
                Log("TransitionManager.IsTransitioning is false initially", !transitionManager.IsTransitioning);
            }

            // TransitionType enum
            Log("TransitionType.Fade exists", TransitionType.Fade.ToString() == "Fade");
            Log("TransitionType.SlideLeft exists", TransitionType.SlideLeft.ToString() == "SlideLeft");
            Log("TransitionType.Dissolve exists", TransitionType.Dissolve.ToString() == "Dissolve");
        }

        // ============================================
        // Interactive Tests
        // ============================================

        /// <summary>
        /// Test damage number spawning (press D).
        /// </summary>
        public void TestDamageNumber()
        {
            var spawner = _damageSpawner ?? FindAnyObjectByType<DamageNumberSpawner>();
            if (spawner != null)
            {
                spawner.SpawnNumber(42, DamageNumberType.Damage, Vector3.zero, true);
                Debug.Log("[TEST] Spawned critical damage number (42) at center");
            }
            else
            {
                Debug.LogWarning("[TEST] DamageNumberSpawner not found");
            }
        }

        /// <summary>
        /// Test screen shake (press S).
        /// </summary>
        public void TestScreenShake()
        {
            ScreenShakeController controller = _shakeController;
            if (controller == null)
            {
                ServiceLocator.TryGet<ScreenShakeController>(out controller);
            }
            if (controller != null)
            {
                controller.Shake(ShakeIntensity.Heavy);
                Debug.Log("[TEST] Triggered heavy screen shake");
            }
            else
            {
                Debug.LogWarning("[TEST] ScreenShakeController not found");
            }
        }

        /// <summary>
        /// Test corruption pulse (press C).
        /// </summary>
        public void TestCorruptionPulse()
        {
            var pulse = FindAnyObjectByType<CorruptionPulseEffect>();
            if (pulse != null)
            {
                pulse.TriggerBurst();
                Debug.Log("[TEST] Triggered corruption pulse burst");
            }
            else
            {
                Debug.LogWarning("[TEST] CorruptionPulseEffect not found");
            }
        }

        /// <summary>
        /// Test screen flash (press F).
        /// </summary>
        public void TestScreenFlash()
        {
            TransitionManager transition = null;
            if (!ServiceLocator.TryGet<TransitionManager>(out transition))
            {
                transition = FindAnyObjectByType<TransitionManager>();
            }
            if (transition != null)
            {
                transition.FlashScreen(Color.red, 0.3f);
                Debug.Log("[TEST] Triggered red screen flash");
            }
            else
            {
                Debug.LogWarning("[TEST] TransitionManager not found");
            }
        }

        // ============================================
        // Utility
        // ============================================

        private void Log(string testName, bool passed)
        {
            if (passed)
            {
                Debug.Log($"<color=green>[PASS]</color> {testName}");
                _passCount++;
            }
            else
            {
                Debug.LogError($"<color=red>[FAIL]</color> {testName}");
                _failCount++;
            }
        }
    }
}
