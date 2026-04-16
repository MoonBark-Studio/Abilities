namespace MoonBark.Abilities.Effects;

using MoonBark.Framework.Effects;
using MoonBark.Framework.Abilities;

public class AbilityEffectDefinition : IEffectDefinition
{
    public string Id { get; }
    public string Name { get; }
    public EffectTargetRequirements TargetRequirements { get; }
    public AbilityKind EffectKind { get; }

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
}

public sealed class HealAbilityEffect : AbilityEffectDefinition
{
    public float BaseHealAmount { get; }

    public HealAbilityEffect(string id, string name, float baseHealAmount)
        : base(id, name, AbilityKind.Heal)
    {
        BaseHealAmount = baseHealAmount;
    }
}

public sealed class DamageAbilityEffect : AbilityEffectDefinition
{
    public float BaseDamageAmount { get; }
    public string DamageType { get; }

    public DamageAbilityEffect(string id, string name, float baseDamageAmount, string damageType = "physical")
        : base(id, name, AbilityKind.Damage, new EffectTargetRequirements { RequireEnemy = true, MaxRange = 10f })
    {
        BaseDamageAmount = baseDamageAmount;
        DamageType = damageType;
    }
}

public sealed class BuffAbilityEffect : AbilityEffectDefinition
{
    public float Duration { get; }
    public string BuffId { get; }

    public BuffAbilityEffect(string id, string name, float duration, string buffId)
        : base(id, name, AbilityKind.Buff)
    {
        Duration = duration;
        BuffId = buffId;
    }
}

public sealed class DebuffAbilityEffect : AbilityEffectDefinition
{
    public float Duration { get; }
    public string DebuffId { get; }

    public DebuffAbilityEffect(string id, string name, float duration, string debuffId)
        : base(id, name, AbilityKind.Debuff, new EffectTargetRequirements { RequireEnemy = true })
    {
        Duration = duration;
        DebuffId = debuffId;
    }
}

public sealed class UtilityAbilityEffect : AbilityEffectDefinition
{
    public string UtilityKind { get; }

    public UtilityAbilityEffect(string id, string name, string utilityKind)
        : base(id, name, AbilityKind.Utility)
    {
        UtilityKind = utilityKind;
    }
}

public sealed class SummonAbilityEffect : AbilityEffectDefinition
{
    public string EntityToSummon { get; }

    public SummonAbilityEffect(string id, string name, string entityToSummon)
        : base(id, name, AbilityKind.Summon)
    {
        EntityToSummon = entityToSummon;
    }
}