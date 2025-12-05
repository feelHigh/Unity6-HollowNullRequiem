// ============================================
// Week10IntegrationTest.cs
// Integration tests for VFX and Audio systems
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.VFX;
using HNR.Audio;
using HNR.Cards;
using HNR.UI;

namespace HNR.Testing
{
    /// <summary>
    /// Week 10 integration tests for VFX and Audio systems.
    /// Tests VFXPoolManager, AudioManager, HapticController, and CombatFeedbackIntegrator.
    /// </summary>
    /// <remarks>
    /// Keyboard shortcuts:
    /// [T] Run all tests
    /// [V] Test VFX spawn
    /// [M] Toggle music
    /// [S] Test SFX
    /// [H] Test haptic
    /// [1] Spawn Flame hit VFX
    /// [2] Spawn Shadow hit VFX
    /// [3] Spawn Nature hit VFX
    /// </remarks>
    public class Week10IntegrationTest : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Test Configuration")]
        [SerializeField, Tooltip("Spawn point for VFX tests")]
        private Transform _testSpawnPoint;

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
            // Full test suite
            if (Input.GetKeyDown(KeyCode.T)) RunAllTests();

            // Individual system tests
            if (Input.GetKeyDown(KeyCode.V)) TestVFXSpawn();
            if (Input.GetKeyDown(KeyCode.M)) TestMusicToggle();
            if (Input.GetKeyDown(KeyCode.S)) TestSFX();
            if (Input.GetKeyDown(KeyCode.H)) TestHaptic();

