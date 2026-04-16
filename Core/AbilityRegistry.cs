namespace MoonBark.Abilities;

using System.Collections.Generic;

/// <summary>
/// Central registry for ability definitions.
/// </summary>
/// <remarks>
/// This registry stores all ability definitions in a central location,
/// providing a single source of truth for ability properties.
/// Targeting rules are NOT stored here - they are handled by the EntityTargetingSystem plugin.
/// </remarks>
public class AbilityRegistry
{
    private readonly Dictionary<string, IAbilityDefinition> _abilities;

    /// <summary>
    /// Creates a new ability registry.
    /// </summary>
    public AbilityRegistry()
    {
        _abilities = new Dictionary<string, IAbilityDefinition>();
    }

    /// <summary>
    /// Registers an ability definition.
    /// </summary>
    /// <param name="definition">The ability definition to register.</param>
    /// <exception cref="ArgumentException">Thrown if an ability with the same ID is already registered.</exception>
    public void Register(IAbilityDefinition definition)
    {
        if (_abilities.ContainsKey(definition.Id))
        {
            throw new ArgumentException($"Ability with ID '{definition.Id}' is already registered.");
        }

        _abilities[definition.Id] = definition;
    }

    /// <summary>
    /// Gets an ability definition by ID.
    /// </summary>
    /// <param name="abilityId">The ability ID.</param>
    /// <returns>The ability definition.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the ability ID is not found.</exception>
    public IAbilityDefinition Get(string abilityId)
    {
        if (!_abilities.TryGetValue(abilityId, out var definition))
        {
            throw new KeyNotFoundException($"Ability with ID '{abilityId}' is not registered.");
        }

        return definition;
    }

    /// <summary>
    /// Checks if an ability with the given ID is registered.
    /// </summary>
    /// <param name="abilityId">The ability ID to check.</param>
    /// <returns>True if the ability is registered, false otherwise.</returns>
    public bool Exists(string abilityId)
    {
        return _abilities.ContainsKey(abilityId);
    }

    /// <summary>
    /// Gets all registered ability IDs.
    /// </summary>
    /// <returns>A collection of all registered ability IDs.</returns>
    public System.Collections.Generic.ICollection<string> GetAllAbilityIds()
    {
        return _abilities.Keys;
    }

    /// <summary>
    /// Gets the count of registered abilities.
    /// </summary>
    /// <returns>The number of registered abilities.</returns>
    public int Count => _abilities.Count;
}




