namespace MoonBark.Abilities.Effects;

using MoonBark.Framework.Effects;
using Friflo.Engine.ECS;

public sealed class AbilityExecutor : IEffectExecutor
{
    public void Execute(IEffectDefinition effect, EffectContext context)
    {
        switch (effect)
        {
            case HealAbilityEffect heal:
                ExecuteHeal(heal, context);
                break;
            case DamageAbilityEffect damage:
                ExecuteDamage(damage, context);
                break;
            case BuffAbilityEffect buff:
                ExecuteBuff(buff, context);
                break;
            case DebuffAbilityEffect debuff:
                ExecuteDebuff(debuff, context);
                break;
            case UtilityAbilityEffect utility:
                ExecuteUtility(utility, context);
                break;
            case SummonAbilityEffect summon:
                ExecuteSummon(summon, context);
                break;
            default:
                throw new NotSupportedException($"Unknown effect type: {effect.GetType().Name}");
        }
    }

    private void ExecuteHeal(HealAbilityEffect effect, EffectContext context)
    {
        var amount = (float)context.Parameters.GetValueOrDefault("amount", effect.BaseHealAmount);
        context.EventBus?.PublishHealing(context.Target, context.Source, amount);
    }

    private void ExecuteDamage(DamageAbilityEffect effect, EffectContext context)
    {
        var amount = (float)context.Parameters.GetValueOrDefault("amount", effect.BaseDamageAmount);
        var damageType = (string)context.Parameters.GetValueOrDefault("type", effect.DamageType);
        context.EventBus?.PublishDamage(context.Target, context.Source, amount, damageType);
    }

    private void ExecuteBuff(BuffAbilityEffect effect, EffectContext context)
    {
        var duration = (float)context.Parameters.GetValueOrDefault("duration", effect.Duration);
        context.EventBus?.PublishStatusApplied(context.Target, context.Source, effect.BuffId);
    }

    private void ExecuteDebuff(DebuffAbilityEffect effect, EffectContext context)
    {
        var duration = (float)context.Parameters.GetValueOrDefault("duration", effect.Duration);
        context.EventBus?.PublishStatusApplied(context.Target, context.Source, effect.DebuffId);
    }

    private void ExecuteUtility(UtilityAbilityEffect effect, EffectContext context)
    {
        context.EventBus?.PublishEffect(context.Target, context.Source, effect.Id);
    }

    private void ExecuteSummon(SummonAbilityEffect effect, EffectContext context)
    {
        context.EventBus?.PublishEffect(context.Target, context.Source, effect.Id);
    }
}