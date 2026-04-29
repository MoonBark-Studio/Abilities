namespace MoonBark.Abilities.Core.Effects;

using MoonBark.Framework.Effects;

/// <summary>
/// Heals an entity's health or mana.
/// </summary>
public sealed class HealEffect : IEffectDefinition
{
    /// <summary>Amount of healing to apply.</summary>
    public float Amount { get; }
    
    /// <summary>Whether this heals mana instead of health.</summary>
    public bool IsManaHeal { get; }
    
    public string Id => $"heal_{Amount}_{IsManaHeal}";
    public string Name => IsManaHeal ? $"Mana Heal {Amount}" : $"Heal {Amount}";
    public EffectTargetRequirements TargetRequirements { get; }
    
    public HealEffect(float amount, bool isManaHeal = false, EffectTargetRequirements? targetReqs = null)
    {
        Amount = amount;
        IsManaHeal = isManaHeal;
        TargetRequirements = targetReqs ?? new EffectTargetRequirements();
    }
}
