# Abilities

A modular ECS ability system for Godot C# games. Defines castable abilities with mana costs, cooldowns, and effect pipelines, integrating with the MoonBark Framework command bus.

## Architecture Overview

The plugin follows a **thin Godot wiring** principle:

```
Abilities/
├── Core/                          ← Pure C# business logic (no Godot deps)
│   ├── Abilities.Abstractions.cs  ← Interfaces & enums
│   ├── AbilityDefinition.cs       ← Immutable ability definitions
│   ├── AbilityRegistry.cs         ← Central definition registry
│   ├── AbilitiesModule.cs         ← Framework module registration
│   └── Execution/
│       ├── AbilityCommandHandler.cs  # Command validation & execution
│       ├── AbilityCommandTypes.cs    # Commands & results
│       └── TargetingResult.cs        # Targeting result types
│
├── Core/Effects/                  ← Effect definitions (damage, heal, etc.)
│   ├── DamageEffect.cs
│   ├── HealEffect.cs
│   ├── BuffEffect.cs
│   ├── DebuffEffect.cs
│   └── SummonEffect.cs
│
├── ECS/                           ← Frilox ECS components & systems
│   ├── AbilityComponent.cs        ← Runtime ability on entity
│   ├── AbilityBookComponent.cs    ← Known abilities for caster
│   ├── AbilityComponents.cs       ← Mana, cooldown, tags, resistances
│   ├── AbilitySystems.cs          ← Cooldown & mana regen systems
│   └── HealthComponent.cs         ← Health/HP tracking
│
├── godot/                         ← Godot integration (thin adapters)
│   ├── scripts/
│   │   ├── GodotAbilityNode.cs    ← Godot node wrapping ECS entity
│   │   ├── WorldManager.cs        ← ECS world composition root
│   │   ├── AbilitySignalBus.cs    ← Core events → Godot signals
│   │   ├── GodotEffectApplier.cs  ← Effect → Godot visual reactions
│   │   ├── AbilitiesPlugin.cs     ← Editor plugin
│   │   └── AbilityDemo.cs         ← Usage example
│   ├── addons/abilities/          ← Godot plugin
│   │   ├── plugin.cfg
│   │   └── AbilitiesPlugin.cs
│   └── scenes/
│       └── AbilityDemo.tscn       ← Demo scene
│
└── Tests/                         ← Pure C# unit tests (xUnit)
    ├── AbilityDefinitionTests.cs
    ├── AbilityRegistryTests.cs
    ├── AbilityComponentTests.cs
    └── AbilityExecutionTests/
```

## Key Principles

1. **Logic Lives in Core** — All ability logic (validation, cooldowns, mana costs, effect application) lives in `Core/`. Zero Godot dependencies.

2. **ECS is the Source of Truth** — Entity state is stored in Friflo ECS components. Godot nodes observe and react to component changes.

3. **Thin Godot Wiring** — The `godot/` layer provides:
   - `GodotAbilityNode`: Godot `Node3D` wrapping an ECS entity
   - `WorldManager`: Composition root managing the ECS world
   - `AbilitySignalBus`: Bridges Core events to Godot signals
   - `GodotEffectApplier`: Translates effect definitions to Godot visual reactions

4. **Signals Bridge the Gap** — Core events (component changes) are translated to Godot signals that UI can bind to.

## Dependencies

- **.NET 8.0**
- **Friflo.Engine.ECS** 3.6.0
- **MoonBark.Framework** (project reference)
- **Godot 4.4** (optional, for Godot integration)
  - `GodotSharp` 4.4.1
  - `Godot.SourceGenerators` 4.4.1

## Quick Start

### 1. Setup WorldManager

```csharp
// Create WorldManager (manages ECS world and systems)
var worldManager = new WorldManager(autoUpdateSystems: true, manaRegenRate: 5f);
AddChild(worldManager);
worldManager.InitializeWorld();
```

### 2. Register Abilities

```csharp
var registry = worldManager.Registry;

// Fireball: Offensive ability with damage
var fireballEffects = new IEffectDefinition[] {
    new DamageEffect(amount: 25f, isMagical: true)
};

var fireball = new AbilityDefinition(
    id: "fireball",
    name: "Fireball",
    manaCost: 20f,
    baseCooldownSeconds: 3f,
    kind: AbilityKind.Damage,
    requiresTarget: true,
    canTargetSelf: false,
    range: 15f,
    effects: fireballEffects
);

registry.Register(fireball);
```

### 3. Create Ability Entities

```csharp
// Method 1: Programmatic
var player = worldManager.CreateAbilityEntity("fireball", position: Vector3.Zero);

// Method 2: Scene-based (Godot)
var player = preload("res://scenes/Player.tscn").Instantiate<GodotAbilityNode>();
player.Initialize(world, registry, commandHandler);
AddChild(player);
```

### 4. Cast Abilities

