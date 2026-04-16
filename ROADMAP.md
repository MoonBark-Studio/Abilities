# Abilities — Roadmap
Updated: 2026-04-16

## Overview

Ability system providing `IAbilityDefinition` (local) + `IEffectDefinition` (Framework.Effects) + `IEffectExecutor` (execution). ECS components for entity ability state.

## What's Done ✅

### Local Abstractions (2026-04-16)
- [x] Created `Core/Abilities.Abstractions.cs` with local `IAbilityDefinition`, `AbilityKind`, `AbilitySlotKind`
- [x] Removed dependency on `MoonBark.Framework.Abilities` (moved to local abstractions)
- [x] Updated all files to use local abstractions

### Framework Integration (2026-04-15)
- [x] `AbilityDefinition` implements `IAbilityDefinition`
- [x] `AbilityRegistry` stores `IAbilityDefinition` (was concrete type)
- [x] Added `Core/Effects/` with `AbilityEffectDefinition` base class + 6 concrete types (Heal, Damage, Buff, Debuff, Utility, Summon)
- [x] Created `AbilityExecutor : IEffectExecutor`
- [x] `AbilityExecutionPipeline` takes `IEffectExecutor` in constructor, calls `executor.Execute()` for each ability effect
- [x] `AbilityCommandHandler` uses `IAbilityDefinition` interface

### Interface Separation
- [x] `IAbilityDefinition` — "can be used" (mana, cooldown, kind, range, targeting)
- [x] `IEffectDefinition` — "what happens" (Id, Name, targeting metadata)
- [x] `IEffectExecutor` — "how it executes" (Execute(IEffectDefinition, EffectContext))

## What's Left

### P1 — Production Hardening
| Item | Status |
|------|--------|
| Real targeting validation (not stub) | P1 |
| Real effect application (not just event bus) | P1 |

### P2 — Feature Expansion
| Item | Status |
|------|--------|
| `AbilityTargetingBridge` integration | P2 |
| Multiple effects per ability | P2 |
| Effect parameters from ability context | P2 |

## Architecture Diagram

```
HotbarSlot → IHasEffects → IReadOnlyList<IEffectDefinition>
                                          ↑
                    ┌─────────────────────┼─────────────────────┐
                    │                     │                     │
            ItemDefinition         AbilityDefinition      CommandDefinition
            (Items plugin)         (Abilities plugin)     (CommandSystem)
                    │                     │                     │
                    └─────────────────────┴─────────────────────┘
                                          │
                                   IEffectExecutor
                                          |
                                  AbilityExecutor
                                    └─→ HealAbilityEffect
                                    └─→ DamageAbilityEffect
                                    └─→ BuffAbilityEffect
                                    └─→ ...
```

## Key Files

| File | Purpose |
|------|---------|
| `Core/Abilities.Abstractions.cs` | Local `IAbilityDefinition`, `AbilityKind`, `AbilitySlotKind` |
| `Core/AbilityDefinition.cs` | `IAbilityDefinition` implementation |
| `Core/AbilityRegistry.cs` | Stores `IAbilityDefinition` |
| `Core/Effects/AbilityEffectDefinitions.cs` | `AbilityEffectDefinition` + 6 concrete effects |
| `Core/Effects/AbilityExecutor.cs` | `IEffectExecutor` implementation |
| `Core/Execution/AbilityExecutionPipeline.cs` | Orchestrates execution with executor |
| `Core/Execution/AbilityCommandHandler.cs` | Command handler using interface |

## Dependencies

- `MoonBark.Framework.Effects` — `IEffectDefinition`, `IEffectExecutor`, `EffectContext`, `IHasEffects`
- `EntityTargetingSystem` — targeting validation
