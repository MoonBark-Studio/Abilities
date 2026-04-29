# Godot Integration for Abilities Plugin

This directory contains the Godot-specific integration layer for the Abilities plugin.

## Architecture

The Abilities plugin follows a **thin Godot wiring** principle:

```
Core/                          ← Pure C# business logic (no Godot dependencies)
  ├── AbilityDefinition.cs     ← Immutable ability definitions
  ├── AbilityRegistry.cs       ← Central ability registry
  ├── AbilityCommandHandler.cs ← Command validation & execution
  └── Effects/                 ← Effect definitions (damage, heal, etc.)

ECS/                           ← Friflo ECS components & systems
  ├── AbilityComponent.cs      ← Runtime ability state
  ├── ManaComponent.cs         ← Mana resource tracking
  └── AbilitySystems.cs        ← Cooldown & mana regen systems

godot/                         ← Godot integration (thin adapters)
  ├── scripts/
  │   ├── GodotAbilityNode.cs  ← Godot node wrapping ECS entity
  │   ├── WorldManager.cs      ← ECS world composition root
  │   ├── AbilitySignalBus.cs  ← Core events → Godot signals
  │   └── GodotEffectApplier.cs← Effect → Godot visual reactions
  ├── addons/abilities/        ← Godot plugin
  │   ├── plugin.cfg
  │   └── AbilitiesPlugin.cs   ← Editor plugin
  └── scenes/                  ← Demo scenes
```

## Key Principles

### 1. Logic Lives in Core
All ability logic (validation, cooldowns, mana costs) lives in `Core/`. The Godot layer only provides:
- Node wrappers for entities
- Signal emission for Godot events
- Visual effect triggers

### 2. ECS is the Source of Truth
Entity state is stored in ECS components. Godot nodes observe and react to component changes.

### 3. Signals Bridge the Gap
`AbilitySignalBus` translates Core events (component changes) into Godot signals that UI can bind to.

## Usage

### Quick Start

1. **Add WorldManager to your scene:**
```gdscript
# Create a root node and attach WorldManager
var world_manager = WorldManager.new()
world_manager.auto_update_systems = true
add_child(world_manager)
world_manager.initialize_world()
```

2. **Register abilities:**
```csharp
// In C# (bootstrapping code)
var registry = worldManager.Registry;
registry.Register(new AbilityDefinition(
    id: "fireball",
    name: "Fireball",
    manaCost: 20f,
    baseCooldownSeconds: 3f,
    kind: AbilityKind.Damage,
    requiresTarget: true,
    range: 15f
));
```

3. **Create ability entities:**
```csharp
// Method 1: Programmatic
var player = worldManager.CreateAbilityEntity("fireball", position: Vector3.Zero);

// Method 2: Scene-based
var player = preload("res://scenes/Player.tscn").Instantiate<GodotAbilityNode>();
player.Initialize(world, registry, commandHandler);
add_child(player);
```

4. **Cast abilities:**
```csharp
// From Godot node
player.CastAbility();

// From systems
var result = commandHandler.Handle(
    new AbilityCommand("fireball", AbilityAction.Cast),
    world,
    entity
);
```

### Using Signals

```gdscript
# GDScript
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

### Effect System

Abilities can have effects (damage, heal, buffs, etc.):

```csharp
var fireball = new AbilityDefinition(
    id: "fireball",
    name: "Fireball",
    manaCost: 20f,
    baseCooldownSeconds: 3f,
    kind: AbilityKind.Damage,
    effects: new IEffectDefinition[] {
        new DamageEffect(amount: 25f, isMagical: true)
    }
);
```

Effects are applied automatically when the ability executes. The Godot layer observes component changes and triggers visual effects.

## Godot Plugin

The plugin provides an editor dock for managing abilities:

1. Enable the plugin in Project Settings → Plugins
2. The "Abilities" dock appears in the bottom panel
3. Register abilities and see them listed

## Demo

See `scenes/AbilityDemo.tscn` for a complete example with:
- WorldManager setup
- Multiple ability types
- Signal connections
- UI integration

## Building

The plugin builds as a DLL with optional Godot dependencies:

```xml
<!-- In .csproj -->
<ItemGroup Condition="'$(GodotBuild)' == 'true'">
    <PackageReference Include="GodotSharp" Version="4.4.1" />
</ItemGroup>
```

Build for Godot:
```bash
dotnet build -p:GodotBuild=true
```

## Testing

See the main `Tests/` directory for unit tests. Integration tests using GoDotTest are in `godot/tests/`.

## Files

| File | Purpose |
|------|---------|
| `scripts/GodotAbilityNode.cs` | Godot node wrapping an ECS entity with abilities |
| `scripts/WorldManager.cs` | Composition root: manages world, systems, entities |
| `scripts/AbilitySignalBus.cs` | Bridges Core events to Godot signals |
| `scripts/GodotEffectApplier.cs` | Applies effects to Godot nodes |
| `scripts/AbilitiesPlugin.cs` | Editor plugin for ability management |
| `addons/abilities/plugin.cfg` | Godot plugin configuration |
| `scenes/AbilityDemo.tscn` | Demo scene |
