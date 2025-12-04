// ============================================
// IRunManager.cs
// Run state management interface
// ============================================

using System.Collections.Generic;
using HNR.Cards;
using HNR.Characters;

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Run state management service.
    /// Handles team, deck, and run progression state.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: RunManager (MonoBehaviour)
    /// </remarks>
    public interface IRunManager
    {
        /// <summary>
        /// Whether a run is currently active.
        /// </summary>
        bool IsRunActive { get; }

        /// <summary>
        /// Current run seed for deterministic generation.
        /// </summary>
        int RunSeed { get; }

        /// <summary>
        /// The player's team of Requiem instances.
        /// </summary>
        IReadOnlyList<RequiemInstance> Team { get; }

        /// <summary>
        /// Current team HP (shared pool).
        /// </summary>
        int TeamCurrentHP { get; }

        /// <summary>
        /// Maximum team HP (shared pool).
        /// </summary>
        int TeamMaxHP { get; }

        /// <summary>
        /// The player's current deck.
        /// </summary>
        IReadOnlyList<CardDataSO> Deck { get; }

        /// <summary>
        /// Current zone number (1-3).
        /// </summary>
        int CurrentZone { get; }

        /// <summary>
        /// Initialize a new run with the selected team.
        /// </summary>
        /// <param name="selectedTeam">List of Requiem data for the team.</param>
        void InitializeNewRun(List<RequiemDataSO> selectedTeam);

        /// <summary>
        /// End the current run.
        /// </summary>
        /// <param name="victory">Whether the run was won.</param>
        void EndRun(bool victory);

        /// <summary>
        /// Save the current run state.
        /// </summary>
        void SaveRun();

        /// <summary>
        /// Load a saved run state.
        /// </summary>
        /// <returns>True if load was successful.</returns>
        bool LoadRun();

        /// <summary>
        /// Add a card to the deck.
        /// </summary>
        /// <param name="card">Card to add.</param>
        void AddCardToDeck(CardDataSO card);

        /// <summary>
        /// Remove a card from the deck.
        /// </summary>
        /// <param name="card">Card to remove.</param>
        void RemoveCardFromDeck(CardDataSO card);

        /// <summary>
        /// Mark a card as upgraded.
        /// </summary>
        /// <param name="card">Card to upgrade.</param>
        void UpgradeCard(CardDataSO card);

        /// <summary>
        /// Check if a card is upgraded.
        /// </summary>
        /// <param name="cardId">Card ID to check.</param>
        /// <returns>True if card is upgraded.</returns>
        bool IsCardUpgraded(string cardId);

        /// <summary>
        /// Heal the team by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to heal.</param>
        void HealTeam(int amount);

        /// <summary>
        /// Damage the team by the specified amount.
        /// </summary>
        /// <param name="amount">Amount of damage.</param>
        void DamageTeam(int amount);

        /// <summary>
        /// Increase the team's maximum HP.
        /// </summary>
        /// <param name="amount">Amount to increase.</param>
        void IncreaseMaxHP(int amount);

        /// <summary>
        /// Advance to the next zone.
        /// </summary>
        void AdvanceZone();
    }
}
