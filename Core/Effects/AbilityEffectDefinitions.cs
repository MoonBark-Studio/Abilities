namespace MoonBark.Abilities.Core.Effects;

using System;
using MoonBark.Framework.Effects;

/// <summary>
/// Defines an effect produced by an ability.
/// </summary>
public class AbilityEffectDefinition : IEffectDefinition, IEquatable<AbilityEffectDefinition>
{
    /// <summary>
    /// Unique identifier for this effect (e.g., "fire_damage", "heal_over_time").
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Human-readable name for UI display.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Targeting requirements for this effect.
    /// </summary>
    public EffectTargetRequirements TargetRequirements { get; }

    /// <summary>
    /// The kind/category of this effect.
    /// </summary>
    public AbilityKind EffectKind { get; }

    /// <summary>
    /// Creates a new ability effect definition.
    /// </summary>
    /// <param name="id">Unique identifier for this effect.</param>
    /// <param name="name">Human-readable name for UI display.</param>
    /// <param name="effectKind">The kind/category of this effect.</param>
    /// <param name="targetRequirements">Targeting requirements for this effect.</param>
    public AbilityEffectDefinition(
        string id,
        string name,
        AbilityKind effectKind,
        EffectTargetRequirements? targetRequirements = null)
    {
        Id = id;
        Name = name;
        EffectKind = effectKind;
        TargetRequirements = targetRequirements ?? new EffectTargetRequirements();
    }

    /// <inheritdoc/>
    public bool Equals(AbilityEffectDefinition? other) =>
        other != null &&
        Id == other.Id &&
        Name == other.Name &&
        EffectKind == other.EffectKind &&
        TargetRequirements.Equals(other.TargetRequirements);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is AbilityEffectDefinition other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id, Name, EffectKind, TargetRequirements.RequireAlive, TargetRequirements.RequireEnemy, TargetRequirements.RequireFriendly, TargetRequirements.MaxRange);

    /// <summary>
    /// Checks equality between two effect definitions.
    /// </summary>
    public static bool operator ==(AbilityEffectDefinition? left, AbilityEffectDefinition? right) => Equals(left, right);

    /// <summary>
    /// Checks inequality between two effect definitions.
    /// </summary>
    public static bool operator !=(AbilityEffectDefinition? left, AbilityEffectDefinition? right) => !Equals(left, right);
}

/// <summary>
/// An effect that heals a target.
/// </summary>
public sealed class HealAbilityEffect : AbilityEffectDefinition, IEquatable<HealAbilityEffect>
{
    /// <summary>
    /// Base amount of healing applied.
    /// </summary>
    public float BaseHealAmount { get; }

    /// <summary>
    /// Creates a new heal effect.
    /// </summary>
    /// <param name="id">Unique identifier for this effect.</param>
    /// <param name="name">Human-readable name for UI display.</param>
    /// <param name="baseHealAmount">Base amount of healing applied.</param>
    public HealAbilityEffect(string id, string name, float baseHealAmount)
        : base(id, name, AbilityKind.Heal)
    {
        BaseHealAmount = baseHealAmount;
    }

    /// <inheritdoc/>
    public bool Equals(HealAbilityEffect? other) =>
        other != null &&
        base.Equals((AbilityEffectDefinition)other) &&
        BaseHealAmount == other.BaseHealAmount;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), BaseHealAmount);
}

/// <summary>
/// An effect that deals damage to a target.
/// </summary>
public sealed class DamageAbilityEffect : AbilityEffectDefinition, IEquatable<DamageAbilityEffect>
{
    /// <summary>
    /// Base amount of damage applied.
    /// </summary>
    public float BaseDamageAmount { get; }

    /// <summary>
    /// The type of damage (e.g., "physical", "fire", "ice").
    /// </summary>
    public string DamageType { get; }

    /// <summary>
    /// Creates a new damage effect.
    /// </summary>
    /// <param name="id">Unique identifier for this effect.</param>
    /// <param name="name">Human-readable name for UI display.</param>
    /// <param name="baseDamageAmount">Base amount of damage applied.</param>
    /// <param name="damageType">The type of damage (e.g., "physical", "fire", "ice").</param>
    public DamageAbilityEffect(string id, string name, float baseDamageAmount, string damageType = "physical")
        : base(id, name, AbilityKind.Damage, new EffectTargetRequirements { RequireEnemy = true, MaxRange = 10f })
    {
        BaseDamageAmount = baseDamageAmount;
        DamageType = damageType;
    }

