// ============================================
// CombatFeedbackIntegrator.cs
// Coordinates VFX, audio, haptics, and screen shake
// ============================================

using UnityEngine;
using HNR.Core.Events;
using HNR.Combat;
using HNR.VFX;
using HNR.Audio;
using HNR.UI;

namespace HNR.Core
{
    /// <summary>
    /// Coordinates all feedback systems for combat events.
    /// Provides unified API for complex feedback combinations.
    /// </summary>
    /// <remarks>
    /// Note: Individual controllers (CombatAudioController, CombatVFXController, ScreenShakeController)
    /// handle their own event subscriptions. This integrator provides additional coordination
    /// for combined feedback scenarios and manual triggering.
    ///
    /// To avoid duplicate feedback, you can either:
    /// 1. Disable automatic event handling in individual controllers
    /// 2. Use this integrator for manual triggering only
    /// </remarks>
    public class CombatFeedbackIntegrator : MonoBehaviour
    {
        // ============================================
        // System References
        // ============================================

        [Header("System References")]
        [SerializeField, Tooltip("VFX controller for particle effects")]
        private CombatVFXController _vfxController;

        [SerializeField, Tooltip("Screen shake controller")]
        private ScreenShakeController _shakeController;

        // ============================================
        // Feature Toggles
        // ============================================

        [Header("Feature Toggles")]
        [SerializeField] private bool _enableVFX = true;
        [SerializeField] private bool _enableScreenShake = true;
        [SerializeField] private bool _enableHaptics = true;

        [Header("Auto-Subscribe to Events")]
        [SerializeField, Tooltip("Subscribe to combat events for automatic feedback coordination")]
        private bool _autoSubscribe = true;

        // ============================================
        // Damage Thresholds
        // ============================================

        [Header("Damage Thresholds")]
        [SerializeField, Tooltip("Minimum damage for heavy hit feedback")]
        private int _heavyHitThreshold = 20;

        [SerializeField, Tooltip("Minimum damage for medium hit feedback")]
        private int _mediumHitThreshold = 10;

        // ============================================
        // Runtime References
        // ============================================

        private HapticController _hapticController;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Get haptic controller from ServiceLocator
            ServiceLocator.TryGet(out _hapticController);

            // Try to find controllers if not assigned
            if (_vfxController == null)
                _vfxController = FindAnyObjectByType<CombatVFXController>();

            if (_shakeController == null)
                ServiceLocator.TryGet(out _shakeController);
        }

