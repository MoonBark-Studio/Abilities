# Abilities Migration

## Scope

This plugin now owns the shared ability data contracts previously duplicated in Thistletide:

- `AbilityComponent` - Runtime entity ability component
- `AbilityBookComponent` - Tracks known abilities
- `AbilityCooldownComponent` - Tracks cooldown state
- `ManaComponent` - Mana resource management
- `AbilityLearningComponent` - Learning progress tracking
- `CooldownReductionComponent` - Cooldown reduction modifiers
- `AbilityDefinition` - Centralized ability definitions
- `AbilityRegistry` - Registry for ability definitions
- Tags: `CanCastAbilitiesTag`, `LearningAbilityTag`, `CanLearnAbilitiesTag`, `OnCooldownTag`

## Plugin Dependencies

Thistletide now references these plugins for ability functionality:
- **Abilities** - Core ability components and systems
- **AbilityExecution** - Execution pipeline orchestration
- **EntityTargetingSystem** - Targeting validation

## Migration Details

### Removed Duplicates from Thistletide

1. `Thistletide/Core/Abilities/AbilityComponents.cs` - Now uses `MoonBark.Abilities.AbilityComponent`
2. Duplicate `CooldownSystem` - Now uses `MoonBark.Abilities.AbilityCooldownSystem`

### Thistletide-Specific Code That Remains

The following game-specific code remains in Thistletide:

1. **AbilityLearningSystem** - Game-specific learning rate logic
2. **CooldownReductionSystem** - Helper for cooldown reduction calculations
3. **AbilityCommandHandler** - Game-specific command handling using plugin components
4. **AbilityExecutionSystem** - ECS system for ability execution

### Using the Plugin Components

```csharp
using MoonBark.Abilities;

// Use AbilityRegistry for centralized ability definitions
var registry = new AbilityRegistry();
registry.Register(new AbilityDefinition("fireball", "Fireball", 30f, 5f, AbilityKind.Damage));

// Entity setup
var entity = world.CreateEntity();
entity.AddComponent(new AbilityBookComponent());
entity.AddComponent(new ManaComponent(100f, 100f));
entity.AddComponent(new AbilityCooldownComponent("fireball", 5f));
entity.AddComponent(new CanCastAbilitiesTag());
```

## Integration Notes

Game-specific execution remains in the game layer or in adapters built on top of this plugin.

Thistletide now references this plugin directly instead of compiling duplicate copies of the same components.
