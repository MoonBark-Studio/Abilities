using System;
using Friflo.Engine.ECS;

namespace MoonBark.Abilities.ECS;

/// <summary>
/// Tracks health/HP for an entity.
/// Can be used by damage/heal effects.
/// </summary>
public struct HealthComponent : IComponent, IEquatable<HealthComponent>
{
    /// <summary>
    /// Current health value.
    /// </summary>
    public float CurrentHealth { get; private set; }
    
    /// <summary>
    /// Maximum health capacity.
    /// </summary>
    public float MaxHealth { get; }
    
    /// <summary>
    /// Creates a new health component.
    /// </summary>
    /// <param name="currentHealth">Initial current health.</param>
    /// <param name="maxHealth">Maximum health capacity.</param>
    public HealthComponent(float currentHealth, float maxHealth)
    {
        CurrentHealth = Math.Clamp(currentHealth, 0f, maxHealth);
        MaxHealth = maxHealth;
    }
    
    /// <summary>
    /// Damages the entity.
    /// </summary>
    /// <param name="amount">Damage amount.</param>
    /// <returns>True if entity is still alive, false if health reached 0 or below.</returns>
    public bool Damage(float amount)
    {
        CurrentHealth = Math.Clamp(CurrentHealth - amount, 0f, MaxHealth);
        return CurrentHealth > 0f;
    }
    
    /// <summary>
    /// Heals the entity.
    /// </summary>
    /// <param name="amount">Heal amount.</param>
    /// <returns>New current health value.</returns>
    public float Heal(float amount)
    {
        CurrentHealth = Math.Clamp(CurrentHealth + amount, 0f, MaxHealth);
        return CurrentHealth;
    }
    
    /// <summary>
    /// Checks if entity is alive (health > 0).
    /// </summary>
    public bool IsAlive => CurrentHealth > 0f;
    
    public bool Equals(HealthComponent other) => 
        CurrentHealth == other.CurrentHealth && MaxHealth == other.MaxHealth;
    
    public override bool Equals(object? obj) => 
        obj is HealthComponent other && Equals(other);
    
    public override int GetHashCode() => 
        HashCode.Combine(CurrentHealth, MaxHealth);
    
    public static bool operator ==(HealthComponent left, HealthComponent right) => 
        left.Equals(right);
    
    public static bool operator !=(HealthComponent left, HealthComponent right) => 
        !left.Equals(right);
}

/// <summary>
/// Component for tracking damage resistance/weakness.
/// </summary>
public struct ResistanceComponent : IComponent, IEquatable<ResistanceComponent>
{
    /// <summary>
    /// Damage reduction percentage (0.0 to 1.0).
    /// 0.0 = no reduction, 0.5 = 50% reduction, 1.0 = immune.
    /// </summary>
    public float DamageReduction { get; }
    
    /// <summary>
    /// Creates a new resistance component.
    /// </summary>
    /// <param name="damageReduction">Reduction percentage (0.0 to 1.0).</param>
    public ResistanceComponent(float damageReduction)
    {
        DamageReduction = Math.Clamp(damageReduction, 0f, 1f);
    }
    
    public bool Equals(ResistanceComponent other) => 
        DamageReduction == other.DamageReduction;
    
    public override bool Equals(object? obj) => 
        obj is ResistanceComponent other && Equals(other);
    
    public override int GetHashCode() => 
        DamageReduction.GetHashCode();
    
    public static bool operator ==(ResistanceComponent left, ResistanceComponent right) => 
        left.Equals(right);
    
    public static bool operator !=(ResistanceComponent left, ResistanceComponent right) => 
        !left.Equals(right);
}

/// <summary>
/// Tag for entities that are currently dead.
/// </summary>
public struct DeadTag : IComponent
{
}