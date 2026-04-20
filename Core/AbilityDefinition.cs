namespace MoonBark.Abilities.Core;

using System;
using System.Collections.Generic;
using MoonBark.Framework.Effects;

/// <summary>
/// Defines an ability that can be cast by entities.
/// </summary>
public sealed class AbilityDefinition : IAbilityDefinition, IEquatable<AbilityDefinition>
{
    /// <summary>
    /// Unique identifier for this ability.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Human-readable name for UI display.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Mana cost required to cast this ability.
    /// </summary>
    public float ManaCost { get; }

    /// <summary>
    /// Base cooldown duration in seconds.
    /// </summary>
    public float BaseCooldownSeconds { get; }

    /// <summary>
    /// The kind/category of this ability.
    /// </summary>
    public AbilityKind Kind { get; }

    /// <summary>
    /// The slot kind (active, passive, toggle).
    /// </summary>
    public AbilitySlotKind SlotKind { get; }

    /// <summary>
    /// Whether this ability requires a valid target to cast.
    /// </summary>
    public bool RequiresTarget { get; }

    /// <summary>
    /// Whether this ability can target the caster.
    /// </summary>
    public bool CanTargetSelf { get; }

    /// <summary>
    /// Maximum range in world units. 0 means no limit.
    /// </summary>
    public float Range { get; }

    /// <summary>
    /// List of effects applied when this ability is executed.
    /// </summary>
    public IReadOnlyList<IEffectDefinition> Effects { get; }

    /// <summary>
    /// Creates a new ability definition.
    /// </summary>
    /// <param name="id">Unique identifier for this ability.</param>
    /// <param name="name">Human-readable name for UI display.</param>
    /// <param name="manaCost">Mana cost required to cast this ability.</param>
    /// <param name="baseCooldownSeconds">Base cooldown duration in seconds.</param>
    /// <param name="kind">The kind/category of this ability.</param>
    /// <param name="requiresTarget">Whether this ability requires a valid target to cast.</param>
    /// <param name="canTargetSelf">Whether this ability can target the caster.</param>
    /// <param name="range">Maximum range in world units. 0 means no limit.</param>
    /// <param name="effects">List of effects applied when this ability is executed.</param>
    public AbilityDefinition(
        string id,
        string name,
        float manaCost,
        float baseCooldownSeconds,
        AbilityKind kind,
        bool requiresTarget = false,
        bool canTargetSelf = true,
        float range = 0f,
        IEnumerable<IEffectDefinition>? effects = null)
    {
        Id = id;
        Name = name;
        ManaCost = manaCost;
        BaseCooldownSeconds = baseCooldownSeconds;
        Kind = kind;
        SlotKind = AbilitySlotKind.Active;
        RequiresTarget = requiresTarget;
        CanTargetSelf = canTargetSelf;
        Range = range;
        Effects = effects != null ? new List<IEffectDefinition>(effects) : new List<IEffectDefinition>();
    }

    /// <inheritdoc/>
    public bool Equals(AbilityDefinition? other) =>
        other != null &&
        Id == other.Id &&
        Name == other.Name &&
        ManaCost == other.ManaCost &&
        BaseCooldownSeconds == other.BaseCooldownSeconds &&
        Kind == other.Kind &&
        SlotKind == other.SlotKind &&
        RequiresTarget == other.RequiresTarget &&
        CanTargetSelf == other.CanTargetSelf &&
        Range == other.Range &&
        Effects.Count == other.Effects.Count;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is AbilityDefinition other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id, Name, ManaCost, BaseCooldownSeconds, Kind, SlotKind, RequiresTarget, CanTargetSelf, Range, Effects.Count);

    /// <summary>
    /// Checks equality between two ability definitions.
    /// </summary>
    public static bool operator ==(AbilityDefinition? left, AbilityDefinition? right) => Equals(left, right);

    /// <summary>
    /// Checks inequality between two ability definitions.
    /// </summary>
    public static bool operator !=(AbilityDefinition? left, AbilityDefinition? right) => !Equals(left, right);
}