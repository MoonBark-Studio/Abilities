# Abilities

A C# ECS plugin for managing abilities in Godot games using Friflo.Engine.ECS.

## Purpose

This plugin provides a complete ability system including ability definitions, registry, components, and execution pipeline. Abilities can have effects (heal, damage, buff, debuff, utility, summon) that are executed through a unified `IEffectExecutor` interface.

## Core Functionality

### 1. Ability Definitions — `IAbilityDefinition`

`AbilityDefinition` implements `MoonBark.Framework.Abilities.IAbilityDefinition`:
- `Id`, `Name` — ability identity
- `ManaCost`, `BaseCooldownSeconds` — resource costs
- `Kind` — Buff, Debuff, Heal, Damage, Utility, Summon
- `RequiresTarget`, `CanTargetSelf`, `Range` — targeting rules
- `Effects` — `IReadOnlyList<IEffectDefinition>` — what happens when ability is used

### 2. Ability Registry

`AbilityRegistry<IAbilityDefinition>` provides centralized ability management:
- Register ability definitions
- Retrieve abilities by ID
- Check if abilities exist

### 3. Ability Effects

Concrete effect types in `Core/Effects/`:
- `HealAbilityEffect` — healing with configurable amount
- `DamageAbilityEffect` — damage with type (physical, fire, ice, etc.)
- `BuffAbilityEffect` — beneficial status effects
- `DebuffAbilityEffect` — harmful status effects
- `UtilityAbilityEffect` — teleport, stealth, etc.
- `SummonAbilityEffect` — creature summoning

`AbilityExecutor` implements `IEffectExecutor` to execute effects.

### 4. Ability Components (ECS)

#### AbilityBookComponent
Tracks which abilities an entity knows:
- `KnowsAbility(abilityId)` — check if entity knows an ability
- `LearnAbility(abilityId)` — learn a new ability
- `ForgetAbility(abilityId)` — forget an ability

#### AbilityCooldownComponent
Tracks cooldown state for abilities:
- `IsOnCooldown` — check if ability is on cooldown
- `RemainingCooldownSeconds` — get remaining cooldown time
- `StartCooldown()` — start the cooldown
- `ReduceCooldown(deltaTime)` — reduce cooldown over time

#### ManaComponent
Manages mana for entities:
- `CurrentMana` — current mana value
- `MaxMana` — maximum mana value
- `ConsumeMana(amount)` — consume mana

### 5. Execution Pipeline

`AbilityExecutionPipeline` orchestrates full ability execution:
1. Validate command (caster can cast, knows ability, not learning)
2. Check mana
3. Check cooldown
4. Validate targeting
5. Execute ability
6. Apply effects via `IEffectExecutor`
7. Start cooldown

## Architecture

```
Abilities/
├── Core/
│   ├── AbilityDefinition.cs           # IAbilityDefinition implementation
│   ├── AbilityComponent.cs            # Runtime entity ability component
│   ├── AbilityRegistry.cs             # Centralized ability management
│   ├── AbilityBookComponent.cs        # Tracks known abilities
│   ├── AbilityComponents.cs           # Cooldown, mana, tags
│   ├── AbilitySystems.cs             # Cooldown and mana systems
│   ├── Effects/
│   │   ├── AbilityEffectDefinitions.cs # Heal, Damage, Buff, Debuff, etc.
│   │   └── AbilityExecutor.cs         # IEffectExecutor implementation
│   └── Execution/
│       ├── AbilityExecutionPipeline.cs
│       ├── AbilityCommandHandler.cs
│       ├── AbilityCommandTypes.cs
│       └── TargetingValidation.cs
├── Tests/
│   ├── Abilities.Tests.csproj
│   └── *.cs
└── Abilities.csproj
```

## Key Interfaces (Framework)

| Interface | Purpose |
|-----------|---------|
| `IAbilityDefinition` | Ability metadata (cost, cooldown, kind, targeting) |
| `IEffectDefinition` | Effect metadata (Id, Name, targeting rules) |
| `IEffectExecutor` | Executes an effect with `Execute(IEffectDefinition, EffectContext)` |
| `IHasEffects` | Exposes `IReadOnlyList<IEffectDefinition>` — unified for items, abilities, commands |

## Design Principles

- **Separation of Concerns** — `IAbilityDefinition` describes "can be used", `IEffectDefinition` describes "what happens"
- **Framework Integration** — Uses `MoonBark.Framework.Abilities.IAbilityDefinition` and `MoonBark.Framework.Effects`
- **Unified Execution** — `IEffectExecutor` handles effects from abilities, items, and commands uniformly
- **ECS Components** — Ability state stored in components, not on definition

## Integration

- **EntityTargetingSystem** — validates ability targeting
- **Items plugin** — `IItemData.Effects` uses same `IEffectDefinition` interface for unified hotbar

## Dependencies

- **MoonBark.Framework** — `IAbilityDefinition`, `IEffectDefinition`, `IEffectExecutor`
- **EntityTargetingSystem** — targeting validation
- **Friflo.Engine.ECS** — ECS framework

## Test Coverage

20 tests covering `AbilityDefinition`, `AbilityRegistry`, `AbilityExecutionPipeline`, `AbilityCommandHandler`, and effect types.

## Build Status

- Build: ✅ PASS
- Tests: ✅ 20 PASS