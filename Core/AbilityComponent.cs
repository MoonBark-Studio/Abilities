namespace MoonBark.Abilities.Core;

using System;
using Friflo.Engine.ECS;

/// <summary>
/// Defines an ability attached to an entity with its runtime properties.
/// </summary>
public struct AbilityComponent : IComponent, IEquatable<AbilityComponent>
{
    /// <summary>
    /// Unique identifier for the ability.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Display name of the ability.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Mana cost to cast the ability.
    /// </summary>
    public float ManaCost { get; }

    /// <summary>
    /// Base cooldown duration in seconds.
    /// </summary>
    public float BaseCooldownSeconds { get; }

    /// <summary>
    /// Creates a new ability component.
    /// </summary>
    /// <param name="id">Unique identifier for the ability.</param>
    /// <param name="name">Display name of the ability.</param>
    /// <param name="manaCost">Mana cost to cast the ability.</param>
    /// <param name="baseCooldownSeconds">Base cooldown duration in seconds.</param>
    public AbilityComponent(string id, string name, float manaCost, float baseCooldownSeconds)
    {
        Id = id;
        Name = name;
        ManaCost = manaCost;
        BaseCooldownSeconds = baseCooldownSeconds;
    }

    public bool Equals(AbilityComponent other) =>
        Id == other.Id &&
        Name == other.Name &&
        ManaCost == other.ManaCost &&
        BaseCooldownSeconds == other.BaseCooldownSeconds;
    public override bool Equals(object? obj) => obj is AbilityComponent other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Id, Name, ManaCost, BaseCooldownSeconds);
    public static bool operator ==(AbilityComponent left, AbilityComponent right) => left.Equals(right);
    public static bool operator !=(AbilityComponent left, AbilityComponent right) => !left.Equals(right);
}
