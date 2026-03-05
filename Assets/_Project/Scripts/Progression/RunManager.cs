// ============================================
// RunManager.cs
// Manages the current run state
// ============================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Cards;
using HNR.Characters;
using HNR.Combat;
using HNR.Map;

namespace HNR.Progression
{
    /// <summary>
    /// Manages the current run state including team, deck, and progression.
    /// Coordinates with SaveManager for persistence.
    /// </summary>
    public class RunManager : MonoBehaviour, IRunManager
    {
        // ============================================
        // Private Fields
        // ============================================

        private List<RequiemInstance> _team = new();
        private List<CardDataSO> _deck = new();
        private HashSet<string> _upgradedCardIds = new();
        private Dictionary<string, CardDataSO> _cardCache = new();
        private Dictionary<string, RequiemDataSO> _requiemCache = new();

        private int _runSeed;
        private int _teamCurrentHP;
        private int _teamMaxHP;
        private int _soulEssence;
        private int _currentZone = 1;
        private bool _isRunActive;
        private float _runStartTime;

        // Run stats
        private StatsSaveData _stats = new();

        // Battle Mission context (set before starting run)
        private int _battleMissionZone = 1;
        private DifficultyLevel _battleMissionDifficulty = DifficultyLevel.Easy;
        private bool _isBattleMissionRun = false;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Whether a run is currently active.</summary>
        public bool IsRunActive => _isRunActive;

        /// <summary>Current run seed.</summary>
        public int RunSeed => _runSeed;

        /// <summary>The player's team.</summary>
        public IReadOnlyList<RequiemInstance> Team => _team.AsReadOnly();

        /// <summary>Current team HP.</summary>
        public int TeamCurrentHP => _teamCurrentHP;

        /// <summary>Maximum team HP.</summary>
        public int TeamMaxHP => _teamMaxHP;

        /// <summary>Current Soul Essence (persists across combats).</summary>
        public int SoulEssence => _soulEssence;

        /// <summary>The player's deck.</summary>
        public IReadOnlyList<CardDataSO> Deck => _deck.AsReadOnly();

        /// <summary>Current zone number.</summary>
        public int CurrentZone => _currentZone;

        /// <summary>Run statistics.</summary>
        public StatsSaveData Stats => _stats;

        /// <summary>Battle Mission zone context.</summary>
        public int BattleMissionZone => _battleMissionZone;

        /// <summary>Battle Mission difficulty context.</summary>
        public DifficultyLevel BattleMissionDifficulty => _battleMissionDifficulty;

        /// <summary>Whether this is a Battle Mission run (vs story run).</summary>
        public bool IsBattleMissionRun => _isBattleMissionRun;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register<IRunManager>(this);
            DontDestroyOnLoad(gameObject);
            CacheResources();
        }

