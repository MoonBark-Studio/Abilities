# Abilities

A modular ECS ability system for Godot C# games. Defines castable abilities with mana costs, cooldowns, and effect pipelines, integrating with the MoonBark Framework command bus.

## Key Features

- **Ability Definitions** — Immutable `AbilityDefinition` objects describing mana cost, cooldown, range, targeting rules, and attached effects.
- **Ability Registry** — Central `AbilityRegistry` provides a single source of truth for all ability definitions.
- **ECS Components** — `AbilityComponent`, `AbilityBookComponent`, `ManaComponent`, and `AbilityCooldownComponent` attach abilities directly to entities.
- **Cooldown & Mana Systems** — `AbilityCooldownSystem` and `ManaRegenerationSystem` manage runtime ability constraints.
- **Command Integration** — `AbilityCommandHandler` validates and executes abilities through the Framework `CommandEventBus`, checking caster eligibility, mana, and cooldowns.
- **Targeting Abstraction** — The plugin exposes capability (range, `RequiresTarget`, `CanTargetSelf`); actual targeting logic lives in the game layer.

## Architecture

```
Abilities/
├── Core/
│   ├── AbilitiesModule.cs           # Framework module registration
│   ├── AbilityDefinition.cs         # Immutable ability definition
│   ├── AbilityRegistry.cs           # Central definition registry
│   └── Execution/
│       ├── AbilityCommandHandler.cs # Command bus integration
│       ├── AbilityCommandTypes.cs   # Commands & results
│       └── TargetingResult.cs       # Targeting result types
├── ECS/
│   ├── AbilityComponent.cs          # Runtime ability on entity
│   ├── AbilityBookComponent.cs      # Known abilities for caster
│   ├── AbilityComponents.cs         # Mana, cooldown, tags
│   └── AbilitySystems.cs            # Cooldown & mana regen systems
├── Tests/
│   ├── AbilityDefinitionTests.cs
│   ├── AbilityRegistryTests.cs
│   ├── AbilityComponentTests.cs
│   └── AbilityExecutionTests/
└── docs/
    └── migration.md
```

## Dependencies

- **.NET 8.0**
- **Friflo.Engine.ECS** 3.6.0
- **MoonBark.Framework** (project reference)

## Usage Example

```csharp
using MoonBark.Abilities.Core;
using MoonBark.Abilities.ECS;
using MoonBark.Framework.Core;
using Friflo.Engine.ECS;

// 1. Register abilities
var registry = new AbilityRegistry();
registry.Register(new AbilityDefinition(
    id: "fireball",
    name: "Fireball",
    manaCost: 20f,
    baseCooldownSeconds: 5f,
    kind: AbilityKind.Offensive,
    requiresTarget: true,
    range: 10f
));

// 2. Create an entity that can cast
var entity = world.CreateEntity();
entity.AddComponent(new AbilityBookComponent());
entity.AddComponent(new ManaComponent(maxMana: 100f));
entity.AddComponent(new AbilityCooldownComponent("fireball", 5f));
entity.AddComponent(new CanCastAbilitiesTag());

// 3. Wire systems
var cooldownSystem = new AbilityCooldownSystem();
var manaRegenSystem = new ManaRegenerationSystem();
var module = new AbilitiesModule(registry, cooldownSystem, manaRegenSystem);
module.ConfigureServices(services);

// 4. Execute via command handler
var handler = new AbilityCommandHandler(registry);
var command = new AbilityCommand("fireball", AbilityAction.Cast);
var result = handler.Handle(command, world, entity);
```

## Status

- ✅ Audited: 2026-04-18
