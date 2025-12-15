# Card Balance Guide

## Overview

This guide defines balance targets for Hollow Null Requiem's card system. Use the CardBalanceTest scene (HNR > Generate Card Balance Test Scene, then press B) to verify balance.

## Balance Philosophy

1. **Role Identity** - Each Requiem should excel in their role
2. **Team Synergy** - Cards should encourage team composition
3. **Meaningful Choices** - No strictly better/worse cards at same cost
4. **Corruption Risk/Reward** - Power should scale with Corruption mechanics

---

## Role Targets (Starting Deck Averages)

| Role | Damage/Card | Block/Card | Healing | Utility |
|------|-------------|------------|---------|---------|
| **Striker** (Kira) | 12-15 | 5-8 | 0 | Draw 1, SE +5 |
| **Tank** (Thornwick) | 6-8 | 15-20 | 0-5 | Taunt, Thorns |
| **Support** (Elara) | 4-6 | 8-12 | 10-15 | Cleanse, Buff |
| **Controller** (Mordren) | 8-10 | 6-10 | 5-8 (lifesteal) | Debuffs, SE steal |

---

## AP Cost Guidelines

### 0 AP Cards
- Weak effects or strong downsides
- Draw +1, very conditional damage/block
- Example: "Spark" - Deal 3 damage

### 1 AP Cards (Bread & Butter)
- Standard efficiency baseline
- Damage: 6-8 | Block: 6-8 | Heal: 4-6
- Example: "Strike" - Deal 6 damage

### 2 AP Cards
- Enhanced single effects or multi-effect
- Damage: 12-15 | Block: 12-15 | Heal: 8-10
- Should be ~1.8x value of 1 AP cards
- Example: "Heavy Blow" - Deal 14 damage

### 3 AP Cards
- Powerful multi-target or combo effects
- AoE: 8-10 to all enemies
- Conditional: 20+ damage with setup
- Example: "Inferno" - Deal 8 damage to ALL enemies

---

## Damage Formulas

### Base Damage by Requiem
```
Kira (Striker):     BaseDamage × 1.2
Mordren (Controller): BaseDamage × 1.0
Elara (Support):    BaseDamage × 0.8
Thornwick (Tank):   BaseDamage × 0.9
```

### Status Effect Damage
```
Burn:    Stack × 1 per turn (burns through block)
Poison:  Stack × 1 per turn (reduces by 1 each tick)
```

### Corruption Bonus
```
At 50+ Corruption:  +10% damage
At 75+ Corruption:  +20% damage
In Null State (100): +50% damage (via RequiemArtExecutor)
```

---

## Block & Defense

### Block Efficiency
- 1 AP should grant 6-8 Block
- Tank cards grant 12-15 Block at 1 AP
- Block expires at turn end

### Damage Reduction
- Base DEF reduces damage by flat amount
- Vulnerability: +50% damage taken
- Weakness: -25% damage dealt

---

## Healing

### Healing Targets
- Elara's average heal: 10-12 per card
- Emergency heals: 15-20 (higher cost)
- Team heals: 6-8 per ally

### Corruption Cleanse
- Standard: -10 Corruption
- Strong: -20 Corruption (higher cost)
- Team cleanse: -5 Corruption per ally

---

## Card Rarity Scaling

| Rarity | Power Level | AP Efficiency |
|--------|-------------|---------------|
| Common | Baseline | 1.0x |
| Uncommon | +20% power | 1.2x |
| Rare | +40% power | 1.4x |
| Legendary | +60% power + unique effect | 1.6x |

---

## Requiem-Specific Balance

### Kira (Flame Striker)
- **Focus**: Single-target burst, Burn stacking
- **Signature**: Multi-hit attacks, self-buff, SE generation
- **Weakness**: Low defense, no healing
- **Art Cost**: 40 SE

### Mordren (Shadow Controller)
- **Focus**: Debuffs, lifesteal, manipulation
- **Signature**: Weakness/Vulnerability, SE drain, discard
- **Weakness**: Inconsistent damage, Corruption risk
- **Art Cost**: 35 SE

### Elara (Light Support)
- **Focus**: Healing, cleansing, protection
- **Signature**: Team heals, Corruption cleanse, buffs
- **Weakness**: Low damage, expensive cards
- **Art Cost**: 45 SE

### Thornwick (Nature Tank)
- **Focus**: Block, Thorns, survival
- **Signature**: High block, regeneration, crowd control
- **Weakness**: Slow, low utility
- **Art Cost**: 30 SE

---

## Shared Card Balance

Neutral cards should be:
- Generically useful (no aspect synergy)
- Slightly weaker than class-specific equivalents
- Good for filling deck gaps

| Card | Cost | Effect | Notes |
|------|------|--------|-------|
| Strike | 1 | 6 damage | Baseline attack |
| Defend | 1 | 5 block | Baseline defense |
| Quick Draw | 0 | Draw 1 | Cycle card |
| Desperate Strike | 1 | 10 damage, +10 Corruption | Risk/reward |
| Second Wind | 2 | Heal 8, Draw 1 | Recovery |

---

## Testing Checklist

Run CardBalanceTest and verify:

- [ ] All Strikers deal 12+ avg damage per damage card
- [ ] All Tanks block 15+ avg per block card
- [ ] All Supports heal 10+ total from starting deck
- [ ] All Controllers have 2+ debuff effects
- [ ] Each Requiem has at least one 0-1 AP card
- [ ] No shared cards have an Owner assigned
- [ ] Team of 3 has balanced role coverage

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Dec 2024 | Initial balance targets |