```csharp
// From Godot node
player.CastAbility();

// Check if can cast
if (player.CanCast()) {
    player.CastAbility();
}

// From systems
var command = new AbilityCommand("fireball", AbilityAction.Cast);
var result = commandHandler.Handle(command, world, entity);
```

### 5. Use Signals (GDScript)

```gdscript
func _ready():
    var player = $Player
    player.connect("ability_cast", Callable(self, "_on_ability_cast"))
    player.connect("mana_changed", Callable(self, "_on_mana_changed"))

func _on_ability_cast(ability_id: String, caster: Node3D):
    print("Ability cast:", ability_id)
    # Show VFX, play sound, etc.

func _on_mana_changed(current: float, max: float, caster: Node3D):
    $UI/ManaBar.value = current / max
```

## Effect System

Abilities can have effects that are applied automatically on execution:

```csharp
var healSpell = new AbilityDefinition(
    id: "heal",
    name: "Heal",
    manaCost: 25f,
    baseCooldownSeconds: 5f,
    kind: AbilityKind.Heal,
    requiresTarget: true,
    canTargetSelf: true,
    range: 10f,
    effects: new IEffectDefinition[] {
        new HealEffect(amount: 30f, isManaHeal: false)
    }
);

var shieldSpell = new AbilityDefinition(
    id: "shield",
    name: "Magic Shield",
    manaCost: 30f,
    baseCooldownSeconds: 15f,
    kind: AbilityKind.Buff,
    requiresTarget: false,
    canTargetSelf: true,
    range: 0f,
    effects: new IEffectDefinition[] {
        new BuffEffect(
            buffId: "shield",
            duration: 10f,
            stats: new BuffStats { ArmorBonus = 5f, ResistanceBonus = 0.2f }
        )
    }
);
```

**Available Effects:**
- `DamageEffect` — Damages health (with resistance support)
- `HealEffect` — Heals health or mana
- `BuffEffect` — Applies beneficial stat modifiers
- `DebuffEffect` — Applies harmful stat modifiers
- `SummonEffect` — Spawns new entities

## Godot Plugin

Enable the plugin in **Project Settings → Plugins**:

1. The "Abilities" dock appears in the bottom panel
2. Register abilities and see them listed
3. Create ability entities from the editor

## Demo Scene

See `godot/scenes/AbilityDemo.tscn` for a complete example with:
- WorldManager setup
- Multiple ability types (fireball, heal, shield)
- Signal connections
- Player and enemy entities

## Testing

### Core Tests (xUnit)
```bash
dotnet test cs/Tests/Core/Abilities.Core.Tests.csproj
```

Tests cover:
- Ability definition creation & equality
- Registry operations (register, get, exists)
- Component behavior (cooldown, mana, learning)
- Command validation & execution

### Godot Integration Tests (GoDotTest)
```bash
# Close Godot editor first
godot --headless --path godot/ --run-tests --quit-on-finish
```

## Building

### For Godot (with Godot dependencies)
```bash
dotnet build -p:GodotBuild=true
```

### Library Only (no Godot)
```bash
dotnet build
```

The `godot/` folder is optional — the Core/ECS libraries work standalone.

## Project Structure Rules

Following MoonBark standards:
- ✅ `godot/` folder contains Godot project files
- ✅ `godot/addons/abilities/` contains plugin source
- ✅ `Core/` contains pure C# business logic
- ✅ `ECS/` contains Friflo ECS components & systems
- ✅ `Tests/` contains xUnit tests (no Godot runtime)

## Status
- ✅ Audited: 2026-04-30
- Changed files this run: 3
- File count: 33 C# files (~3252 lines)
## Documentation

- [Godot Integration Guide](godot/README.md) — Detailed Godot usage
- [Migration Guide](docs/migration.md) — Migrating from Thistletide
- [Roadmap](ROADMAP.md) — Planned features
- [Health](HEALTH.md) — Project health & metrics

## Key Types
## Key Types (33 files, ~3252 lines)
AbilitiesModule, AbilitiesPlugin, AbilityBookComponent, AbilityCommandHandler, AbilityCommandTypesTests, AbilityComponent, AbilityComponentTests, AbilityCooldownComponent, AbilityCooldownSystem, AbilityDefinition, AbilityDefinitionTests, AbilityDemo, AbilityKind, AbilityLearningComponent, AbilityRegistry, AbilityRegistryTests, AbilitySignalBus, AbilitySlotKind, BuffEffect, BuffStats
## Namespaces
- `MoonBark.Abilities.Core`
- `MoonBark.Abilities.Core.Effects`
- `MoonBark.Abilities.Core.Execution`
- `MoonBark.Abilities.ECS`
- `MoonBark.Abilities.Godot`
- `MoonBark.Abilities.Godot.Examples`
- `MoonBark.Abilities.Tests`
## ECS Architecture (v2)
- ECS subdirectories: ECS
- ECS files outside subdirectories: 7
- Flat structure: Core/, ECS/, Godot/ (cs/ prefix not required)