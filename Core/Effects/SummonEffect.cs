namespace MoonBark.Abilities.Core.Effects;

using MoonBark.Framework.Effects;

/// <summary>
/// Integer vector for grid-based summon offsets.
/// </summary>
public struct Vector2i
{
    public int X { get; }
    public int Y { get; }
    
    public Vector2i(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public static Vector2i Zero => new(0, 0);
}

/// <summary>
/// Summons a new entity (minion, projectile, etc.).
/// </summary>
public sealed class SummonEffect : IEffectDefinition
{
    /// <summary>Unique identifier for the summon type.</summary>
    public string SummonId { get; }
    
    /// <summary>Number of entities to summon.</summary>
    public int Count { get; }
    
    /// <summary>Duration before summon expires. 0 = permanent.</summary>
    public float Duration { get; }
    
    /// <summary>Offset from summon location.</summary>
    public Vector2i Offset { get; }
    
    /// <summary>Optional: reference to a Godot PackedScene for visual representation.</summary>
    public string? ScenePath { get; }
    
    public string Id => $"summon_{SummonId}";
    public string Name => $"Summon: {SummonId}";
    public EffectTargetRequirements TargetRequirements { get; }
    
    public SummonEffect(string summonId, int count, float duration, Vector2i offset, string? scenePath = null, EffectTargetRequirements? targetReqs = null)
    {
        SummonId = summonId;
        Count = count;
        Duration = duration;
        Offset = offset;
        ScenePath = scenePath;
        TargetRequirements = targetReqs ?? new EffectTargetRequirements();
    }
}