    /// <inheritdoc/>
    public bool Equals(DamageAbilityEffect? other) =>
        other != null &&
        base.Equals((AbilityEffectDefinition)other) &&
        BaseDamageAmount == other.BaseDamageAmount &&
        DamageType == other.DamageType;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), BaseDamageAmount, DamageType);
}

/// <summary>
/// An effect that applies a buff to the target.
/// </summary>
public sealed class BuffAbilityEffect : AbilityEffectDefinition, IEquatable<BuffAbilityEffect>
{
    /// <summary>
    /// Duration of the buff in seconds.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Unique identifier for the buff applied.
    /// </summary>
    public string BuffId { get; }

    /// <summary>
    /// Creates a new buff effect.
    /// </summary>
    /// <param name="id">Unique identifier for this effect.</param>
    /// <param name="name">Human-readable name for UI display.</param>
    /// <param name="duration">Duration of the buff in seconds.</param>
    /// <param name="buffId">Unique identifier for the buff applied.</param>
    public BuffAbilityEffect(string id, string name, float duration, string buffId)
        : base(id, name, AbilityKind.Buff)
    {
        Duration = duration;
        BuffId = buffId;
    }

    /// <inheritdoc/>
    public bool Equals(BuffAbilityEffect? other) =>
        other != null &&
        base.Equals((AbilityEffectDefinition)other) &&
        Duration == other.Duration &&
        BuffId == other.BuffId;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Duration, BuffId);
}

/// <summary>
/// An effect that applies a debuff to the target.
/// </summary>
public sealed class DebuffAbilityEffect : AbilityEffectDefinition, IEquatable<DebuffAbilityEffect>
{
    /// <summary>
    /// Duration of the debuff in seconds.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Unique identifier for the debuff applied.
    /// </summary>
    public string DebuffId { get; }

    /// <summary>
    /// Creates a new debuff effect.
    /// </summary>
    /// <param name="id">Unique identifier for this effect.</param>
    /// <param name="name">Human-readable name for UI display.</param>
    /// <param name="duration">Duration of the debuff in seconds.</param>
    /// <param name="debuffId">Unique identifier for the debuff applied.</param>
    public DebuffAbilityEffect(string id, string name, float duration, string debuffId)
        : base(id, name, AbilityKind.Debuff, new EffectTargetRequirements { RequireEnemy = true })
    {
        Duration = duration;
        DebuffId = debuffId;
    }

    /// <inheritdoc/>
    public bool Equals(DebuffAbilityEffect? other) =>
        other != null &&
        base.Equals((AbilityEffectDefinition)other) &&
        Duration == other.Duration &&
        DebuffId == other.DebuffId;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Duration, DebuffId);
}

/// <summary>
/// A utility effect (e.g., teleport, dash, etc.).
/// </summary>
public sealed class UtilityAbilityEffect : AbilityEffectDefinition, IEquatable<UtilityAbilityEffect>
{
    /// <summary>
    /// The kind of utility effect.
    /// </summary>
    public string UtilityKind { get; }

    /// <summary>
    /// Creates a new utility effect.
    /// </summary>
    /// <param name="id">Unique identifier for this effect.</param>
    /// <param name="name">Human-readable name for UI display.</param>
    /// <param name="utilityKind">The kind of utility effect.</param>
    public UtilityAbilityEffect(string id, string name, string utilityKind)
        : base(id, name, AbilityKind.Utility)
    {
        UtilityKind = utilityKind;
    }

    /// <inheritdoc/>
    public bool Equals(UtilityAbilityEffect? other) =>
        other != null &&
        base.Equals((AbilityEffectDefinition)other) &&
        UtilityKind == other.UtilityKind;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), UtilityKind);
}

/// <summary>
/// An effect that summons an entity.
/// </summary>
public sealed class SummonAbilityEffect : AbilityEffectDefinition, IEquatable<SummonAbilityEffect>
{
    /// <summary>
    /// The identifier of the entity to summon.
    /// </summary>
    public string EntityToSummon { get; }

    /// <summary>
    /// Creates a new summon effect.
    /// </summary>
    /// <param name="id">Unique identifier for this effect.</param>
    /// <param name="name">Human-readable name for UI display.</param>
    /// <param name="entityToSummon">The identifier of the entity to summon.</param>
    public SummonAbilityEffect(string id, string name, string entityToSummon)
        : base(id, name, AbilityKind.Summon)
    {
        EntityToSummon = entityToSummon;
    }

    /// <inheritdoc/>
    public bool Equals(SummonAbilityEffect? other) =>
        other != null &&
        base.Equals((AbilityEffectDefinition)other) &&
        EntityToSummon == other.EntityToSummon;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), EntityToSummon);
}