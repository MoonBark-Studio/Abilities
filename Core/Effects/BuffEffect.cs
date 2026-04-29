namespace MoonBark.Abilities.Core.Effects;

using MoonBark.Framework.Effects;

/// <summary>
/// Applies a beneficial buff to an entity.
/// </summary>
public sealed class BuffEffect : IEffectDefinition
{
    /// <summary>Unique identifier for the buff type.</summary>
    public string BuffId { get; }
    
    /// <summary>Duration of the buff in seconds. 0 = permanent.</summary>
    public float Duration { get; }
    
    /// <summary>Stat modification values.</summary>
    public BuffStats Stats { get; }
    
    public string Id => $"buff_{BuffId}";
    public string Name => $"Buff: {BuffId}";
    public EffectTargetRequirements TargetRequirements { get; }
    
    public BuffEffect(string buffId, float duration, BuffStats stats, EffectTargetRequirements? targetReqs = null)
    {
        BuffId = buffId;
        Duration = duration;
        Stats = stats;
        TargetRequirements = targetReqs ?? new EffectTargetRequirements();
    }
}

/// <summary>
/// Stat modifications applied by a buff.
/// </summary>
public struct BuffStats
{
    public float DamageMultiplier { get; set; } = 1.0f;
    public float SpeedMultiplier { get; set; } = 1.0f;
    public float ArmorBonus { get; set; } = 0.0f;
    public float ResistanceBonus { get; set; } = 0.0f;
    
    public BuffStats() { }
}
