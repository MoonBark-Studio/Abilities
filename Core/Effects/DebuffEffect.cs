namespace MoonBark.Abilities.Core.Effects;

using MoonBark.Framework.Effects;

/// <summary>
/// Applies a harmful debuff to an entity.
/// </summary>
public sealed class DebuffEffect : IEffectDefinition
{
    /// <summary>Unique identifier for the debuff type.</summary>
    public string DebuffId { get; }
    
    /// <summary>Duration of the debuff in seconds. 0 = permanent.</summary>
    public float Duration { get; }
    
    /// <summary>Stat modification values.</summary>
    public DebuffStats Stats { get; }
    
    public string Id => $"debuff_{DebuffId}";
    public string Name => $"Debuff: {DebuffId}";
    public EffectTargetRequirements TargetRequirements { get; }
    
    public DebuffEffect(string debuffId, float duration, DebuffStats stats, EffectTargetRequirements? targetReqs = null)
    {
        DebuffId = debuffId;
        Duration = duration;
        Stats = stats;
        TargetRequirements = targetReqs ?? new EffectTargetRequirements();
    }
}

/// <summary>
/// Stat modifications applied by a debuff.
/// </summary>
public struct DebuffStats
{
    public float DamageMultiplier { get; set; } = 1.0f;
    public float SpeedMultiplier { get; set; } = 1.0f;
    public float ArmorPenalty { get; set; } = 0.0f;
    public float ResistancePenalty { get; set; } = 0.0f;
    
    public DebuffStats() { }
}
