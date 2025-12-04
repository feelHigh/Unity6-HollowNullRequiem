// ============================================
// CardBalanceTest.cs
// Balance verification for card effectiveness
// ============================================

using UnityEngine;
using HNR.Cards;
using HNR.Characters;
using System.Collections.Generic;
using System.Linq;

namespace HNR.Testing
{
    /// <summary>
    /// Tests card balance to verify damage output, healing, and resource generation
    /// match GDD specifications for each Requiem role.
    /// Press 'B' at runtime to run balance tests.
    /// </summary>
    public class CardBalanceTest : MonoBehaviour
    {
        [Header("Requiems to Test")]
        [SerializeField] private RequiemDataSO[] _requiems;

        [Header("Shared Cards")]
        [SerializeField] private CardDataSO[] _sharedCards;

        [Header("Balance Targets")]
        [SerializeField] private float _strikerDamageTarget = 12f;
        [SerializeField] private float _tankBlockTarget = 15f;
        [SerializeField] private float _supportHealTarget = 10f;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
                RunBalanceTests();
        }

        public void RunBalanceTests()
        {
            Debug.Log("=== Card Balance Tests ===");
            Debug.Log($"Testing {_requiems?.Length ?? 0} Requiems, {_sharedCards?.Length ?? 0} Shared Cards\n");

            if (_requiems == null || _requiems.Length == 0)
            {
                Debug.LogWarning("[CardBalanceTest] No Requiems assigned! Assign RequiemDataSO assets in Inspector.");
                return;
            }

            // Test each Requiem
            foreach (var requiem in _requiems)
            {
                if (requiem == null) continue;

                Debug.Log($"--- {requiem.RequiemName} ({requiem.Class}) ---");
                TestRequiemStats(requiem);
                TestRequiemDamageOutput(requiem);
                TestRequiemDefense(requiem);
                TestRequiemUtility(requiem);
                TestRequiemHealing(requiem);
                TestCardCostDistribution(requiem);
                ValidateRoleBalance(requiem);
                Debug.Log("");
            }

            // Test shared cards
            TestSharedCards();

            // Cross-Requiem comparison
            TestTeamComposition();

            Debug.Log("=== Balance Tests Complete ===");
        }

        // ============================================
        // Requiem Stats
        // ============================================

        private void TestRequiemStats(RequiemDataSO requiem)
        {
            Debug.Log($"  Stats: HP={requiem.BaseHP}, ATK={requiem.BaseATK}, DEF={requiem.BaseDEF}, SE Rate={requiem.SERate}x");
            Debug.Log($"  Aspect: {requiem.SoulAspect}, Cards: {requiem.StartingCards.Count} starting, {requiem.UnlockableCards.Count} unlockable");
        }

        // ============================================
        // Damage Output
        // ============================================

        private void TestRequiemDamageOutput(RequiemDataSO requiem)
        {
            int totalDamage = 0;
            int damageCardCount = 0;
            int burnDamage = 0;

            foreach (var card in requiem.StartingCards)
            {
                int cardDamage = card.GetTotalDamage();
                if (cardDamage > 0)
                {
                    totalDamage += cardDamage;
                    damageCardCount++;
                }

                // Check for burn damage
                foreach (var effect in card.Effects)
                {
                    if (effect.EffectType == EffectType.ApplyBurn)
                        burnDamage += effect.Value * effect.Duration;
                }
            }

            float avgDamage = damageCardCount > 0 ? totalDamage / (float)damageCardCount : 0;
            string burnInfo = burnDamage > 0 ? $" (+{burnDamage} burn)" : "";
            Debug.Log($"  Damage: {totalDamage} total, {avgDamage:F1} avg per card ({damageCardCount} damage cards){burnInfo}");
        }

        // ============================================
        // Defense
        // ============================================

