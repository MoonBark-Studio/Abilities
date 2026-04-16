namespace MoonBark.Abilities;

using System.Collections.Generic;
using MoonBark.Framework.Effects;

public sealed class AbilityDefinition : IAbilityDefinition
{
    public string Id { get; }
    public string Name { get; }
    public float ManaCost { get; }
    public float BaseCooldownSeconds { get; }
    public AbilityKind Kind { get; }
    public AbilitySlotKind SlotKind { get; }
    public bool RequiresTarget { get; }
    public bool CanTargetSelf { get; }
    public float Range { get; }
    public IReadOnlyList<IEffectDefinition> Effects { get; }

    public AbilityDefinition(
        string id,
        string name,
        float manaCost,
        float baseCooldownSeconds,
        AbilityKind kind,
        bool requiresTarget = false,
        bool canTargetSelf = true,
        float range = 0f,
        IEnumerable<IEffectDefinition>? effects = null)
    {
        Id = id;
        Name = name;
        ManaCost = manaCost;
        BaseCooldownSeconds = baseCooldownSeconds;
        Kind = kind;
        SlotKind = AbilitySlotKind.Active;
        RequiresTarget = requiresTarget;
        CanTargetSelf = canTargetSelf;
        Range = range;
        Effects = effects != null ? new List<IEffectDefinition>(effects) : new List<IEffectDefinition>();
    }
}