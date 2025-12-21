// ============================================
// CombatScreenCZN.cs
// Combat screen with CZN layout integrating all combat UI components
// ============================================

using UnityEngine;
using UnityEngine.UI;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Combat;
using HNR.Characters;
using HNR.Cards;
using HNR.UI.Combat;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Combat screen with CZN layout integrating all combat UI components.
    /// Features: top HUD, left sidebar, bottom command center, world-space UI.
    /// </summary>
    public class CombatScreenCZN : ScreenBase
    {
        // ============================================
        // Top HUD
        // ============================================

        [Header("Top HUD")]
        [SerializeField, Tooltip("Wide HP bar with embedded portraits")]
        private SharedVitalityBarCZN _vitalityBar;

        [SerializeField, Tooltip("Speed, auto-battle, settings buttons")]
        private SystemMenuBar _systemMenu;

        // ============================================
        // Left Sidebar
        // ============================================

        [Header("Left Sidebar")]
        [SerializeField, Tooltip("Party member slots with EP gauges")]
        private PartyStatusSidebar _partySidebar;

        // ============================================
        // Bottom Command Center
        // ============================================

        [Header("Bottom Command Center")]
        [SerializeField, Tooltip("Curved card fan layout")]
        private CardFanLayout _cardFanLayout;

        [SerializeField, Tooltip("Large AP number display")]
        private APCounterDisplay _apCounter;

        [SerializeField, Tooltip("End turn button")]
        private ExecutionButton _executionButton;

        // ============================================
        // World Space UI
        // ============================================

        [Header("World Space")]
        [SerializeField, Tooltip("Container for enemy floating UIs")]
        private Transform _enemyUIContainer;

        [SerializeField, Tooltip("Container for ally indicators")]
        private Transform _allyIndicatorContainer;

        [SerializeField, Tooltip("Prefab for enemy floating UI")]
        private EnemyFloatingUI _enemyUIPrefab;

        [SerializeField, Tooltip("Prefab for ally indicator")]
        private AllyIndicator _allyIndicatorPrefab;

        [SerializeField, Tooltip("Transforms for ally slot positions")]
        private Transform[] _allySlots;

        // ============================================
        // Runtime State
        // ============================================

        private CombatContext _context;
        private int _cardDrawIndex;

        // ============================================
        // Unity Lifecycle
        // ============================================

        protected virtual void Awake()
        {
            _showGlobalHeader = false;
            _showGlobalNav = false;
        }

        // ============================================
        // ScreenBase Overrides
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            // Get combat context
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                _context = turnManager.Context;
            }

            if (_context == null)
            {
                Debug.LogWarning("[CombatScreenCZN] No combat context available");
                return;
            }

            _cardDrawIndex = 0;
            InitializeUI();
            SubscribeToEvents();
        }

        public override void OnHide()
        {
            base.OnHide();
            UnsubscribeFromEvents();

            if (_systemMenu != null)
            {
                _systemMenu.ResetOnCombatEnd();
            }

            ClearWorldSpaceUI();
        }

        // ============================================
        // Initialization
        // ============================================

        private void InitializeUI()
        {
            // Auto-find missing references
            AutoFindMissingReferences();

            // Top HUD
            if (_vitalityBar != null)
            {
                _vitalityBar.SetPartyPortraits(_context.Team.ToArray());
                _vitalityBar.Initialize(_context.TeamHP, _context.TeamMaxHP, _context.TeamBlock);
                Debug.Log($"[CombatScreenCZN] Vitality bar initialized with HP {_context.TeamHP}/{_context.TeamMaxHP}");
            }
            else
            {
                Debug.LogWarning("[CombatScreenCZN] _vitalityBar is null - HP bar will not update!");
            }

            // Left Sidebar
            if (_partySidebar != null)
            {
                _partySidebar.Initialize(_context.Team.ToArray());
            }

            // Bottom Command Center
            if (_apCounter != null)
            {
                _apCounter.SetAP(_context.CurrentAP, _context.MaxAP);
            }

            // World Space
            SpawnEnemyUIs();
            SpawnAllyIndicators();
        }

        /// <summary>
        /// Auto-find UI component references if not assigned in Inspector.
        /// </summary>
        private void AutoFindMissingReferences()
        {
            // Find SharedVitalityBarCZN
            if (_vitalityBar == null)
            {
                _vitalityBar = FindAnyObjectByType<SharedVitalityBarCZN>(FindObjectsInactive.Include);
                if (_vitalityBar != null)
                {
                    Debug.Log($"[CombatScreenCZN] Auto-found SharedVitalityBarCZN: {_vitalityBar.name}");
                }
                else
                {
                    // Try to find the GameObject and add the component
                    var vitalityBarGO = GameObject.Find("SharedVitalityBar");
                    if (vitalityBarGO != null)
                    {
                        _vitalityBar = vitalityBarGO.AddComponent<SharedVitalityBarCZN>();
                        AutoWireVitalityBar(_vitalityBar, vitalityBarGO);
                        Debug.Log($"[CombatScreenCZN] Added SharedVitalityBarCZN component to {vitalityBarGO.name}");
                    }
                }
            }

            // Find PartyStatusSidebar
            if (_partySidebar == null)
            {
                _partySidebar = FindAnyObjectByType<PartyStatusSidebar>(FindObjectsInactive.Include);
                if (_partySidebar != null)
                {
                    Debug.Log($"[CombatScreenCZN] Auto-found PartyStatusSidebar: {_partySidebar.name}");
                }
            }

            // Find APCounterDisplay
            if (_apCounter == null)
            {
                _apCounter = FindAnyObjectByType<APCounterDisplay>(FindObjectsInactive.Include);
                if (_apCounter != null)
                {
                    Debug.Log($"[CombatScreenCZN] Auto-found APCounterDisplay: {_apCounter.name}");
                }
            }

            // Find CardFanLayout
            if (_cardFanLayout == null)
            {
                _cardFanLayout = FindAnyObjectByType<CardFanLayout>(FindObjectsInactive.Include);
                if (_cardFanLayout != null)
                {
                    Debug.Log($"[CombatScreenCZN] Auto-found CardFanLayout: {_cardFanLayout.name}");
                }
            }

            // Find enemy UI container
            if (_enemyUIContainer == null)
            {
                var container = GameObject.Find("EnemyUIContainer");
                if (container != null)
                {
                    _enemyUIContainer = container.transform;
                    Debug.Log($"[CombatScreenCZN] Auto-found EnemyUIContainer");
                }
            }
        }

        // ============================================
        // World Space UI Management
        // ============================================

        private void SpawnEnemyUIs()
        {
            if (_enemyUIContainer == null)
            {
                Debug.LogWarning("[CombatScreenCZN] Cannot spawn enemy UIs - _enemyUIContainer is null");
                return;
            }

            // Auto-load prefab if not assigned
            if (_enemyUIPrefab == null)
            {
                _enemyUIPrefab = Resources.Load<EnemyFloatingUI>("Prefabs/UI/Combat/EnemyFloatingUI");
#if UNITY_EDITOR
                if (_enemyUIPrefab == null)
                {
                    var prefabGO = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Combat/EnemyFloatingUI.prefab");
                    if (prefabGO != null)
                    {
                        _enemyUIPrefab = prefabGO.GetComponent<EnemyFloatingUI>();
                        Debug.Log($"[CombatScreenCZN] Loaded EnemyFloatingUI from AssetDatabase. Prefab childCount={prefabGO.transform.childCount}");
                    }
                }
#endif
                if (_enemyUIPrefab == null)
                {
                    Debug.LogError("[CombatScreenCZN] Cannot spawn enemy UIs - _enemyUIPrefab is null and auto-load failed. Run: HNR > 2. Prefabs > UI > Combat UI > EnemyFloatingUI Only");
                    return;
                }
                Debug.Log($"[CombatScreenCZN] Auto-loaded EnemyFloatingUI prefab with {_enemyUIPrefab.transform.childCount} children");
            }

            // Clear existing
            foreach (Transform child in _enemyUIContainer)
            {
                Destroy(child.gameObject);
            }

            // Spawn for each enemy
            int spawnedCount = 0;
            foreach (var enemy in _context.Enemies)
            {
                if (enemy == null || enemy.IsDead) continue;

                var ui = Instantiate(_enemyUIPrefab, _enemyUIContainer);
                Debug.Log($"[CombatScreenCZN] Instantiated EnemyFloatingUI for {enemy.Name}, childCount={ui.transform.childCount}, prefab childCount={_enemyUIPrefab.transform.childCount}");
                ui.Initialize(enemy);
                spawnedCount++;
            }
            Debug.Log($"[CombatScreenCZN] Spawned {spawnedCount} enemy floating UIs");
        }

        private void SpawnAllyIndicators()
        {
            // Position RequiemInstances at ally slots WITHOUT reparenting
            // IMPORTANT: We must NOT reparent RequiemInstances because they need to persist
            // across scene transitions. They are children of RunManager (DontDestroyOnLoad).
            int positionedCount = 0;
            for (int i = 0; i < _context.Team.Count; i++)
            {
                var requiem = _context.Team[i];
                if (requiem == null) continue;

                // Position at ally slot location (without changing parent)
                if (_allySlots != null && i < _allySlots.Length && _allySlots[i] != null)
                {
                    // Just set world position, keep parent as RunManager
                    requiem.transform.position = _allySlots[i].position;
                    requiem.transform.rotation = _allySlots[i].rotation;

                    // Ensure visual is facing right (toward enemies)
                    requiem.Visual?.SetFacing(true);

                    Debug.Log($"[CombatScreenCZN] Positioned {requiem.Name} at slot {i}: {_allySlots[i].name}");
                }
                else
                {
                    // Fallback to fixed positions if slots not configured
                    Vector3 fallbackPosition = new Vector3(-7f + (i * 2f), 0f, 0f);
                    requiem.transform.position = fallbackPosition;
                    requiem.Visual?.SetFacing(true);
                    Debug.LogWarning($"[CombatScreenCZN] Using fallback position for {requiem.Name}: {fallbackPosition}");
                }

                positionedCount++;
            }
            Debug.Log($"[CombatScreenCZN] Positioned {positionedCount} Requiem visuals at ally slots");
        }

        private void ClearWorldSpaceUI()
        {
            if (_enemyUIContainer != null)
            {
                foreach (Transform child in _enemyUIContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            if (_allyIndicatorContainer != null)
            {
                foreach (Transform child in _allyIndicatorContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // ============================================
        // Event Management
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<CardDiscardedEvent>(OnCardDiscarded);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<CardDiscardedEvent>(OnCardDiscarded);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCardDrawn(CardDrawnEvent evt)
        {
            if (_cardFanLayout == null || evt.Card == null) return;

            // Get card from pool
            if (ServiceLocator.TryGet<IPoolManager>(out var poolManager))
            {
                var card = poolManager.Get<CombatCard>();
                if (card != null)
                {
                    card.Initialize(evt.Card);
                    _cardFanLayout.AddCard(card, _cardDrawIndex * 0.1f);
                    _cardDrawIndex++;
                }
            }
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            // Card removal handled by CardFanLayout internally
            _cardDrawIndex = Mathf.Max(0, _cardDrawIndex - 1);
        }

        private void OnCardDiscarded(CardDiscardedEvent evt)
        {
            _cardDrawIndex = Mathf.Max(0, _cardDrawIndex - 1);
        }

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            // Enemy UI handles its own cleanup via event subscription
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            Debug.Log($"[CombatScreenCZN] Combat ended - Victory: {evt.Victory}");

            // Get enemy name and rewards from context
            string enemyName = "Enemy";
            int voidShards = 0;
            int soulEssence = 0;

            if (ServiceLocator.TryGet<TurnManager>(out var turnManager) && turnManager.Context != null)
            {
                var context = turnManager.Context;
                if (context.Enemies != null && context.Enemies.Count > 0)
                {
                    enemyName = context.Enemies[0].Name;
                    foreach (var enemy in context.Enemies)
                    {
                        if (enemy.Data != null)
                        {
                            voidShards += enemy.Data.VoidShardReward;
                        }
                    }
                }
            }

            // Generate random card rewards for victory
            System.Collections.Generic.List<CardDataSO> cardRewards = null;
            if (evt.Victory)
            {
                cardRewards = GenerateCardRewards(3);
            }

            // Show ResultsScreen and configure it
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<ResultsScreen>();

                // Get the screen after showing it
                if (uiManager is UIManager uiMgr)
                {
                    var resultsScreen = uiMgr.GetScreen<ResultsScreen>();
                    if (resultsScreen != null)
                    {
                        resultsScreen.SetResults(evt.Victory, enemyName, voidShards, soulEssence, cardRewards);
                    }
                }
            }
        }

        /// <summary>
        /// Generates random card rewards from available cards.
        /// </summary>
        private System.Collections.Generic.List<CardDataSO> GenerateCardRewards(int count)
        {
            var rewards = new System.Collections.Generic.List<CardDataSO>();
            var allCards = Resources.LoadAll<CardDataSO>("Data/Cards");

            if (allCards == null || allCards.Length == 0)
            {
                allCards = Resources.LoadAll<CardDataSO>("");
            }

            if (allCards != null && allCards.Length > 0)
            {
                var shuffled = new System.Collections.Generic.List<CardDataSO>(allCards);
                for (int i = shuffled.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
                }

                for (int i = 0; i < Mathf.Min(count, shuffled.Count); i++)
                {
                    rewards.Add(shuffled[i]);
                }
            }

            return rewards;
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Sets the active party member highlight in the sidebar.
        /// </summary>
        /// <param name="index">Index of active member (0-2).</param>
        public void SetActivePartyMember(int index)
        {
            if (_partySidebar != null)
            {
                _partySidebar.SetActiveSlot(index);
            }
        }

        /// <summary>
        /// Refreshes all UI components from current context state.
        /// </summary>
        public void RefreshUI()
        {
            if (_context == null) return;

            if (_vitalityBar != null)
            {
                _vitalityBar.UpdateHealth(_context.TeamHP, _context.TeamMaxHP);
                _vitalityBar.UpdateBlock(_context.TeamBlock);
            }

            if (_partySidebar != null)
            {
                _partySidebar.RefreshAll();
            }

            if (_apCounter != null)
            {
                _apCounter.SetAP(_context.CurrentAP, _context.MaxAP);
            }
        }

        // ============================================
        // Auto-Wiring Helpers
        // ============================================

        /// <summary>
        /// Auto-wire SharedVitalityBarCZN child references using reflection.
        /// </summary>
        private void AutoWireVitalityBar(SharedVitalityBarCZN vitalityBar, GameObject go)
        {
            var type = typeof(SharedVitalityBarCZN);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            // Find HP Fill - look for "Fill" child inside "HPBar", or any child named "Fill"
            Transform hpFillTransform = null;
            var hpBarTransform = go.transform.Find("HPBar");
            if (hpBarTransform != null)
            {
                hpFillTransform = hpBarTransform.Find("Fill");
            }

            // Fallback: search recursively for any "Fill" object
            if (hpFillTransform == null)
            {
                foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "Fill" || child.name.Contains("HPFill") || child.name.Contains("HealthFill"))
                    {
                        hpFillTransform = child;
                        break;
                    }
                }
            }

            if (hpFillTransform != null)
            {
                var hpFillImage = hpFillTransform.GetComponent<Image>();
                if (hpFillImage != null)
                {
                    // Ensure Image is set to Filled type for fillAmount to work
                    hpFillImage.type = Image.Type.Filled;
                    hpFillImage.fillMethod = Image.FillMethod.Horizontal;
                    hpFillImage.fillOrigin = 0; // Left
                    hpFillImage.fillAmount = 1f;

                    type.GetField("_healthFill", flags)?.SetValue(vitalityBar, hpFillImage);
                    Debug.Log($"[CombatScreenCZN] Auto-wired _healthFill to {hpFillTransform.name} (set to Filled type)");
                }
            }
            else
            {
                Debug.LogWarning("[CombatScreenCZN] Could not find HP fill image for SharedVitalityBar");
            }

            // Find HP Text
            var hpText = go.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (hpText != null)
            {
                type.GetField("_hpText", flags)?.SetValue(vitalityBar, hpText);
                Debug.Log($"[CombatScreenCZN] Auto-wired _hpText to {hpText.name}");
            }

            // Find damage fill (secondary fill for linger effect) - use HPBar background as fallback
            var damageFillTransform = go.transform.Find("DamageFill");
            if (damageFillTransform == null && hpBarTransform != null)
            {
                // Use HPBar itself as damage fill background
                var hpBarImage = hpBarTransform.GetComponent<Image>();
                if (hpBarImage != null)
                {
                    hpBarImage.type = Image.Type.Filled;
                    hpBarImage.fillMethod = Image.FillMethod.Horizontal;
                    hpBarImage.fillOrigin = 0;
                    type.GetField("_damageFill", flags)?.SetValue(vitalityBar, hpBarImage);
                    Debug.Log($"[CombatScreenCZN] Auto-wired _damageFill to HPBar background");
                }
            }
            else if (damageFillTransform != null)
            {
                var damageFillImage = damageFillTransform.GetComponent<Image>();
                if (damageFillImage != null)
                {
                    type.GetField("_damageFill", flags)?.SetValue(vitalityBar, damageFillImage);
                    Debug.Log($"[CombatScreenCZN] Auto-wired _damageFill");
                }
            }
        }
    }
}