        private void OnEnable()
        {
            if (!_autoSubscribe) return;

            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        private void OnDisable()
        {
            if (!_autoSubscribe) return;

            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            if (evt.IsCritical)
            {
                TriggerCriticalFeedback(evt.Target?.Position ?? Vector3.zero);
            }
            else if (evt.Amount >= _heavyHitThreshold)
            {
                TriggerHeavyHitFeedback(evt.Target?.Position ?? Vector3.zero);
            }
            else if (evt.Amount >= _mediumHitThreshold)
            {
                TriggerMediumHitFeedback(evt.Target?.Position ?? Vector3.zero);
            }
            else if (evt.Amount > 0)
            {
                TriggerLightHitFeedback();
            }
        }

        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            Vector3 position = Vector3.zero;

            // Get position from Requiem if available
            if (evt.Requiem != null && evt.Requiem.transform != null)
            {
                position = evt.Requiem.transform.position;
            }

            TriggerNullStateFeedback(position);
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            // Light haptic for card plays
            if (_enableHaptics)
            {
                _hapticController?.LightTap();
            }
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            if (evt.Victory)
            {
                TriggerVictoryFeedback();
            }
            else
            {
                TriggerDefeatFeedback();
            }
        }

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            TriggerEnemyDefeatedFeedback(evt.Enemy?.Position ?? Vector3.zero);
        }

        // ============================================
        // Public API - Combined Feedback
        // ============================================

        /// <summary>
        /// Trigger feedback for light hit (less than medium threshold).
        /// </summary>
        public void TriggerLightHitFeedback()
        {
            // Note: ScreenShakeController handles its own shake via events
            // Only add haptics here if not already triggered
            if (_enableHaptics)
            {
                _hapticController?.LightTap();
            }
        }

        /// <summary>
        /// Trigger feedback for medium hit.
        /// </summary>
        /// <param name="position">World position for positional effects</param>
        public void TriggerMediumHitFeedback(Vector3 position)
        {
            if (_enableScreenShake)
            {
                _shakeController?.Shake(ShakeIntensity.Medium);
            }

            if (_enableHaptics)
            {
                _hapticController?.MediumImpact();
            }
        }

        /// <summary>
        /// Trigger feedback for heavy hit.
        /// </summary>
        /// <param name="position">World position for positional effects</param>
        public void TriggerHeavyHitFeedback(Vector3 position)
        {
            if (_enableScreenShake)
            {
                _shakeController?.Shake(ShakeIntensity.Heavy);
            }

            if (_enableHaptics)
            {
                _hapticController?.MediumImpact();
            }
        }

        /// <summary>
        /// Trigger feedback for critical hit.
        /// </summary>
        /// <param name="position">World position for positional effects</param>
        public void TriggerCriticalFeedback(Vector3 position)
        {
            if (_enableScreenShake)
            {
                _shakeController?.Shake(ShakeIntensity.Heavy);
            }

            if (_enableHaptics)
            {
                _hapticController?.HeavyImpact();
            }

            Debug.Log("[CombatFeedbackIntegrator] Critical hit feedback triggered");
        }

        /// <summary>
        /// Trigger feedback for Null State entry.
        /// </summary>
        /// <param name="position">World position for positional effects</param>
        public void TriggerNullStateFeedback(Vector3 position)
        {
            if (_enableScreenShake)
            {
                _shakeController?.Shake(ShakeIntensity.Heavy);
            }

            if (_enableHaptics)
            {
                _hapticController?.HeavyImpact();
            }

            Debug.Log("[CombatFeedbackIntegrator] Null State feedback triggered");
        }

        /// <summary>
        /// Trigger feedback for enemy defeat.
        /// </summary>
        /// <param name="position">World position of defeated enemy</param>
        public void TriggerEnemyDefeatedFeedback(Vector3 position)
        {
            if (_enableScreenShake)
            {
                _shakeController?.Shake(ShakeIntensity.Light);
            }

            if (_enableHaptics)
            {
                _hapticController?.MediumImpact();
            }
        }

        /// <summary>
        /// Trigger victory feedback.
        /// </summary>
        public void TriggerVictoryFeedback()
        {
            if (_enableHaptics)
            {
                _hapticController?.MediumImpact();
            }

            Debug.Log("[CombatFeedbackIntegrator] Victory feedback triggered");
        }

        /// <summary>
        /// Trigger defeat feedback.
        /// </summary>
        public void TriggerDefeatFeedback()
        {
            if (_enableScreenShake)
            {
                _shakeController?.Shake(ShakeIntensity.Heavy);
            }

            if (_enableHaptics)
            {
                _hapticController?.HeavyImpact();
            }

            Debug.Log("[CombatFeedbackIntegrator] Defeat feedback triggered");
        }

        // ============================================
        // Settings API
        // ============================================

        /// <summary>
        /// Enable or disable VFX feedback.
        /// </summary>
        public void SetVFXEnabled(bool enabled)
        {
            _enableVFX = enabled;
            Debug.Log($"[CombatFeedbackIntegrator] VFX {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Enable or disable screen shake.
        /// </summary>
        public void SetScreenShakeEnabled(bool enabled)
        {
            _enableScreenShake = enabled;
            _shakeController?.SetEnabled(enabled);
            Debug.Log($"[CombatFeedbackIntegrator] Screen shake {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Enable or disable haptic feedback.
        /// </summary>
        public void SetHapticsEnabled(bool enabled)
        {
            _enableHaptics = enabled;

            if (_hapticController != null)
            {
                _hapticController.HapticsEnabled = enabled;
            }

            Debug.Log($"[CombatFeedbackIntegrator] Haptics {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Enable or disable all feedback systems.
        /// </summary>
        public void SetAllFeedbackEnabled(bool enabled)
        {
            SetVFXEnabled(enabled);
            SetScreenShakeEnabled(enabled);
            SetHapticsEnabled(enabled);
        }

        // ============================================
        // Properties
        // ============================================

        /// <summary>Whether VFX feedback is enabled.</summary>
        public bool VFXEnabled => _enableVFX;

        /// <summary>Whether screen shake is enabled.</summary>
        public bool ScreenShakeEnabled => _enableScreenShake;

        /// <summary>Whether haptic feedback is enabled.</summary>
        public bool HapticsEnabled => _enableHaptics;
    }
}