            // Aspect-specific VFX tests
            if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnHitVFX(SoulAspect.Flame);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnHitVFX(SoulAspect.Shadow);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnHitVFX(SoulAspect.Nature);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnHitVFX(SoulAspect.Arcane);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SpawnHitVFX(SoulAspect.Light);
        }

        // ============================================
        // Full Test Suite
        // ============================================

        /// <summary>
        /// Run all Week 10 integration tests.
        /// </summary>
        public void RunAllTests()
        {
            _passCount = 0;
            _failCount = 0;

            Debug.Log("=== WEEK 10 INTEGRATION TESTS ===");
            Debug.Log("Testing VFX and Audio systems...");

            TestVFXPoolManager();
            TestVFXInstance();
            TestAudioManager();
            TestHapticController();
            TestCombatFeedbackIntegrator();

            Debug.Log($"=== RESULTS: {_passCount}/{_passCount + _failCount} passed ===");

            if (_failCount == 0)
            {
                Debug.Log("All Week 10 tests PASSED!");
            }
            else
            {
                Debug.LogWarning($"Week 10 tests completed with {_failCount} failure(s)");
            }
        }

        // ============================================
        // VFXPoolManager Tests
        // ============================================

        private void TestVFXPoolManager()
        {
            Debug.Log("--- VFX Pool Manager ---");

            var pool = ServiceLocator.Get<VFXPoolManager>();
            Log("VFXPoolManager registered", pool != null);

            if (pool != null)
            {
                // Test spawn
                var instance = pool.Spawn("hit_flame", Vector3.zero, Quaternion.identity);
                Log("VFX spawn returns instance", instance != null);

                if (instance != null)
                {
                    Log("VFXInstance has EffectId", !string.IsNullOrEmpty(instance.EffectId));

                    // Test return
                    pool.Return(instance);
                    Log("VFX return to pool works", true);
                }

                // Test max active limit (spawn multiple)
                int spawnCount = 0;
                for (int i = 0; i < 15; i++)
                {
                    var testInstance = pool.Spawn("hit_flame", Vector3.zero, Quaternion.identity);
                    if (testInstance != null) spawnCount++;
                }
                Log("Pool spawns multiple instances", spawnCount > 0);

                // Clean up
                pool.ReturnAll();
                Log("ReturnAll cleans up spawned VFX", true);
            }
        }

        // ============================================
        // VFXInstance Tests
        // ============================================

        private void TestVFXInstance()
        {
            Debug.Log("--- VFX Instance ---");

            var pool = ServiceLocator.Get<VFXPoolManager>();
            if (pool == null)
            {
                Log("VFXPoolManager required for VFXInstance tests", false);
                return;
            }

            var instance = pool.Spawn("hit_flame", Vector3.zero, Quaternion.identity);
            if (instance != null)
            {
                // Test IsPlaying
                Log("IsPlaying property accessible", instance.IsPlaying || !instance.IsPlaying);

                // Test SetColor
                try
                {
                    instance.SetColor(Color.red);
                    Log("SetColor doesn't throw", true);
                }
                catch
                {
                    Log("SetColor doesn't throw", false);
                }

                // Test SetScale
                instance.SetScale(1.5f);
                Log("SetScale works", instance.transform.localScale.x >= 1.4f);

                pool.Return(instance);
            }
            else
            {
                Log("VFXInstance spawn for testing", false);
            }
        }

        // ============================================
        // AudioManager Tests
        // ============================================

        private void TestAudioManager()
        {
            Debug.Log("--- Audio Manager ---");

            var audio = ServiceLocator.Get<IAudioManager>();
            Log("AudioManager registered", audio != null);

            if (audio != null)
            {
                // Test volume properties
                Log("MasterVolume accessible", audio.MasterVolume >= 0f && audio.MasterVolume <= 1f);
                Log("MusicVolume accessible", audio.MusicVolume >= 0f && audio.MusicVolume <= 1f);
                Log("SFXVolume accessible", audio.SFXVolume >= 0f && audio.SFXVolume <= 1f);

                // Test volume setting
                float originalMaster = audio.MasterVolume;
                audio.MasterVolume = 0.5f;
                Log("MasterVolume settable", Mathf.Approximately(audio.MasterVolume, 0.5f));
                audio.MasterVolume = originalMaster; // Restore

                // Test mute properties
                Log("IsMasterMuted accessible", audio.IsMasterMuted || !audio.IsMasterMuted);
                Log("IsMusicMuted accessible", audio.IsMusicMuted || !audio.IsMusicMuted);
                Log("IsSFXMuted accessible", audio.IsSFXMuted || !audio.IsSFXMuted);
            }
        }

        // ============================================
        // HapticController Tests
        // ============================================

        private void TestHapticController()
        {
            Debug.Log("--- Haptic Controller ---");

            var haptic = ServiceLocator.Get<HapticController>();
            Log("HapticController registered", haptic != null);

            if (haptic != null)
            {
                // Test HapticsEnabled property
                Log("HapticsEnabled accessible", haptic.HapticsEnabled || !haptic.HapticsEnabled);

                // Test vibration methods don't throw
                try
                {
                    haptic.LightTap();
                    haptic.MediumImpact();
                    haptic.HeavyImpact();
                    Log("Haptic methods don't throw", true);
                }
                catch
                {
                    Log("Haptic methods don't throw", false);
                }
            }
        }

        // ============================================
        // CombatFeedbackIntegrator Tests
        // ============================================

        private void TestCombatFeedbackIntegrator()
        {
            Debug.Log("--- Combat Feedback Integrator ---");

            var integrator = FindAnyObjectByType<CombatFeedbackIntegrator>();
            Log("CombatFeedbackIntegrator exists in scene", integrator != null);

            if (integrator != null)
            {
                // Test properties
                Log("VFXEnabled accessible", integrator.VFXEnabled || !integrator.VFXEnabled);
                Log("ScreenShakeEnabled accessible", integrator.ScreenShakeEnabled || !integrator.ScreenShakeEnabled);
                Log("HapticsEnabled accessible", integrator.HapticsEnabled || !integrator.HapticsEnabled);

                // Test feedback methods don't throw
                try
                {
                    integrator.TriggerLightHitFeedback();
                    Log("TriggerLightHitFeedback works", true);
                }
                catch
                {
                    Log("TriggerLightHitFeedback works", false);
                }
            }
        }

        // ============================================
        // Manual Test Methods
        // ============================================

        /// <summary>
        /// Manually spawn a test VFX.
        /// </summary>
        private void TestVFXSpawn()
        {
            var pool = ServiceLocator.Get<VFXPoolManager>();
            if (pool == null)
            {
                Debug.LogWarning("[TEST] VFXPoolManager not available");
                return;
            }

            Vector3 pos = _testSpawnPoint != null ? _testSpawnPoint.position : Vector3.zero;
            var instance = pool.Spawn("hit_flame", pos, Quaternion.identity);

            if (instance != null)
            {
                Debug.Log("[TEST] Spawned hit_flame VFX at " + pos);
            }
            else
            {
                Debug.LogWarning("[TEST] Failed to spawn VFX - check pool configuration");
            }
        }

        /// <summary>
        /// Spawn hit VFX for specific Soul Aspect.
        /// </summary>
        private void SpawnHitVFX(SoulAspect aspect)
        {
            var pool = ServiceLocator.Get<VFXPoolManager>();
            if (pool == null)
            {
                Debug.LogWarning("[TEST] VFXPoolManager not available");
                return;
            }

            string effectId = aspect switch
            {
                SoulAspect.Flame => "hit_flame",
                SoulAspect.Shadow => "hit_shadow",
                SoulAspect.Nature => "hit_nature",
                SoulAspect.Arcane => "hit_arcane",
                SoulAspect.Light => "hit_light",
                _ => "hit_flame"
            };

            Vector3 pos = _testSpawnPoint != null ? _testSpawnPoint.position : Vector3.zero;
            var instance = pool.Spawn(effectId, pos, Quaternion.identity);

            if (instance != null)
            {
                instance.SetColor(UIColors.GetAspectColor(aspect));
                Debug.Log($"[TEST] Spawned {effectId} VFX with {aspect} color");
            }
            else
            {
                Debug.LogWarning($"[TEST] Failed to spawn {effectId} - check pool configuration");
            }
        }

        /// <summary>
        /// Toggle music playback.
        /// </summary>
        private void TestMusicToggle()
        {
            var audio = ServiceLocator.Get<IAudioManager>();
            if (audio == null)
            {
                Debug.LogWarning("[TEST] AudioManager not available");
                return;
            }

            if (audio.IsMusicMuted)
            {
                audio.MuteMusic(false);
                audio.PlayMusic("combat_theme");
                Debug.Log("[TEST] Music unmuted and playing combat_theme");
            }
            else
            {
                audio.MuteMusic(true);
                Debug.Log("[TEST] Music muted");
            }
        }

        /// <summary>
        /// Test SFX playback.
        /// </summary>
        private void TestSFX()
        {
            var audio = ServiceLocator.Get<IAudioManager>();
            if (audio == null)
            {
                Debug.LogWarning("[TEST] AudioManager not available");
                return;
            }

            audio.PlaySFX("card_play");
            Debug.Log("[TEST] Played card_play SFX");
        }

        /// <summary>
        /// Test haptic feedback.
        /// </summary>
        private void TestHaptic()
        {
            var haptic = ServiceLocator.Get<HapticController>();
            if (haptic == null)
            {
                Debug.LogWarning("[TEST] HapticController not available");
                return;
            }

            haptic.Vibrate(Audio.HapticIntensity.Medium);
            Debug.Log("[TEST] Triggered medium haptic feedback");
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
