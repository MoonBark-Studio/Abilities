namespace MoonBark.Abilities;

using Friflo.Engine.ECS;

/// <summary>
/// Defines an ability attached to an entity with its runtime properties.
/// </summary>
public struct AbilityComponent : IComponent
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
}
