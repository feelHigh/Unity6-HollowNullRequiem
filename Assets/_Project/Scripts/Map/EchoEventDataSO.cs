// ============================================
// EchoEventDataSO.cs
// ScriptableObject for Echo (narrative) events
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HNR.Map
{
    /// <summary>
    /// Defines an Echo event - narrative encounters with choices and outcomes.
    /// Echo events provide story moments and player agency in the Null Rift.
    /// </summary>
    [CreateAssetMenu(fileName = "New Echo Event", menuName = "HNR/Echo Event")]
    public class EchoEventDataSO : ScriptableObject
    {
        // ============================================
        // Identity
        // ============================================

        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier for this event")]
        private string _eventId;

        [SerializeField, Tooltip("Display title")]
        private string _eventTitle;

        [SerializeField, TextArea(3, 6), Tooltip("Event narrative text")]
        private string _narrative;

        // ============================================
        // Visuals
        // ============================================

        [Header("Visuals")]
        [SerializeField, Tooltip("Background illustration for the event")]
        private Sprite _backgroundImage;

        // ============================================
        // Choices
        // ============================================

        [Header("Choices")]
        [SerializeField, Tooltip("Available choices for this event (2-4 recommended)")]
        private List<EchoChoice> _choices = new();

        // ============================================
        // Restrictions
        // ============================================

        [Header("Restrictions")]
        [SerializeField, Tooltip("Zones where this event can appear (empty = all)")]
        private List<int> _validZones = new();

        [SerializeField, Tooltip("Can only appear once per run")]
        private bool _uniquePerRun = true;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Unique event identifier.</summary>
        public string EventId => _eventId;

        /// <summary>Event display title.</summary>
        public string EventTitle => _eventTitle;

        /// <summary>Narrative description.</summary>
        public string Narrative => _narrative;

        /// <summary>Background illustration.</summary>
        public Sprite BackgroundImage => _backgroundImage;

        /// <summary>Available choices.</summary>
        public IReadOnlyList<EchoChoice> Choices => _choices;

        /// <summary>Zones this event can appear in.</summary>
        public IReadOnlyList<int> ValidZones => _validZones;

        /// <summary>Whether this event can only appear once per run.</summary>
        public bool UniquePerRun => _uniquePerRun;

        // ============================================
        // Methods
        // ============================================

        /// <summary>
        /// Checks if this event can appear in the specified zone.
        /// </summary>
        public bool CanAppearInZone(int zone)
        {
            if (_validZones.Count == 0) return true;
            return _validZones.Contains(zone);
        }

        // ============================================
        // Editor Validation
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_eventId))
            {
                _eventId = name.ToLower().Replace(" ", "_");
            }
        }
#endif
    }

    /// <summary>
    /// A single choice within an Echo event.
    /// Each choice can have multiple outcomes.
    /// </summary>
    [Serializable]
    public class EchoChoice
    {
        [SerializeField, Tooltip("Choice button text")]
        private string _choiceText;

        [SerializeField, TextArea(2, 4), Tooltip("Outcome description shown after choosing")]
        private string _outcomeText;

        [SerializeField, Tooltip("Effects applied when this choice is selected")]
        private List<EchoOutcome> _outcomes = new();

        /// <summary>Text displayed on the choice button.</summary>
        public string ChoiceText => _choiceText;

        /// <summary>Narrative text shown after selecting this choice.</summary>
        public string OutcomeText => _outcomeText;

        /// <summary>List of effects applied by this choice.</summary>
        public IReadOnlyList<EchoOutcome> Outcomes => _outcomes;
    }

    /// <summary>
    /// A single outcome effect from an Echo choice.
    /// </summary>
    [Serializable]
    public class EchoOutcome
    {
        [SerializeField, Tooltip("Type of effect")]
        private EchoOutcomeType _type;

        [SerializeField, Tooltip("Numeric value for the effect")]
        private int _value;

        [SerializeField, Tooltip("Card ID for card-related outcomes")]
        private string _cardId;

        [SerializeField, Tooltip("Relic ID for relic-related outcomes")]
        private string _relicId;

        /// <summary>Type of outcome effect.</summary>
        public EchoOutcomeType Type => _type;

        /// <summary>Numeric value (amount of gold, HP, corruption, etc.).</summary>
        public int Value => _value;

        /// <summary>Card ID for GainCard/RemoveCard/UpgradeCard outcomes.</summary>
        public string CardId => _cardId;

        /// <summary>Relic ID for GainRelic outcome.</summary>
        public string RelicId => _relicId;
    }

    /// <summary>
    /// Types of outcomes for Echo event choices.
    /// </summary>
    public enum EchoOutcomeType
    {
        /// <summary>No mechanical effect.</summary>
        None,

        /// <summary>Gain Void Shards (currency).</summary>
        GainGold,

        /// <summary>Lose Void Shards.</summary>
        LoseGold,

        /// <summary>Heal team HP.</summary>
        GainHP,

        /// <summary>Take damage.</summary>
        LoseHP,

        /// <summary>Gain a specific card.</summary>
        GainCard,

        /// <summary>Remove a card from deck.</summary>
        RemoveCard,

        /// <summary>Gain a specific relic.</summary>
        GainRelic,

        /// <summary>Gain corruption on team.</summary>
        GainCorruption,

        /// <summary>Reduce corruption on team.</summary>
        LoseCorruption,

        /// <summary>Upgrade a random card.</summary>
        UpgradeCard,

        /// <summary>Permanently increase max HP.</summary>
        GainMaxHP,

        /// <summary>Permanently decrease max HP.</summary>
        LoseMaxHP,

        /// <summary>Start a combat encounter.</summary>
        StartCombat,

        /// <summary>Gain a random card from pool.</summary>
        GainRandomCard,

        /// <summary>Gain a random relic from pool.</summary>
        GainRandomRelic
    }
}