        private void TestRequiemDefense(RequiemDataSO requiem)
        {
            int totalBlock = 0;
            int blockCardCount = 0;

            foreach (var card in requiem.StartingCards)
            {
                int cardBlock = card.GetTotalBlock();
                if (cardBlock > 0)
                {
                    totalBlock += cardBlock;
                    blockCardCount++;
                }
            }

            float avgBlock = blockCardCount > 0 ? totalBlock / (float)blockCardCount : 0;
            Debug.Log($"  Block: {totalBlock} total, {avgBlock:F1} avg per card ({blockCardCount} block cards)");
        }

        // ============================================
        // Utility (Card Draw, AP Gain, SE Gain)
        // ============================================

        private void TestRequiemUtility(RequiemDataSO requiem)
        {
            int drawCards = 0;
            int apGain = 0;
            int seGain = 0;

            foreach (var card in requiem.StartingCards)
            {
                foreach (var effect in card.Effects)
                {
                    switch (effect.EffectType)
                    {
                        case EffectType.DrawCards:
                            drawCards += effect.Value;
                            break;
                        case EffectType.GainAP:
                            apGain += effect.Value;
                            break;
                        case EffectType.GainSE:
                            seGain += effect.Value;
                            break;
                    }
                }
            }

            Debug.Log($"  Utility: Draw {drawCards} cards, Gain {apGain} AP, Gain {seGain} SE");
        }

        // ============================================
        // Healing
        // ============================================

        private void TestRequiemHealing(RequiemDataSO requiem)
        {
            int totalHealing = 0;
            int healCardCount = 0;

            foreach (var card in requiem.StartingCards)
            {
                foreach (var effect in card.Effects)
                {
                    if (effect.EffectType == EffectType.Heal)
                    {
                        totalHealing += effect.Value;
                        healCardCount++;
                    }
                }
            }

            if (healCardCount > 0)
            {
                float avgHeal = totalHealing / (float)healCardCount;
                Debug.Log($"  Healing: {totalHealing} total, {avgHeal:F1} avg ({healCardCount} heal effects)");
            }
        }

        // ============================================
        // Cost Distribution
        // ============================================

        private void TestCardCostDistribution(RequiemDataSO requiem)
        {
            var costCounts = new Dictionary<int, int>();

            foreach (var card in requiem.StartingCards)
            {
                int cost = card.APCost;
                if (!costCounts.ContainsKey(cost))
                    costCounts[cost] = 0;
                costCounts[cost]++;
            }

            string distribution = string.Join(", ", costCounts.OrderBy(k => k.Key).Select(k => $"{k.Key}AP:{k.Value}"));
            Debug.Log($"  Cost Distribution: {distribution}");
        }

        // ============================================
        // Role Validation
        // ============================================

        private void ValidateRoleBalance(RequiemDataSO requiem)
        {
            var warnings = new List<string>();

            float avgDamage = GetAverageDamage(requiem);
            float avgBlock = GetAverageBlock(requiem);
            float totalHealing = GetTotalHealing(requiem);

            switch (requiem.Class)
            {
                case RequiemClass.Striker:
                    if (avgDamage < _strikerDamageTarget)
                        warnings.Add($"Striker damage ({avgDamage:F1}) below target ({_strikerDamageTarget})");
                    break;

                case RequiemClass.Tank:
                    if (avgBlock < _tankBlockTarget)
                        warnings.Add($"Tank block ({avgBlock:F1}) below target ({_tankBlockTarget})");
                    break;

                case RequiemClass.Support:
                    if (totalHealing < _supportHealTarget)
                        warnings.Add($"Support healing ({totalHealing}) below target ({_supportHealTarget})");
                    break;

                case RequiemClass.Controller:
                    // Controllers validated by debuff presence
                    bool hasDebuffs = requiem.StartingCards.Any(c =>
                        c.HasEffect(EffectType.ApplyWeakness) ||
                        c.HasEffect(EffectType.ApplyVulnerability) ||
                        c.HasEffect(EffectType.ApplyBurn) ||
                        c.HasEffect(EffectType.ApplyStun));
                    if (!hasDebuffs)
                        warnings.Add("Controller lacks debuff cards");
                    break;
            }

            // Check for playable starting deck
            bool hasLowCostCards = requiem.StartingCards.Any(c => c.APCost <= 1);
            if (!hasLowCostCards)
                warnings.Add("No 0-1 cost cards in starting deck");

            foreach (var warning in warnings)
                Debug.LogWarning($"  [BALANCE] {warning}");

            if (warnings.Count == 0)
                Debug.Log($"  [OK] {requiem.Class} role balance validated");
        }

