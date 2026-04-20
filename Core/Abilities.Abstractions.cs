namespace MoonBark.Abilities.Core;

using System.Collections.Generic;
using MoonBark.Framework.Effects;

public enum AbilityKind
{
    Buff,
    Debuff,
    Heal,
    Damage,
    Utility,
    Summon
}

public enum AbilitySlotKind
{
    Active,
    Passive,
    Toggle
}

public interface IAbilityDefinition
{
    string Id { get; }
    string Name { get; }
    float ManaCost { get; }
    float BaseCooldownSeconds { get; }
    AbilityKind Kind { get; }
    AbilitySlotKind SlotKind { get; }
    bool RequiresTarget { get; }
    bool CanTargetSelf { get; }
    float Range { get; }
    IReadOnlyList<IEffectDefinition> Effects { get; }
}
