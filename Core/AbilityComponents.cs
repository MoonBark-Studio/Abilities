namespace MoonBark.Abilities.Core;

using System;
using Friflo.Engine.ECS;

/// <summary>
/// Tracks cooldown state for a specific ability on an entity.
/// </summary>
public struct AbilityCooldownComponent : IComponent, IEquatable<AbilityCooldownComponent>
{
    /// <summary>
    /// The ability ID this cooldown is tracking.
    /// </summary>
    public string AbilityId { get; }

    /// <summary>
    /// Base cooldown duration in seconds.
    /// </summary>
    public float BaseCooldownSeconds { get; }

    /// <summary>
    /// Remaining cooldown time in seconds.
    /// </summary>
    public float RemainingCooldownSeconds { get; private set; }

    /// <summary>
    /// Whether the ability is currently on cooldown.
    /// </summary>
    public bool IsOnCooldown => RemainingCooldownSeconds > 0.0f;

    /// <summary>
    /// Creates a new ability cooldown component.
    /// </summary>
    /// <param name="abilityId">The ability ID to track.</param>
    /// <param name="baseCooldownSeconds">Base cooldown duration in seconds.</param>
    public AbilityCooldownComponent(string abilityId, float baseCooldownSeconds)
    {
        AbilityId = abilityId;
        BaseCooldownSeconds = baseCooldownSeconds;
        RemainingCooldownSeconds = 0.0f;
    }

    /// <summary>
    /// Starts the cooldown for this ability.
    /// </summary>
    public void StartCooldown()
    {
        RemainingCooldownSeconds = BaseCooldownSeconds;
    }

    /// <summary>
    /// Reduces the remaining cooldown time.
    /// </summary>
    /// <param name="deltaTime">Time to reduce in seconds.</param>
    public void ReduceCooldown(float deltaTime)
    {
        RemainingCooldownSeconds = System.Math.Max(0.0f, RemainingCooldownSeconds - deltaTime);
    }

    /// <summary>
    /// Gets the cooldown progress (0.0 to 1.0, where 1.0 is fully on cooldown).
    /// </summary>
    /// <returns>Cooldown progress value.</returns>
    public float GetCooldownProgress()
    {
        if (!IsOnCooldown)
        {
            return 0.0f;
        }

        return RemainingCooldownSeconds / BaseCooldownSeconds;
    }

    public bool Equals(AbilityCooldownComponent other) =>
        AbilityId == other.AbilityId &&
        BaseCooldownSeconds == other.BaseCooldownSeconds &&
        RemainingCooldownSeconds == other.RemainingCooldownSeconds;
    public override bool Equals(object? obj) => obj is AbilityCooldownComponent other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(AbilityId, BaseCooldownSeconds, RemainingCooldownSeconds);
    public static bool operator ==(AbilityCooldownComponent left, AbilityCooldownComponent right) => left.Equals(right);
    public static bool operator !=(AbilityCooldownComponent left, AbilityCooldownComponent right) => !left.Equals(right);
}

/// <summary>
/// Tracks mana resource for an entity.
/// </summary>
public struct ManaComponent : IComponent, IEquatable<ManaComponent>
{
    /// <summary>
    /// Current mana amount.
    /// </summary>
    public float CurrentMana { get; private set; }

    /// <summary>
    /// Maximum mana capacity.
    /// </summary>
    public float MaxMana { get; }

    /// <summary>
    /// Creates a new mana component.
    /// </summary>
    /// <param name="currentMana">Initial current mana.</param>
    /// <param name="maxMana">Maximum mana capacity.</param>
    public ManaComponent(float currentMana, float maxMana)
    {
        CurrentMana = currentMana;
        MaxMana = maxMana;
    }

    /// <summary>
    /// Attempts to consume mana.
    /// </summary>
    /// <param name="amount">Amount of mana to consume.</param>
    /// <returns>True if mana was consumed, false if insufficient mana.</returns>
    public bool ConsumeMana(float amount)
    {
        if (CurrentMana < amount)
        {
            return false;
        }

        CurrentMana -= amount;
        return true;
    }

    /// <summary>
    /// Regenerates mana.
    /// </summary>
    /// <param name="amount">Amount of mana to regenerate.</param>
    public void RegenerateMana(float amount)
    {
        CurrentMana = System.Math.Min(MaxMana, CurrentMana + amount);
    }

    public bool Equals(ManaComponent other) => CurrentMana == other.CurrentMana && MaxMana == other.MaxMana;
    public override bool Equals(object? obj) => obj is ManaComponent other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(CurrentMana, MaxMana);
    public static bool operator ==(ManaComponent left, ManaComponent right) => left.Equals(right);
    public static bool operator !=(ManaComponent left, ManaComponent right) => !left.Equals(right);
}

/// <summary>
/// Applies cooldown reduction modifiers to abilities.
/// </summary>
public struct CooldownReductionComponent : IComponent, IEquatable<CooldownReductionComponent>
{
    /// <summary>
    /// Cooldown reduction percentage (0.0 to 1.0).
    /// </summary>
    public float ReductionPercentage { get; }

    /// <summary>
    /// Creates a new cooldown reduction component.
    /// </summary>
    /// <param name="reductionPercentage">Cooldown reduction percentage (0.0 to 1.0).</param>
    public CooldownReductionComponent(float reductionPercentage)
    {
        ReductionPercentage = System.Math.Clamp(reductionPercentage, 0.0f, 1.0f);
    }

    /// <summary>
    /// Calculates the effective cooldown after applying reduction.
    /// </summary>
    /// <param name="baseCooldown">Base cooldown duration.</param>
    /// <returns>Effective cooldown duration after reduction.</returns>
    public float GetEffectiveCooldown(float baseCooldown)
    {
        return baseCooldown * (1.0f - ReductionPercentage);
    }

    public bool Equals(CooldownReductionComponent other) => ReductionPercentage == other.ReductionPercentage;
    public override bool Equals(object? obj) => obj is CooldownReductionComponent other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(ReductionPercentage);
    public static bool operator ==(CooldownReductionComponent left, CooldownReductionComponent right) => left.Equals(right);
    public static bool operator !=(CooldownReductionComponent left, CooldownReductionComponent right) => !left.Equals(right);
}

/// <summary>
/// Tag for entities that are currently on cooldown.
/// </summary>
public struct OnCooldownTag : IComponent, IEquatable<OnCooldownTag>
{
    public bool Equals(OnCooldownTag other) => true;
    public override bool Equals(object? obj) => obj is OnCooldownTag;
    public override int GetHashCode() => 0;
    public static bool operator ==(OnCooldownTag left, OnCooldownTag right) => true;
    public static bool operator !=(OnCooldownTag left, OnCooldownTag right) => false;
}

/// <summary>
/// Tag for entities that can cast abilities.
/// </summary>
public struct CanCastAbilitiesTag : IComponent, IEquatable<CanCastAbilitiesTag>
{
    public bool Equals(CanCastAbilitiesTag other) => true;
    public override bool Equals(object? obj) => obj is CanCastAbilitiesTag;
    public override int GetHashCode() => 0;
    public static bool operator ==(CanCastAbilitiesTag left, CanCastAbilitiesTag right) => true;
    public static bool operator !=(CanCastAbilitiesTag left, CanCastAbilitiesTag right) => false;
}

