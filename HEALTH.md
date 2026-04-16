# Abilities — Health

## Health Score: 75/100 🟡
**Status:** 🟡 **FAIR** (Updated 2026-04-16)

---

## Anti-Pattern Audit Findings

### ✅ RESOLVED (2026-04-16)
| Severity | File | Issue | Resolution |
|----------|------|-------|------------|
| MEDIUM | All files | Framework.Abilities dependency | Created `Core/Abilities.Abstractions.cs` with local abstractions |

### ✅ RESOLVED (2026-04-15)
| Severity | File | Issue | Resolution |
|----------|------|-------|------------|
| MEDIUM | `AbilityExecutionPipeline.cs` | DEAD CODE stub implementations | Now uses `IEffectExecutor` — `ApplyEffects` calls executor with ability effects |
| MEDIUM | `AbilityCommandHandler.cs` | DEAD CODE stub with NOTE | Now uses `IAbilityDefinition` interface |

### ⚠️ REMAINING Issues

| Severity | File | Issue |
|----------|------|-------|
| LOW | `Core/Execution/TargetingValidation.cs` | ValidateTargeting stub always returns success |

---

## Local Abstractions (2026-04-16)

```
Core/Abilities.Abstractions.cs
├── IAbilityDefinition     # Local interface (was Framework.Abilities)
├── AbilityKind            # Enum: Buff, Debuff, Heal, Damage, Utility, Summon
└── AbilitySlotKind        # Enum: Active, Passive, Toggle
```

### Why Local Abstractions?

`MoonBark.Framework.Abilities` was removed because:
- `IAbilityDefinition` contained game-specific `ManaCost`
- Framework should not contain game-specific ability concepts
- All ability abstractions now live in the Abilities plugin itself

---

## Framework Integration (2026-04-15)

| Interface | Implementation |
|-----------|---------------|
| `IAbilityDefinition` | `AbilityDefinition` in Core/ |
| `IEffectDefinition` | `AbilityEffectDefinition` + 6 concrete types in Core/Effects/ |
| `IEffectExecutor` | `AbilityExecutor` in Core/Effects/ |

### Current Structure

```
Core/Abilities.Abstractions.cs  # Local IAbilityDefinition, AbilityKind, AbilitySlotKind
Core/Effects/
├── AbilityEffectDefinitions.cs  # AbilityEffectDefinition base + 6 concrete effects
└── AbilityExecutor.cs          # IEffectExecutor implementation

Core/AbilityDefinition.cs        # Implements IAbilityDefinition
Core/AbilityRegistry.cs          # Stores IAbilityDefinition (was concrete AbilityDefinition)
Core/Execution/
├── AbilityExecutionPipeline.cs  # Takes IEffectExecutor, calls executor.Execute() for each effect
└── AbilityCommandHandler.cs     # Uses IAbilityDefinition interface
```

---

## Build & Tests

| Check | Status | Notes |
|-------|--------|-------|
| Build | ✅ PASS | Clean |
| Tests | ✅ 20 PASS | 0 failures |

---

## Architecture

```
IAbilityDefinition (Local.Abstractions)
├── Id, Name, ManaCost, Cooldown
├── Kind (Buff/Debuff/Heal/Damage/Utility/Summon)
├── RequiresTarget, CanTargetSelf, Range
└── Effects: IReadOnlyList<IEffectDefinition>

IEffectDefinition (Framework.Effects)
├── Id, Name, MaxTargets, CanTargetSelf
└── TargetRequirements

AbilityDefinition : IAbilityDefinition
└── Effects (List<IEffectDefinition>)

AbilityExecutor : IEffectExecutor
└── Execute(IEffectDefinition, EffectContext)

AbilityExecutionPipeline
├── Takes IEffectExecutor in constructor
└── ApplyEffects → iterates ability.Effects, calls executor.Execute()
```

---

## Known Issues

| Severity | Issue | Status |
|----------|-------|--------|
| LOW | TargetingValidation stub always returns success | P2 — deferred |

---

## Tech Debt

| Item | Priority | Status |
|------|----------|--------|
| Implement real targeting validation | P2 | Deferred |

---

*Last Updated: 2026-04-16*