        private void OnDestroy()
        {
            if (ServiceLocator.Has<IRunManager>())
            {
                ServiceLocator.Unregister<IRunManager>();
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Save when app goes to background (mobile)
            if (pauseStatus && _isRunActive)
            {
                SaveRun();
                Debug.Log("[RunManager] Run saved on application pause");
            }
        }

        private void OnApplicationQuit()
        {
            // Save before app closes
            if (_isRunActive)
            {
                SaveRun();
                Debug.Log("[RunManager] Run saved on application quit");
            }
        }

        // ============================================
        // Initialization
        // ============================================

        private void CacheResources()
        {
            // Suppress warnings during resource loading to avoid HeroEditor noise
            #if UNITY_EDITOR
            var previousLogType = Debug.unityLogger.filterLogType;
            Debug.unityLogger.filterLogType = LogType.Exception;
            #endif

            try
            {
                // Cache all cards
                var allCards = Resources.LoadAll<CardDataSO>("Data/Cards");
                foreach (var card in allCards)
                {
                    if (card != null && !string.IsNullOrEmpty(card.CardId))
                    {
                        _cardCache[card.CardId] = card;
                    }
                }

                // Cache all requiems
                var allRequiems = Resources.LoadAll<RequiemDataSO>("Data/Characters/Requiems");
                foreach (var requiem in allRequiems)
                {
                    if (requiem != null && !string.IsNullOrEmpty(requiem.RequiemId))
                    {
                        _requiemCache[requiem.RequiemId] = requiem;
                    }
                }
            }
            finally
            {
                #if UNITY_EDITOR
                Debug.unityLogger.filterLogType = previousLogType;
                #endif
            }

            Debug.Log($"[RunManager] Cached {_cardCache.Count} cards, {_requiemCache.Count} requiems");
        }

        // ============================================
        // Event Subscriptions
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<CardAddedToDeckEvent>(OnCardAddedToDeck);
            EventBus.Subscribe<CardRemovedFromDeckEvent>(OnCardRemovedFromDeck);
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<HealingReceivedEvent>(OnHealingReceived);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<MaxHPChangedEvent>(OnMaxHPChanged);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<CardAddedToDeckEvent>(OnCardAddedToDeck);
            EventBus.Unsubscribe<CardRemovedFromDeckEvent>(OnCardRemovedFromDeck);
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<MaxHPChangedEvent>(OnMaxHPChanged);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCardAddedToDeck(CardAddedToDeckEvent evt)
        {
            if (evt.Card != null && !_deck.Contains(evt.Card))
            {
                _deck.Add(evt.Card);
            }
        }

        private void OnCardRemovedFromDeck(CardRemovedFromDeckEvent evt)
        {
            if (evt.Card != null)
            {
                _deck.Remove(evt.Card);
            }
        }

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            _stats.DamageDealt += evt.Amount;
            if (evt.Amount > _stats.MaxSingleHitDamage)
            {
                _stats.MaxSingleHitDamage = evt.Amount;
            }
        }

        private void OnHealingReceived(HealingReceivedEvent evt)
        {
            _stats.HealingReceived += evt.Amount;
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            _stats.CardsPlayed++;
        }

        private void OnMaxHPChanged(MaxHPChangedEvent evt)
        {
            // Handle negative max HP changes from Echo events
            if (evt.Delta < 0)
            {
                DecreaseMaxHP(-evt.Delta);
            }
            // Positive changes are handled directly via IncreaseMaxHP
        }

        // ============================================
        // Run Management
        // ============================================

        /// <summary>
        /// Sets the Battle Mission context (zone and difficulty) before starting a run.
        /// Called by BattleMissionScreen when a zone is selected.
        /// </summary>
        /// <param name="zone">Zone number (1-3)</param>
        /// <param name="difficulty">Difficulty level</param>
        public void SetBattleMissionContext(int zone, DifficultyLevel difficulty)
        {
            _battleMissionZone = Mathf.Clamp(zone, 1, 3);
            _battleMissionDifficulty = difficulty;
            _currentZone = _battleMissionZone;
            _isBattleMissionRun = true;
            Debug.Log($"[RunManager] Battle Mission context set: Zone {_battleMissionZone}, {_battleMissionDifficulty}");
        }

        /// <summary>
        /// Clears the Battle Mission context (for regular runs).
        /// </summary>
        public void ClearBattleMissionContext()
        {
            _battleMissionZone = 1;
            _battleMissionDifficulty = DifficultyLevel.Easy;
            _isBattleMissionRun = false;
        }

