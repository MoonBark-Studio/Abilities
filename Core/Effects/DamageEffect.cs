namespace MoonBark.Abilities.Core.Effects;

using MoonBark.Framework.Effects;

/// <summary>
/// Damages an entity's health.
/// </summary>
public sealed class DamageEffect : IEffectDefinition
{
    /// <summary>Amount of damage to apply.</summary>
    public float Amount { get; }
    
    /// <summary>Whether this is magical damage (ignores some resistances).</summary>
    public bool IsMagical { get; }
    
    public string Id => $"damage_{Amount}_{IsMagical}";
    public string Name => $"Damage {Amount}";
    public EffectTargetRequirements TargetRequirements { get; }
    
    public DamageEffect(float amount, bool isMagical = false, EffectTargetRequirements? targetReqs = null)
    {
        Amount = amount;
        IsMagical = isMagical;
        TargetRequirements = targetReqs ?? new EffectTargetRequirements();
    }
}
