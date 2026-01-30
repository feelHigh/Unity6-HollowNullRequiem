// ============================================
// RuntimeUIPrefabConfigSO.cs
// Centralized configuration for runtime UI prefabs
// ============================================

using UnityEngine;

namespace HNR.UI.Config
{
    /// <summary>
    /// ScriptableObject holding references to all runtime-generated UI prefabs.
    /// Components can reference this config to use prefabs instead of creating GameObjects at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "RuntimeUIPrefabConfig", menuName = "HNR/Config/Runtime UI Prefab Config")]
    public class RuntimeUIPrefabConfigSO : ScriptableObject
    {
        // ============================================
        // Dialog Prefabs
        // ============================================

        [Header("Dialogs")]
        [SerializeField, Tooltip("Confirmation dialog modal prefab")]
        private GameObject _confirmationDialogPrefab;

        public GameObject ConfirmationDialogPrefab => _confirmationDialogPrefab;

        // ============================================
        // Combat UI Prefabs
        // ============================================

        [Header("Combat UI")]
        [SerializeField, Tooltip("Status effect icon prefab")]
        private GameObject _statusIconPrefab;

        [SerializeField, Tooltip("Relic display icon prefab")]
        private GameObject _relicDisplayIconPrefab;

        public GameObject StatusIconPrefab => _statusIconPrefab;
        public GameObject RelicDisplayIconPrefab => _relicDisplayIconPrefab;

        // ============================================
        // Card Display Prefabs
        // ============================================

        [Header("Card Display")]
        [SerializeField, Tooltip("Deck viewer card slot prefab")]
        private GameObject _deckViewerCardSlotPrefab;

        [SerializeField, Tooltip("Simple card display item prefab")]
        private GameObject _simpleCardDisplayItemPrefab;

        [SerializeField, Tooltip("Sanctuary card slot prefab")]
        private GameObject _sanctuaryCardSlotPrefab;

        [SerializeField, Tooltip("Sanctuary upgrade slot prefab")]
        private GameObject _sanctuaryUpgradeSlotPrefab;

        [SerializeField, Tooltip("Reward card slot prefab")]
        private GameObject _rewardCardSlotPrefab;

        public GameObject DeckViewerCardSlotPrefab => _deckViewerCardSlotPrefab;
        public GameObject SimpleCardDisplayItemPrefab => _simpleCardDisplayItemPrefab;
        public GameObject SanctuaryCardSlotPrefab => _sanctuaryCardSlotPrefab;
        public GameObject SanctuaryUpgradeSlotPrefab => _sanctuaryUpgradeSlotPrefab;
        public GameObject RewardCardSlotPrefab => _rewardCardSlotPrefab;

        // ============================================
        // Shop Prefabs
        // ============================================

        [Header("Shop")]
        [SerializeField, Tooltip("Relic shop slot prefab")]
        private GameObject _relicShopSlotPrefab;

        public GameObject RelicShopSlotPrefab => _relicShopSlotPrefab;

        // ============================================
        // Banner Prefabs
        // ============================================

        [Header("Banner Carousel")]
        [SerializeField, Tooltip("Banner slide prefab")]
        private GameObject _bannerSlidePrefab;

        [SerializeField, Tooltip("Banner page indicator prefab")]
        private GameObject _bannerIndicatorPrefab;

        public GameObject BannerSlidePrefab => _bannerSlidePrefab;
        public GameObject BannerIndicatorPrefab => _bannerIndicatorPrefab;

        // ============================================
        // Character Selection Prefabs
        // ============================================

        [Header("Character Selection")]
        [SerializeField, Tooltip("Requiem selection slot prefab")]
        private GameObject _requiemSelectionSlotPrefab;

        [SerializeField, Tooltip("Requiem portrait button prefab")]
        private GameObject _requiemPortraitButtonPrefab;

        public GameObject RequiemSelectionSlotPrefab => _requiemSelectionSlotPrefab;
        public GameObject RequiemPortraitButtonPrefab => _requiemPortraitButtonPrefab;

        // ============================================
        // Validation
        // ============================================

        /// <summary>
        /// Returns true if all prefab references are assigned.
        /// </summary>
        public bool IsValid()
        {
            return _confirmationDialogPrefab != null &&
                   _statusIconPrefab != null &&
                   _relicDisplayIconPrefab != null &&
                   _deckViewerCardSlotPrefab != null &&
                   _simpleCardDisplayItemPrefab != null &&
                   _sanctuaryCardSlotPrefab != null &&
                   _sanctuaryUpgradeSlotPrefab != null &&
                   _rewardCardSlotPrefab != null &&
                   _relicShopSlotPrefab != null &&
                   _bannerSlidePrefab != null &&
                   _bannerIndicatorPrefab != null &&
                   _requiemSelectionSlotPrefab != null &&
                   _requiemPortraitButtonPrefab != null;
        }

        /// <summary>
        /// Returns count of assigned vs total prefabs.
        /// </summary>
        public (int assigned, int total) GetAssignmentStatus()
        {
            int total = 13;
            int assigned = 0;

            if (_confirmationDialogPrefab != null) assigned++;
            if (_statusIconPrefab != null) assigned++;
            if (_relicDisplayIconPrefab != null) assigned++;
            if (_deckViewerCardSlotPrefab != null) assigned++;
            if (_simpleCardDisplayItemPrefab != null) assigned++;
            if (_sanctuaryCardSlotPrefab != null) assigned++;
            if (_sanctuaryUpgradeSlotPrefab != null) assigned++;
            if (_rewardCardSlotPrefab != null) assigned++;
            if (_relicShopSlotPrefab != null) assigned++;
            if (_bannerSlidePrefab != null) assigned++;
            if (_bannerIndicatorPrefab != null) assigned++;
            if (_requiemSelectionSlotPrefab != null) assigned++;
            if (_requiemPortraitButtonPrefab != null) assigned++;

            return (assigned, total);
        }

        // ============================================
        // Static Helper
        // ============================================

        private static RuntimeUIPrefabConfigSO _instance;

        /// <summary>
        /// Gets the RuntimeUIPrefabConfig from Resources.
        /// </summary>
        public static RuntimeUIPrefabConfigSO Instance
        {
            get
            {
                // Check if instance is null or destroyed (Unity null check)
                if (_instance == null || !_instance)
                {
                    _instance = Resources.Load<RuntimeUIPrefabConfigSO>("Config/RuntimeUIPrefabConfig");
                    if (_instance == null)
                    {
                        Debug.LogWarning("[RuntimeUIPrefabConfigSO] Config not found in Resources/Config/RuntimeUIPrefabConfig");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Clears the cached instance. Call when config asset is modified.
        /// </summary>
        public static void ClearCache()
        {
            _instance = null;
        }

        /// <summary>
        /// Reset static cache on domain reload to prevent stale references.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }
    }
}
