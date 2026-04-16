# Abilities

**Internal plugin** — ECS-based ability system with CommandSystem integration.

## What It Is

An ability system for Godot games using Friflo ECS. Provides:
- Ability definitions (metadata: cost, cooldown, range, effects)
- Ability execution pipeline (validation → resource check → targeting → execute)
- ECS components for runtime state (cooldowns, mana, ability book)
- Integration with Framework's slot system for hotbar UI

## Core Concepts

### AbilityDefinition
Metadata for an ability. Implements `IAbilityDefinition` (local to this plugin).

```csharp
AbilityDefinition fireball = new(
    id: "fireball",
    name: "Fireball",
    manaCost: 30f,
    baseCooldownSeconds: 5f,
    kind: AbilityKind.Damage,
    requiresTarget: true,
    range: 10f
);
```

### AbilityBookComponent (ECS)
Tracks which abilities an entity knows.

```csharp
entity.AddComponent(new AbilityBookComponent());
entity.GetComponent<AbilityBookComponent>().LearnAbility("fireball");
```

### IActionSlot Integration
Abilities implement `IHasEffects` (Framework) and can be placed in Framework's `IActionSlot`:

```csharp
// Framework slot
IActionSlot slot = new ActionSlot(index: 0, content: content, executor: executor);

// Content wraps IHasEffects
ActionContent content = ActionContent.FromEffect(abilityEffect);
```

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Framework                               │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────┐  │
│  │ IActionSlot │  │IHasEffects │  │ IEffectDefinition │  │
│  │ (slot UI)  │  │(items,etc) │  │ (damage, heal)   │  │
│  └─────────────┘  └─────────────┘  └──────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │ implements
┌─────────────────────────────────────────────────────────────┐
│                   Abilities Plugin                          │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ IAbilityDefinition (local)                           │  │
│  │ + ManaCost, Cooldown, Range, Kind, SlotKind       │  │
│  └─────────────────────────────────────────────────────┘  │
│  ┌─────────────────┐  ┌─────────────────────────────────┐ │
│  │ AbilityDefinition │  │ AbilityExecutor                 │ │
│  │ : IAbilityDefinition │  │ Execute(effect, context)      │ │
│  │ + Effects[]      │  └─────────────────────────────────┘ │
│  └─────────────────┘                                      │
│  ┌─────────────────────────────────────────────────────┐ │
│  │ AbilityCommandHandler                               │ │
│  │ Validates → Checks Resources → Executes            │ │
│  └─────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │ ECS Components                                      │ │
│  │ AbilityBookComponent | ManaComponent | Cooldown...  │ │
│  └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## Components

| Component | Purpose |
|-----------|---------|
| `AbilityBookComponent` | Tracks learned abilities for an entity |
| `ManaComponent` | Resource pool for ability costs |
| `AbilityCooldownComponent` | Tracks cooldown state per ability |
| `CooldownReductionComponent` | Modifies cooldown duration |
| `CanCastAbilitiesTag` | Marks entity as able to cast |
| `LearningAbilityTag` | Marks entity currently learning |
| `OnCooldownTag` | Marks entity on cooldown |

---

## Systems

| System | Update Frequency |
|--------|-------------|
| `AbilityCooldownSystem` | Per-frame |
| `ManaRegenerationSystem` | Per-frame |

---

## Effects

Effects implement Framework's `IEffectDefinition`:

| Effect | Class | Purpose |
|--------|-------|---------|
| Damage | `DamageAbilityEffect` | Deal damage to target |
| Heal | `HealAbilityEffect` | Restore health |
| Buff | `BuffAbilityEffect` | Apply beneficial status |
| Debuff | `DebuffAbilityEffect` | Apply harmful status |
| Utility | `UtilityAbilityEffect` | Non-combat effects |
| Summon | `SummonAbilityEffect` | Spawn entities |

---

## Dependency Graph

```
Abilities
├── MoonBark.Framework
│   ├── Effects (IEffectDefinition, EffectContext)
│   ├── Commands (ICommand, TargetId)
│   ├── Targeting (ITargetingValidator)
│   └── Slots (IActionSlot, IHasEffects)
├── Friflo.Engine.ECS
└── EntityTargetingSystem
```

---

## Design Decisions

### Why Local IAbilityDefinition?

The Framework provides `IHasEffects` for slot content. The Abilities plugin provides `IAbilityDefinition` for ability-specific metadata (ManaCost, Cooldown, Range). This separation keeps the Framework game-agnostic while Abilities owns mana/cooldown-specific logic.

### Resource Model

Currently uses `ManaComponent`. For games with different resources (stamina, energy, rage):
1. Add new resource components
2. Modify `AbilityCommandHandler.CheckMana()` to check the appropriate resource
3. The pattern scales to multiple resource types

---

## Status

**Internal plugin** — Not part of the public SDK. Owned by MoonBark Studio.
