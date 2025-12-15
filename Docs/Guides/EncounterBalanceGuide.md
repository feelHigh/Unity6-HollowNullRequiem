# Encounter Balance Guide

## Overview

This guide defines balance targets for enemy encounters across the 3 zones of Hollow Null Requiem's Null Rift dungeon.

## Zone Progression Philosophy

1. **Zone 1**: Learning zone - forgiving, teaches mechanics
2. **Zone 2**: Challenge zone - tests team synergy
3. **Zone 3**: Mastery zone - requires optimal play

---

## Enemy Stat Scaling

### Base Stats by Zone

| Zone | HP Range | Damage Range | Block | Corruption/Hit |
|------|----------|--------------|-------|----------------|
| 1 | 18-30 | 6-8 | 3-5 | 2-4 |
| 2 | 32-50 | 9-14 | 4-10 | 3-6 |
| 3 | 48-70 | 14-18 | 6-12 | 5-8 |
| Elite | 75-100 | 12-18 | 8-15 | 5-8 |
| Boss | 250-350 | 18-25 | 15-20 | 8-12 |

### Zone Scaling Multipliers

When enemies appear in higher zones than designed:
- **HP**: +15% per zone
- **Damage**: +10% per zone
- **Rewards**: +25% per zone

---

## Encounter Difficulty Tiers

### Easy (Zone 1 only)
- 1-2 basic enemies
- Total HP: 30-50
- Reward multiplier: 0.8-0.9x

### Normal (Zones 1-2)
- 2 enemies (may be mixed types)
- Total HP: 50-80
- Reward multiplier: 1.0x

### Hard (Zones 2-3)
- 2-3 enemies with synergy
- Total HP: 80-120
- Reward multiplier: 1.25-1.4x

### Elite (Zones 2-3)
- 1 Elite enemy OR 1 Elite + 1 basic
- Total HP: 75-120
- Guaranteed Uncommon card drop
- Reward multiplier: 1.5-1.6x

### Boss (Zone 3 only)
- 1 Boss enemy
- Total HP: 250-350
- Guaranteed Rare card drop
- Arena corruption: +2/turn
- Reward multiplier: 2.0x

---

## Enemy Roles

### Damage Dealers (High ATK, Low HP)
- Focus: Burst damage, must be prioritized
- Examples: Flame Wraith, Void Mage
- Balance: Kill before 3 turns or face heavy damage

### Tanks (High HP, High Block)
- Focus: Soak damage while allies attack
- Examples: Frost Sentinel, Null Knight
- Balance: Require piercing/debuffs to defeat efficiently

### Corruptors (High Corruption/Hit)
- Focus: Push team toward Null State
- Examples: Corruption Sprite, Null Weaver
- Balance: Cleansing or quick kills essential

### Buffers/Debuffers
- Focus: Enhance allies or weaken team
- Examples: Null Cultist
- Balance: Kill first or debuff

---

## Enemy Stat Block Reference

### Zone 1 Enemies

| Enemy | HP | DMG | Block | Corrupt | Reward | Aspect |
|-------|-----|-----|-------|---------|--------|--------|
| Hollow Thrall | 22 | 6 | 4 | 2 | 8 | None |
| Corruption Sprite | 18 | 5 | 3 | 4 | 10 | Shadow |
| Void Hound | 28 | 8 | 5 | 2 | 12 | None |

### Zone 2 Enemies

| Enemy | HP | DMG | Block | Corrupt | Reward | Aspect |
|-------|-----|-----|-------|---------|--------|--------|
| Null Cultist | 38 | 10 | 6 | 4 | 15 | Shadow |
| Flame Wraith | 32 | 12 | 4 | 3 | 18 | Flame |
| Frost Sentinel | 45 | 9 | 10 | 2 | 16 | Arcane |

### Zone 3 Enemies

| Enemy | HP | DMG | Block | Corrupt | Reward | Aspect |
|-------|-----|-----|-------|---------|--------|--------|
| Null Knight | 55 | 14 | 12 | 5 | 22 | Shadow |
| Void Mage | 48 | 16 | 6 | 6 | 25 | Arcane |

### Elite Enemies

| Enemy | HP | DMG | Block | Corrupt | Reward | Aspect |
|-------|-----|-----|-------|---------|--------|--------|
| Hollow Berserker | 85 | 18 | 8 | 5 | 40 | Flame |
| Null Weaver | 75 | 12 | 15 | 8 | 50 | Shadow |

### Boss

| Enemy | HP | DMG | Block | Corrupt | Reward | Aspect |
|-------|-----|-----|-------|---------|--------|--------|
| Malchor | 280 | 22 | 20 | 10 | 100 | Shadow |

---

## Encounter Distribution

### Zone 1 (5 encounters before Elite/Boss)
- 3 Easy encounters
- 2 Normal encounters
- 0 Elite (Zone 2+)
- 0 Boss (Zone 3)

### Zone 2 (5-6 encounters before Boss)
- 0 Easy encounters
- 2 Normal encounters
- 2 Hard encounters
- 1 Elite encounter (optional path)

### Zone 3 (4-5 encounters + Boss)
- 0 Easy/Normal encounters
- 2 Hard encounters
- 1 Elite encounter
- 1 Boss encounter (Malchor)

---

## Reward Formulas

### Void Shards
```
Base Reward × Zone Multiplier × Encounter Multiplier

Zone Multipliers: Zone 1 = 1.0, Zone 2 = 1.25, Zone 3 = 1.5
Encounter Multipliers: Easy = 0.8, Normal = 1.0, Hard = 1.3, Elite = 1.5, Boss = 2.0
```

### Card Drops
```
Base Drop Rate: 30%
Elite Bonus: +20% (50% total)
Boss: 100% (guaranteed)

Rarity Distribution:
- Normal: 60% Common, 30% Uncommon, 10% Rare
- Elite: 40% Common, 40% Uncommon, 20% Rare
- Boss: 50% Rare, 50% Legendary (guaranteed at least Rare)
```

---

## Balance Formulas

### Team vs Encounter Math

For a balanced encounter, team should:
1. **Survive 3 turns** minimum against max damage output
2. **Kill enemies in 4-6 turns** with average card draws
3. **Manage Corruption** below 75 by encounter end

### Example: Zone 2 Normal (Mixed Patrol)
```
Enemies: 2x Null Cultist (38 HP each = 76 HP total)
Enemy Damage: 10 × 2 = 20/turn

Team (avg):
- HP Pool: ~210 (70 per Requiem × 3)
- Damage/Turn: ~25-30 (with 3 AP)
- Block/Turn: ~15-20

Turns to Kill: 76 HP / 28 avg = ~3 turns
Damage Taken: 20 × 3 = 60 (manageable with block)
Corruption: 4 × 2 × 3 = 24 per encounter
```

---

## Testing Checklist

Run combat tests and verify:

- [ ] Zone 1 Easy encounters winnable without losing HP
- [ ] Zone 1 Normal encounters winnable with 80%+ HP remaining
- [ ] Zone 2 Normal encounters winnable with 60%+ HP remaining
- [ ] Zone 2 Hard encounters require strategic card play
- [ ] Elite encounters have risk of defeat without synergy
- [ ] Boss encounter requires full team coordination
- [ ] Corruption gain per zone stays under 50 average
- [ ] Void Shard economy allows 1-2 shop purchases per zone

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Dec 2024 | Initial encounter balance |
