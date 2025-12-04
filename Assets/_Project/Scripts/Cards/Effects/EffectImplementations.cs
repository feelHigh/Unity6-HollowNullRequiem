// ============================================
// EffectImplementations.cs
// Effect implementation registry and notes
// ============================================

// All effect implementations are now in separate files for better organization:
//
// COMBAT EFFECTS:
// - DamageEffect.cs: DamageEffect, DamageMultipleEffect
// - BlockEffect.cs: BlockEffect
// - HealEffect.cs: HealEffect, HealPercentEffect
// - ApplyStatusEffect.cs: ApplyStatusEffect, RemoveStatusEffect, ApplyStatusAllEnemiesEffect
//
// RESOURCE EFFECTS:
// - ResourceEffects.cs: GainAPEffect, GainSEEffect, CorruptionEffect
//
// CARD MANIPULATION:
// - CardManipulationEffects.cs: DrawCardsEffect, DiscardRandomEffect, ExhaustEffect, CopyCardEffect
//
// UTILITY:
// - AspectEffectiveness.cs: Soul Aspect damage multiplier calculations
//
// All effects are registered in CardExecutor.InitializeEffectHandlers()