        // ============================================
        // Shared Cards
        // ============================================

        private void TestSharedCards()
        {
            Debug.Log("--- Shared Cards ---");

            if (_sharedCards == null || _sharedCards.Length == 0)
            {
                Debug.LogWarning("[CardBalanceTest] No shared cards assigned!");
                return;
            }

            int totalDamage = 0;
            int totalBlock = 0;
            int totalHealing = 0;

            foreach (var card in _sharedCards)
            {
                if (card == null) continue;

                totalDamage += card.GetTotalDamage();
                totalBlock += card.GetTotalBlock();

                foreach (var effect in card.Effects)
                {
                    if (effect.EffectType == EffectType.Heal)
                        totalHealing += effect.Value;
                }
            }

            Debug.Log($"  Shared Cards: {_sharedCards.Length} available");
            Debug.Log($"  Total Damage: {totalDamage}, Block: {totalBlock}, Healing: {totalHealing}");

            // Verify neutral cards have no owner
            int ownedCards = _sharedCards.Count(c => c != null && c.Owner != null);
            if (ownedCards > 0)
                Debug.LogWarning($"  [BALANCE] {ownedCards} shared cards have owner assigned (should be null)");
            else
                Debug.Log($"  [OK] All shared cards are neutral (no owner)");
        }

        // ============================================
        // Team Composition
        // ============================================

        private void TestTeamComposition()
        {
            if (_requiems == null || _requiems.Length < 2) return;

            Debug.Log("--- Team Composition Analysis ---");

            int teamHP = _requiems.Where(r => r != null).Sum(r => r.BaseHP);
            int teamATK = _requiems.Where(r => r != null).Sum(r => r.BaseATK);
            int teamDEF = _requiems.Where(r => r != null).Sum(r => r.BaseDEF);
            int totalCards = _requiems.Where(r => r != null).Sum(r => r.StartingCards.Count);

            Debug.Log($"  Team Stats: HP={teamHP}, ATK={teamATK}, DEF={teamDEF}");
            Debug.Log($"  Starting Deck: {totalCards} cards");

            // Role coverage
            var classes = _requiems.Where(r => r != null).Select(r => r.Class).Distinct().ToList();
            Debug.Log($"  Role Coverage: {string.Join(", ", classes)} ({classes.Count}/4 roles)");

            // Aspect coverage
            var aspects = _requiems.Where(r => r != null).Select(r => r.SoulAspect).Distinct().ToList();
            Debug.Log($"  Aspect Coverage: {string.Join(", ", aspects)} ({aspects.Count}/4 aspects)");
        }

        // ============================================
        // Helper Methods
        // ============================================

        private float GetAverageDamage(RequiemDataSO requiem)
        {
            int total = 0;
            int count = 0;
            foreach (var card in requiem.StartingCards)
            {
                int dmg = card.GetTotalDamage();
                if (dmg > 0)
                {
                    total += dmg;
                    count++;
                }
            }
            return count > 0 ? total / (float)count : 0;
        }

        private float GetAverageBlock(RequiemDataSO requiem)
        {
            int total = 0;
            int count = 0;
            foreach (var card in requiem.StartingCards)
            {
                int blk = card.GetTotalBlock();
                if (blk > 0)
                {
                    total += blk;
                    count++;
                }
            }
            return count > 0 ? total / (float)count : 0;
        }

        private float GetTotalHealing(RequiemDataSO requiem)
        {
            int total = 0;
            foreach (var card in requiem.StartingCards)
            {
                foreach (var effect in card.Effects)
                {
                    if (effect.EffectType == EffectType.Heal)
                        total += effect.Value;
                }
            }
            return total;
        }
    }
}