        /// <summary>
        /// Gets the enemy stat multiplier based on current difficulty.
        /// </summary>
        public float GetDifficultyHealthMultiplier()
        {
            return _battleMissionDifficulty switch
            {
                DifficultyLevel.Easy => 1.0f,
                DifficultyLevel.Normal => 1.25f,
                DifficultyLevel.Hard => 1.5f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets the enemy ATK multiplier based on current difficulty.
        /// </summary>
        public float GetDifficultyAttackMultiplier()
        {
            return _battleMissionDifficulty switch
            {
                DifficultyLevel.Easy => 1.0f,
                DifficultyLevel.Normal => 1.1f,
                DifficultyLevel.Hard => 1.25f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Initialize a new run with the selected team.
        /// </summary>
        public void InitializeNewRun(List<RequiemDataSO> selectedTeam)
        {
            if (selectedTeam == null || selectedTeam.Count == 0)
            {
                Debug.LogError("[RunManager] Cannot start run without a team");
                return;
            }

            // Generate seed
            _runSeed = (int)System.DateTime.Now.Ticks;
            Random.InitState(_runSeed);

            // Clear previous run state - destroy old RequiemInstance GameObjects
            CleanupTeam();
            _deck.Clear();
            _upgradedCardIds.Clear();
            _stats = new StatsSaveData();
            // Clear cached map data to ensure fresh map generation
            _cachedMapData = null;
            // Preserve zone from Battle Mission context, otherwise default to 1
            _currentZone = _isBattleMissionRun ? _battleMissionZone : 1;
            _runStartTime = Time.time;

            // Reset VoidShards to 0 for new run (temporary run currency)
            if (ServiceLocator.TryGet<IShopManager>(out var shopManager))
            {
                shopManager.SetVoidShards(0);
                Debug.Log("[RunManager] VoidShards reset to 0 for new run");
            }

            // Initialize team
            _teamMaxHP = 0;
            foreach (var requiemData in selectedTeam)
            {
                var instance = CreateRequiemInstance(requiemData);
                if (instance != null)
                {
                    _team.Add(instance);
                    _teamMaxHP += requiemData.BaseHP;

                    // Add starting cards to deck
                    foreach (var card in requiemData.StartingCards)
                    {
                        if (card != null)
                        {
                            _deck.Add(card);
                        }
                    }
                }
            }

            _teamCurrentHP = _teamMaxHP;
            _soulEssence = 0; // Reset Soul Essence for new run
            _isRunActive = true;

            // Publish event
            EventBus.Publish(new RunStartedEvent(selectedTeam));

            Debug.Log($"[RunManager] New run initialized - Seed: {_runSeed}, Team: {_team.Count}, Deck: {_deck.Count} cards, HP: {_teamCurrentHP}/{_teamMaxHP}, SE: {_soulEssence}");
        }

        /// <summary>
        /// End the current run.
        /// </summary>
        public void EndRun(bool victory)
        {
            if (!_isRunActive)
            {
                Debug.LogWarning("[RunManager] No active run to end");
                return;
            }

            // Calculate final stats
            _stats.PlayTime = Time.time - _runStartTime;
            _stats.FloorsCleared = _currentZone;

            // Update meta progression
            UpdateMetaProgression(victory);

            // Delete run save
            if (ServiceLocator.TryGet<ISaveManager>(out var saveManager))
            {
                saveManager.DeleteRun();
            }

            // Clean up RequiemInstance GameObjects
            CleanupTeam();

            _isRunActive = false;

            // Clear Battle Mission context
            _isBattleMissionRun = false;

            // Clear cached map data so next run generates fresh map
            _cachedMapData = null;

            // Publish event
            EventBus.Publish(new RunEndedEvent(victory, _currentZone, _stats.EnemiesDefeated));

            Debug.Log($"[RunManager] Run ended - Victory: {victory}, Zone: {_currentZone}, Time: {_stats.PlayTime:F1}s");
        }

        /// <summary>
        /// Destroys all RequiemInstance GameObjects and clears the team list.
        /// Called when ending a run or before starting a new one.
        /// </summary>
        private void CleanupTeam()
        {
            foreach (var requiem in _team)
            {
                if (requiem != null && requiem.gameObject != null)
                {
                    Destroy(requiem.gameObject);
                }
            }
            _team.Clear();
            Debug.Log("[RunManager] Team cleaned up - RequiemInstance GameObjects destroyed");
        }

        private void UpdateMetaProgression(bool victory)
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var saveManager)) return;

            var meta = saveManager.LoadMeta();
            meta.TotalRunsStarted++;
            if (victory) meta.TotalRunsCompleted++;
            meta.TotalEnemiesDefeated += _stats.EnemiesDefeated;
            meta.TotalPlayTime += _stats.PlayTime;

            if (_currentZone > meta.HighestZoneReached)
            {
                meta.HighestZoneReached = _currentZone;
            }

            if (victory && (_stats.PlayTime < meta.BestRunTime || meta.BestRunTime <= 0))
            {
                meta.BestRunTime = _stats.PlayTime;
            }

            saveManager.SaveMeta(meta);
        }

        // ============================================
        // Save/Load
        // ============================================

        /// <summary>
        /// Save the current run state.
        /// </summary>
        public void SaveRun()
        {
            if (!_isRunActive)
            {
                Debug.LogWarning("[RunManager] No active run to save");
                return;
            }

            var saveData = new RunSaveData
            {
                RunSeed = _runSeed,
                Team = CreateTeamSaveData(),
                Deck = CreateDeckSaveData(),
                Progression = CreateProgressionSaveData(),
                Map = CreateMapSaveData(),
                Stats = _stats
            };

            if (ServiceLocator.TryGet<ISaveManager>(out var saveManager))
            {
                saveManager.SaveRun(saveData);
                Debug.Log("[RunManager] Run saved");
            }
        }

        /// <summary>
        /// Load a saved run state.
        /// </summary>
        public bool LoadRun()
        {
            var saveData = ServiceLocator.Get<ISaveManager>()?.LoadRun();
            if (saveData == null)
            {
                Debug.Log("[RunManager] No saved run to load");
                return false;
            }

            RestoreFromSaveData(saveData);
            Debug.Log("[RunManager] Run loaded successfully");
            return true;
        }

        // ============================================
        // Save Data Creation
        // ============================================

        private TeamSaveData CreateTeamSaveData()
        {
            var data = new TeamSaveData
            {
                TeamCurrentHP = _teamCurrentHP,
                TeamMaxHP = _teamMaxHP,
                SharedSoulEssence = _soulEssence
            };

            foreach (var requiem in _team)
            {
                data.RequiemIds.Add(requiem.Data.RequiemId);
                data.CurrentHP.Add(requiem.CurrentHP);
                data.MaxHP.Add(requiem.MaxHP);
                data.Corruption.Add(requiem.Corruption);
                data.SoulEssence.Add(requiem.SoulEssence);
            }

            return data;
        }

        private DeckSaveData CreateDeckSaveData()
        {
            return new DeckSaveData
            {
                CardIds = _deck.Select(c => c.CardId).ToList(),
                UpgradedCardIds = _upgradedCardIds.ToList()
            };
        }

        private ProgressionSaveData CreateProgressionSaveData()
        {
            ServiceLocator.TryGet<IShopManager>(out var shopManager);
            ServiceLocator.TryGet<IRelicManager>(out var relicManager);

            return new ProgressionSaveData
            {
                CurrentZone = _currentZone,
                VoidShards = shopManager?.VoidShards ?? 0,
                RelicIds = relicManager?.GetRelicIds() ?? new List<string>(),
                IsBattleMissionRun = _isBattleMissionRun,
                BattleMissionZone = _battleMissionZone,
                BattleMissionDifficulty = _battleMissionDifficulty
            };
        }

        private MapSaveData CreateMapSaveData()
        {
            // Get actual map data from MapManager if available (when in NullRift scene)
            if (ServiceLocator.TryGet<MapManager>(out var mapManager) && mapManager.HasActiveMap)
            {
                var mapSaveData = new MapSaveData
                {
                    Zone = _currentZone,
                    Seed = _runSeed
                };

                var mapData = mapManager.CurrentMap;
                mapSaveData.CurrentNodeId = mapData.CurrentNodeId;

                Debug.Log($"[RunManager] CreateMapSaveData: MapManager found, CurrentNodeId={mapData.CurrentNodeId}, TotalNodes={mapData.Nodes.Count}");

                // Save visited nodes
                foreach (var node in mapData.Nodes)
                {
                    if (node.State == NodeState.Visited || node.State == NodeState.Current)
                    {
                        mapSaveData.VisitedNodes.Add(new VisitedNode
                        {
                            NodeId = node.NodeId,
                            Completed = node.State == NodeState.Visited,
                            NodeType = node.Type.ToString()
                        });
                        Debug.Log($"[RunManager] Saving node: {node.NodeId}, State={node.State}, Completed={node.State == NodeState.Visited}");
                    }

                    if (node.State == NodeState.Available)
                    {
                        mapSaveData.AccessibleNodeIds.Add(node.NodeId);
                    }
                }

                Debug.Log($"[RunManager] Saved map state: CurrentNode={mapSaveData.CurrentNodeId}, Visited={mapSaveData.VisitedNodes.Count}, Accessible={mapSaveData.AccessibleNodeIds.Count}");
                return mapSaveData;
            }

            // Fall back to cached map data (e.g., when saving from Combat scene where MapManager doesn't exist)
            if (_cachedMapData != null)
            {
                Debug.Log($"[RunManager] CreateMapSaveData: Using cached map data, CurrentNode={_cachedMapData.CurrentNodeId}, Visited={_cachedMapData.VisitedNodes?.Count ?? 0}");
                return _cachedMapData;
            }

            // No map data available at all
            Debug.LogWarning("[RunManager] CreateMapSaveData: No MapManager and no cached map data available!");
            return new MapSaveData
            {
                Zone = _currentZone,
                Seed = _runSeed
            };
        }

        /// <summary>
        /// Gets the cached map save data for restoration.
        /// </summary>
        public MapSaveData GetCachedMapData() => _cachedMapData;

        /// <summary>
        /// Checks if the last combat node was a Boss node.
        /// Used to determine if zone should be marked cleared.
        /// </summary>
        public bool WasLastCombatBossNode()
        {
            if (_cachedMapData == null || string.IsNullOrEmpty(_cachedMapData.CurrentNodeId))
                return false;

            var currentNodeEntry = _cachedMapData.VisitedNodes.Find(
                v => v.NodeId == _cachedMapData.CurrentNodeId);

            return currentNodeEntry?.NodeType == "Boss";
        }

        /// <summary>
        /// Caches the current map state for cross-scene persistence.
        /// Called before transitioning to combat.
        /// </summary>
        public void CacheMapState()
        {
            _cachedMapData = CreateMapSaveData();
            Debug.Log($"[RunManager] Map state cached: {_cachedMapData.CurrentNodeId}");
        }

        // Cached map data for cross-scene persistence
        private MapSaveData _cachedMapData;

        // ============================================
        // Save Data Restoration
        // ============================================

        private void RestoreFromSaveData(RunSaveData saveData)
        {
            _runSeed = saveData.RunSeed;
            Random.InitState(_runSeed);

            // Restore team - clean up any existing RequiemInstance GameObjects first
            CleanupTeam();
            _teamCurrentHP = saveData.Team.TeamCurrentHP;
            _teamMaxHP = saveData.Team.TeamMaxHP;
            _soulEssence = saveData.Team.SharedSoulEssence;

            for (int i = 0; i < saveData.Team.RequiemIds.Count; i++)
            {
                var requiemId = saveData.Team.RequiemIds[i];
                if (_requiemCache.TryGetValue(requiemId, out var requiemData))
                {
                    var instance = CreateRequiemInstance(requiemData);
                    if (instance != null)
                    {
                        instance.SetHP(saveData.Team.CurrentHP[i], saveData.Team.MaxHP[i]);
                        instance.SetCorruption(saveData.Team.Corruption[i]);
                        instance.SetSoulEssence(saveData.Team.SoulEssence[i]);
                        _team.Add(instance);
                    }
                }
            }

            // Restore deck
            _deck.Clear();
            foreach (var cardId in saveData.Deck.CardIds)
            {
                if (_cardCache.TryGetValue(cardId, out var card))
                {
                    _deck.Add(card);
                }
            }

            _upgradedCardIds = new HashSet<string>(saveData.Deck.UpgradedCardIds);

            // Restore progression
            _currentZone = saveData.Progression.CurrentZone;

            // Restore void shards (if ShopManager is available)
            if (ServiceLocator.TryGet<IShopManager>(out var shopManager))
            {
                shopManager.SetVoidShards(saveData.Progression.VoidShards);
            }
            else
            {
                Debug.Log("[RunManager] IShopManager not available during restore - void shards will be set when ShopManager initializes");
            }

            // Restore relics (if RelicManager is available)
            if (ServiceLocator.TryGet<IRelicManager>(out var relicManager))
            {
                relicManager.LoadRelics(saveData.Progression.RelicIds);
            }
            else
            {
                Debug.Log("[RunManager] IRelicManager not available during restore - relics will be loaded when RelicManager initializes");
            }

            // Restore Battle Mission context
            _isBattleMissionRun = saveData.Progression.IsBattleMissionRun;
            _battleMissionZone = saveData.Progression.BattleMissionZone;
            _battleMissionDifficulty = saveData.Progression.BattleMissionDifficulty;
            Debug.Log($"[RunManager] Restored Battle Mission context: IsBM={_isBattleMissionRun}, Zone={_battleMissionZone}, Diff={_battleMissionDifficulty}");

            // Restore cached map data for cross-scene persistence
            _cachedMapData = saveData.Map;
            Debug.Log($"[RunManager] Restored cached map data: CurrentNode={_cachedMapData?.CurrentNodeId}, Visited={_cachedMapData?.VisitedNodes?.Count ?? 0}");

            // Restore stats
            _stats = saveData.Stats;

            _isRunActive = true;
            _runStartTime = Time.time - _stats.PlayTime;
        }

        private RequiemInstance CreateRequiemInstance(RequiemDataSO data)
        {
            // Find or create a RequiemInstance GameObject
            var go = new GameObject($"Requiem_{data.RequiemName}");
            go.transform.SetParent(transform);
            var instance = go.AddComponent<RequiemInstance>();
            instance.Initialize(data);
            return instance;
        }

        // ============================================
        // Deck Management
        // ============================================

        /// <summary>
        /// Add a card to the deck.
        /// </summary>
        public void AddCardToDeck(CardDataSO card)
        {
            if (card == null) return;

            _deck.Add(card);
            Debug.Log($"[RunManager] Added card to deck: {card.CardName}");
        }

        /// <summary>
        /// Remove a card from the deck.
        /// </summary>
        public void RemoveCardFromDeck(CardDataSO card)
        {
            if (card == null) return;

            if (_deck.Remove(card))
            {
                Debug.Log($"[RunManager] Removed card from deck: {card.CardName}");
            }
        }

        /// <summary>
        /// Mark a card as upgraded.
        /// </summary>
        public void UpgradeCard(CardDataSO card)
        {
            if (card == null) return;

            _upgradedCardIds.Add(card.CardId);
            Debug.Log($"[RunManager] Upgraded card: {card.CardName}");
        }

        /// <summary>
        /// Check if a card is upgraded.
        /// </summary>
        public bool IsCardUpgraded(string cardId)
        {
            return _upgradedCardIds.Contains(cardId);
        }

        // ============================================
        // Team HP Management
        // ============================================

        /// <summary>
        /// Heal the team.
        /// </summary>
        public void HealTeam(int amount)
        {
            if (amount <= 0) return;

            int oldHP = _teamCurrentHP;
            _teamCurrentHP = Mathf.Min(_teamCurrentHP + amount, _teamMaxHP);
            int actualHeal = _teamCurrentHP - oldHP;

            if (actualHeal > 0)
            {
                _stats.HealingReceived += actualHeal;
                EventBus.Publish(new TeamHPChangedEvent(_teamCurrentHP, _teamMaxHP, actualHeal));
                Debug.Log($"[RunManager] Team healed: {oldHP} → {_teamCurrentHP} (+{actualHeal})");
            }
        }

        /// <summary>
        /// Damage the team.
        /// </summary>
        public void DamageTeam(int amount)
        {
            if (amount <= 0) return;

            int oldHP = _teamCurrentHP;
            _teamCurrentHP = Mathf.Max(0, _teamCurrentHP - amount);
            int actualDamage = oldHP - _teamCurrentHP;

            _stats.DamageTaken += actualDamage;
            EventBus.Publish(new TeamHPChangedEvent(_teamCurrentHP, _teamMaxHP, -actualDamage));
            Debug.Log($"[RunManager] Team damaged: {oldHP} → {_teamCurrentHP} (-{actualDamage})");

            if (_teamCurrentHP <= 0)
            {
                EndRun(false);
            }
        }

        /// <summary>
        /// Increase max HP.
        /// </summary>
        public void IncreaseMaxHP(int amount)
        {
            if (amount <= 0) return;

            _teamMaxHP += amount;
            _teamCurrentHP += amount;
            EventBus.Publish(new TeamHPChangedEvent(_teamCurrentHP, _teamMaxHP, amount));
            Debug.Log($"[RunManager] Max HP increased by {amount}. New max: {_teamMaxHP}, Current: {_teamCurrentHP}");
        }

        /// <summary>
        /// Decrease max HP (e.g., from Echo event curses).
        /// Current HP is clamped to new max.
        /// </summary>
        public void DecreaseMaxHP(int amount)
        {
            if (amount <= 0) return;

            int oldMax = _teamMaxHP;
            int oldCurrent = _teamCurrentHP;
            _teamMaxHP = Mathf.Max(1, _teamMaxHP - amount); // Minimum 1 max HP
            _teamCurrentHP = Mathf.Min(_teamCurrentHP, _teamMaxHP);
            int hpLost = oldCurrent - _teamCurrentHP;

            EventBus.Publish(new TeamHPChangedEvent(_teamCurrentHP, _teamMaxHP, -hpLost));
            Debug.Log($"[RunManager] Max HP decreased by {amount}. {oldMax} → {_teamMaxHP}, Current: {_teamCurrentHP}");

            if (_teamCurrentHP <= 0)
            {
                EndRun(false);
            }
        }

        /// <summary>
        /// Advance to the next zone.
        /// </summary>
        public void AdvanceZone()
        {
            _currentZone++;
            _stats.FloorsCleared++;
            Debug.Log($"[RunManager] Advanced to Zone {_currentZone}");
        }

        /// <summary>
        /// Record an enemy defeat.
        /// </summary>
        public void RecordEnemyDefeated()
        {
            _stats.EnemiesDefeated++;
        }

        // ============================================
        // Team Corruption Management
        // ============================================

        /// <summary>
        /// Add corruption to all team members.
        /// Used by Echo events and other non-combat sources.
        /// </summary>
        public void AddTeamCorruption(int amount)
        {
            if (amount <= 0) return;

            foreach (var requiem in _team)
            {
                if (requiem != null && !requiem.IsDead)
                {
                    int previousCorruption = requiem.Corruption;
                    requiem.AddCorruption(amount);
                    EventBus.Publish(new CorruptionChangedEvent(requiem, previousCorruption, requiem.Corruption));
                    Debug.Log($"[RunManager] {requiem.Name} corruption: {previousCorruption} → {requiem.Corruption} (+{amount})");
                }
            }
        }

        /// <summary>
        /// Remove corruption from all team members.
        /// Used by Echo events and sanctuaries.
        /// </summary>
        public void RemoveTeamCorruption(int amount)
        {
            if (amount <= 0) return;

            foreach (var requiem in _team)
            {
                if (requiem != null && !requiem.IsDead)
                {
                    int previousCorruption = requiem.Corruption;
                    requiem.RemoveCorruption(amount);
                    EventBus.Publish(new CorruptionChangedEvent(requiem, previousCorruption, requiem.Corruption));
                    Debug.Log($"[RunManager] {requiem.Name} corruption: {previousCorruption} → {requiem.Corruption} (-{amount})");
                }
            }
        }

        // ============================================
        // Soul Essence Management
        // ============================================

        /// <summary>
        /// Add Soul Essence to the shared pool.
        /// Persists across combats during a run.
        /// </summary>
        public void AddSoulEssence(int amount)
        {
            if (amount <= 0) return;

            int previous = _soulEssence;
            _soulEssence += amount;
            EventBus.Publish(new SoulEssenceChangedEvent(_soulEssence, previous));
            Debug.Log($"[RunManager] Soul Essence: {previous} → {_soulEssence} (+{amount})");
        }

        /// <summary>
        /// Spend Soul Essence from the shared pool.
        /// </summary>
        /// <returns>True if enough SE was available and spent.</returns>
        public bool SpendSoulEssence(int amount)
        {
            if (amount <= 0) return true;
            if (_soulEssence < amount) return false;

            int previous = _soulEssence;
            _soulEssence -= amount;
            EventBus.Publish(new SoulEssenceChangedEvent(_soulEssence, previous));
            Debug.Log($"[RunManager] Soul Essence: {previous} → {_soulEssence} (-{amount})");
            return true;
        }

        /// <summary>
        /// Set Soul Essence directly (for save/load).
        /// </summary>
        public void SetSoulEssence(int amount)
        {
            int previous = _soulEssence;
            _soulEssence = Mathf.Max(0, amount);
            if (_soulEssence != previous)
            {
                EventBus.Publish(new SoulEssenceChangedEvent(_soulEssence, previous));
                Debug.Log($"[RunManager] Soul Essence set: {previous} → {_soulEssence}");
            }
        }
    }
}
