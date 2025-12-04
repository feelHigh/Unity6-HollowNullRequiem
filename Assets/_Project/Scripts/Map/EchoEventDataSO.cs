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

        // ============================================
        // Presentation
        // ============================================

        [Header("Presentation")]
        [SerializeField, TextArea(3, 8), Tooltip("Event narrative text")]
        private string _description;

        [SerializeField, Tooltip("Event illustration")]
        private Sprite _eventImage;

        // ============================================
        // Choices
        // ============================================

        [Header("Choices")]
        [SerializeField, Tooltip("Available choices for this event")]
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
        public string Description => _description;

        /// <summary>Event illustration.</summary>
        public Sprite EventImage => _eventImage;

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
    /// </summary>
    [Serializable]
    public class EchoChoice
    {
        [Tooltip("Choice button text")]
        public string ChoiceText;

        [TextArea(2, 4), Tooltip("Outcome description shown after choosing")]
        public string OutcomeText;

        [Tooltip("Type of reward/effect")]
        public EchoOutcomeType OutcomeType;

        [Tooltip("Value associated with the outcome")]
        public int OutcomeValue;

        [Tooltip("Optional: Specific card reward")]
        public HNR.Cards.CardDataSO CardReward;

        [Tooltip("Optional: Cost in HP to choose this option")]
        public int HPCost;

        [Tooltip("Optional: Cost in corruption to choose this option")]
        public int CorruptionCost;
    }

    /// <summary>
    /// Types of outcomes for Echo event choices.
    /// </summary>
    public enum EchoOutcomeType
    {
        /// <summary>No mechanical effect.</summary>
        None,

        /// <summary>Heal team HP.</summary>
        Heal,

        /// <summary>Take damage.</summary>
        Damage,

        /// <summary>Gain corruption.</summary>
        GainCorruption,

        /// <summary>Reduce corruption.</summary>
        ReduceCorruption,

        /// <summary>Gain Void Shards (currency).</summary>
        GainCurrency,

        /// <summary>Lose Void Shards.</summary>
        LoseCurrency,

        /// <summary>Gain a random card.</summary>
        GainRandomCard,

        /// <summary>Gain a specific card.</summary>
        GainSpecificCard,

        /// <summary>Remove a card from deck.</summary>
        RemoveCard,

        /// <summary>Upgrade a card.</summary>
        UpgradeCard,

        /// <summary>Gain a random relic.</summary>
        GainRelic,

        /// <summary>Start a combat encounter.</summary>
        Combat
    }
}
