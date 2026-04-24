namespace MoonBark.Abilities.ECS;

using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

/// <summary>
/// Component that tracks all abilities known/learned by an entity.
/// Acts as an ability book registry per entity.
/// </summary>
public struct AbilityBookComponent : IComponent, IEquatable<AbilityBookComponent>
{
    /// <summary>
    /// Set of ability IDs that the entity knows and can cast.
    /// </summary>
    public HashSet<string> KnownAbilities { get; private set; }

    /// <summary>
    /// Creates a new ability book component.
    /// </summary>
    /// <param name="knownAbilities">Initial set of known abilities.</param>
    public AbilityBookComponent(HashSet<string> knownAbilities)
    {
        KnownAbilities = knownAbilities ?? new HashSet<string>();
    }

    /// <summary>
    /// Creates an empty ability book component.
    /// </summary>
    public AbilityBookComponent() : this(new HashSet<string>())
    {
    }

    /// <summary>
    /// Creates an ability book with initial abilities.
    /// </summary>
    /// <param name="initialAbilities">Initial ability IDs to add.</param>
    public static AbilityBookComponent Create(params string[] initialAbilities)
    {
        var knownAbilities = new HashSet<string>(initialAbilities);
        return new AbilityBookComponent(knownAbilities);
    }

    /// <summary>
    /// Checks if the entity knows a specific ability.
    /// </summary>
    /// <param name="abilityId">The ability ID to check.</param>
    /// <returns>True if the entity knows the ability, false otherwise.</returns>
    public readonly bool KnowsAbility(string abilityId)
    {
        return KnownAbilities.Contains(abilityId);
    }

    /// <summary>
    /// Adds a new ability to the entity's known abilities.
    /// </summary>
    /// <param name="abilityId">The ability ID to add.</param>
    /// <returns>True if the ability was added, false if already known.</returns>
    public bool LearnAbility(string abilityId)
    {
        return KnownAbilities.Add(abilityId);
    }

    /// <summary>
    /// Removes an ability from the entity's known abilities.
    /// </summary>
    /// <param name="abilityId">The ability ID to remove.</param>
    /// <returns>True if the ability was removed, false if not known.</returns>
    public bool ForgetAbility(string abilityId)
    {
        return KnownAbilities.Remove(abilityId);
    }

    /// <summary>
    /// Gets the count of known abilities.
    /// </summary>
    public readonly int KnownAbilityCount => KnownAbilities.Count;

    /// <summary>
    /// Gets a copy of all known abilities.
    /// </summary>
    /// <returns>A new set containing all known ability IDs.</returns>
    public readonly HashSet<string> GetAllKnownAbilities()
    {
        return new HashSet<string>(KnownAbilities);
    }

    public bool Equals(AbilityBookComponent other) =>
        KnownAbilities.SetEquals(other.KnownAbilities);
    public override bool Equals(object? obj) => obj is AbilityBookComponent other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(KnownAbilities.Count);
    public static bool operator ==(AbilityBookComponent left, AbilityBookComponent right) => left.Equals(right);
    public static bool operator !=(AbilityBookComponent left, AbilityBookComponent right) => !left.Equals(right);
}

/// <summary>
/// Component that tracks ability learning progress and requirements.
/// </summary>
public struct AbilityLearningComponent : IComponent, IEquatable<AbilityLearningComponent>
{
    /// <summary>
    /// Dictionary mapping ability IDs to learning progress (0.0 to 1.0).
    /// </summary>
    public Dictionary<string, float> LearningProgress { get; private set; }

    /// <summary>
    /// Creates a new ability learning component.
    /// </summary>
    public AbilityLearningComponent()
    {
        LearningProgress = new Dictionary<string, float>();
    }

    /// <summary>
    /// Gets the learning progress for a specific ability.
    /// </summary>
    /// <param name="abilityId">The ability ID to check.</param>
    /// <returns>Learning progress (0.0 to 1.0), or 0.0 if not being learned.</returns>
    public readonly float GetProgress(string abilityId)
    {
        return LearningProgress.TryGetValue(abilityId, out float progress) ? progress : 0.0f;
    }

    /// <summary>
    /// Starts learning an ability.
    /// </summary>
    /// <param name="abilityId">The ability ID to learn.</param>
    /// <returns>True if learning was started, false if already learning or known.</returns>
    public bool StartLearning(string abilityId)
    {
        if (LearningProgress.ContainsKey(abilityId))
        {
            return false;
        }

        LearningProgress[abilityId] = 0.0f;
        return true;
    }

    /// <summary>
    /// Updates learning progress for an ability.
    /// </summary>
    /// <param name="abilityId">The ability ID to update.</param>
    /// <param name="deltaProgress">Progress to add (0.0 to 1.0).</param>
    /// <returns>New progress value, or null if not being learned.</returns>
    public float? UpdateProgress(string abilityId, float deltaProgress)
    {
        if (!LearningProgress.TryGetValue(abilityId, out float currentProgress))
        {
            return null;
        }

        float newProgress = System.Math.Clamp(currentProgress + deltaProgress, 0.0f, 1.0f);
        LearningProgress[abilityId] = newProgress;
        return newProgress;
    }

    /// <summary>
    /// Completes learning for an ability.
    /// </summary>
    /// <param name="abilityId">The ability ID to complete.</param>
    /// <returns>True if learning was completed, false if not being learned.</returns>
    public bool CompleteLearning(string abilityId)
    {
        if (!LearningProgress.ContainsKey(abilityId))
        {
            return false;
        }

        LearningProgress[abilityId] = 1.0f;
        return true;
    }

    /// <summary>
    /// Removes an ability from learning progress (typically when completed or failed).
    /// </summary>
    /// <param name="abilityId">The ability ID to remove.</param>
    /// <returns>True if removed, false if not being learned.</returns>
    public bool RemoveLearning(string abilityId)
    {
        return LearningProgress.Remove(abilityId);
    }

    /// <summary>
    /// Gets all abilities being learned.
    /// </summary>
    /// <returns>Dictionary of ability IDs to their current progress.</returns>
    public readonly Dictionary<string, float> GetAllLearningProgress()
    {
        return new Dictionary<string, float>(LearningProgress);
    }

    public bool Equals(AbilityLearningComponent other) =>
        LearningProgress.Count == other.LearningProgress.Count &&
        LearningProgress.All(kvp => other.LearningProgress.TryGetValue(kvp.Key, out var v) && v == kvp.Value);
    public override bool Equals(object? obj) => obj is AbilityLearningComponent other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(LearningProgress.Count);
    public static bool operator ==(AbilityLearningComponent left, AbilityLearningComponent right) => left.Equals(right);
    public static bool operator !=(AbilityLearningComponent left, AbilityLearningComponent right) => !left.Equals(right);
}

/// <summary>
/// Tag for entities that can learn new abilities.
/// </summary>
public struct CanLearnAbilitiesTag : IComponent
{
}

/// <summary>
/// Tag for entities that are currently learning an ability.
/// </summary>
public struct LearningAbilityTag : IComponent
{
}

