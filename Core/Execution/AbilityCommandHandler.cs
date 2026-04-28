namespace MoonBark.Abilities.Core.Execution;

using MoonBark.Abilities.Core.Effects;
using MoonBark.Abilities.ECS;
using MoonBark.Framework.Commands;
using MoonBark.Framework.Commands.Bus;
using MoonBark.Framework.Effects;
using Friflo.Engine.ECS;
using System;
using System.Threading.Tasks;

/// <summary>
/// Handles ability commands from the Framework CommandEventBus.
/// Integrates the Abilities plugin pipeline with the Framework command system.
/// </summary>
public sealed class AbilityCommandHandler : ICommandHandler<AbilityCommand>
{
    private readonly AbilityRegistry _abilityRegistry;

    public AbilityCommandHandler(AbilityRegistry abilityRegistry)
    {
        _abilityRegistry = abilityRegistry;
    }

    /// <summary>
    /// Handles an ability command dispatched through the Framework CommandEventBus.
    /// Entity resolution from command source is caller responsibility.
    /// </summary>
    public async Task<CommandResult> HandleAsync(AbilityCommand command)
    {
        throw new NotSupportedException(
            "AbilityCommandHandler.HandleAsync requires ECS entity context. " +
            "Use Handle(command, world, caster) for ECS-context calls.");
    }

    /// <summary>
    /// Handles an ability command with explicit ECS entity context.
    /// </summary>
    public AbilityCommandResult Handle(AbilityCommand command, EntityStore world, Entity caster)
    {
        // Cancel is always allowed
        if (command.AbilityAction == AbilityAction.Cancel)
            return CancelAbility(caster, command.AbilityId);

        // Validate caster can cast
        var validation = ValidateCommand(command.AbilityId, caster);
        if (!validation.IsValid)
            return AbilityCommandResult.Failure(validation.FailureReason ?? "Validation failed");

        // Get ability definition
        var abilityDef = _abilityRegistry.Get(command.AbilityId);

        // Check mana
        var mana = CheckMana(caster, abilityDef.ManaCost);
        if (!mana.IsValid)
            return AbilityCommandResult.Failure(mana.FailureReason ?? "Mana check failed");

        // Check cooldown
        var cooldown = CheckCooldown(caster, command.AbilityId);
        if (!cooldown.IsValid)
            return AbilityCommandResult.Failure(cooldown.FailureReason ?? "Cooldown check failed");

        // Execute
        return ExecuteAbility(caster, abilityDef);
    }

    private AbilityCommandResult CancelAbility(Entity caster, string abilityId)
        => AbilityCommandResult.Success($"Ability '{abilityId}' cancelled");

    private CommandValidationResult ValidateCommand(string abilityId, Entity caster)
    {
        if (!caster.HasComponent<CanCastAbilitiesTag>())
            return CommandValidationResult.Failure("Caster cannot cast abilities");

        if (caster.HasComponent<AbilityBookComponent>())
        {
            ref var book = ref caster.GetComponent<AbilityBookComponent>();
            if (!book.KnowsAbility(abilityId))
                return CommandValidationResult.Failure("Caster does not know this ability");
        }

        if (caster.HasComponent<LearningAbilityTag>())
            return CommandValidationResult.Failure("Caster is currently learning");

        if (!_abilityRegistry.Exists(abilityId))
            return CommandValidationResult.Failure("Ability does not exist");

        return CommandValidationResult.Success();
    }

    private ManaCheckResult CheckMana(Entity caster, float manaCost)
    {
        if (!caster.HasComponent<ManaComponent>())
            return ManaCheckResult.Failure("Caster has no mana component");

        ref var mana = ref caster.GetComponent<ManaComponent>();
        if (mana.CurrentMana < manaCost)
            return ManaCheckResult.Failure($"Insufficient mana (need {manaCost}, have {mana.CurrentMana})");

        return ManaCheckResult.Success();
    }

    private CooldownCheckResult CheckCooldown(Entity caster, string abilityId)
    {
        if (!caster.HasComponent<AbilityCooldownComponent>())
            return CooldownCheckResult.Failure("Caster has no cooldown component");

        ref var cooldown = ref caster.GetComponent<AbilityCooldownComponent>();
        if (cooldown.AbilityId != abilityId)
            return CooldownCheckResult.Failure("Cooldown component does not match ability");

        if (cooldown.IsOnCooldown)
            return CooldownCheckResult.Failure($"Ability is on cooldown ({cooldown.RemainingCooldownSeconds:F1}s remaining)");

        return CooldownCheckResult.Success();
    }

    private AbilityCommandResult ExecuteAbility(Entity caster, IAbilityDefinition abilityDef)
    {
        // Consume mana
        if (caster.HasComponent<ManaComponent>())
        {
            ref var mana = ref caster.GetComponent<ManaComponent>();
            if (!mana.ConsumeMana(abilityDef.ManaCost))
            {
                return AbilityCommandResult.Failure($"Insufficient mana (need {abilityDef.ManaCost}, have {mana.CurrentMana})");
            }
        }

        // Start cooldown
        if (caster.HasComponent<AbilityCooldownComponent>())
        {
            ref var cooldown = ref caster.GetComponent<AbilityCooldownComponent>();
            cooldown.StartCooldown();

            if (!caster.HasComponent<OnCooldownTag>())
                caster.AddComponent(new OnCooldownTag());
        }

        // Apply effects (Core logic - no Godot dependencies)
        // Effects are applied but actual visual/Godot reactions happen via signal bus
        ApplyEffects(caster, abilityDef);

        return AbilityCommandResult.Success($"Ability '{abilityDef.Name}' executed successfully");
    }

    /// <summary>
    /// Applies all effects from an ability definition to the caster and/or targets.
    /// This is pure Core logic - no Godot dependencies.
    /// Godot integration happens via AbilitySignalBus which listens to component changes.
    /// </summary>
    private void ApplyEffects(Entity caster, IAbilityDefinition abilityDef)
    {
        if (abilityDef.Effects == null || abilityDef.Effects.Count == 0)
            return;

        foreach (var effect in abilityDef.Effects)
        {
            // Core effect application - modifies ECS components
            // Visual/Godot reactions are handled separately via signals
            ApplyEffectToEntity(caster, effect);
        }
    }

    /// <summary>
    /// Applies a single effect to an entity by modifying its components.
    /// Pure Core logic - no Godot dependencies.
    /// </summary>
    private void ApplyEffectToEntity(Entity entity, IEffectDefinition effect)
    {
        // Effect application modifies ECS components
        // Godot layer observes these changes via signals or system queries
        
        if (effect is DamageEffect damageEffect)
        {
            ApplyDamageEffect(entity, damageEffect);
        }
        else if (effect is HealEffect healEffect)
        {
            ApplyHealEffect(entity, healEffect);
        }
        else if (effect is BuffEffect buffEffect)
        {
            ApplyBuffEffect(entity, buffEffect);
        }
        else if (effect is DebuffEffect debuffEffect)
        {
            ApplyDebuffEffect(entity, debuffEffect);
        }
        else if (effect is SummonEffect summonEffect)
        {
            ApplySummonEffect(entity, summonEffect);
        }
    }

    private void ApplyDamageEffect(Entity entity, DamageEffect effect)
    {
        if (entity.HasComponent<HealthComponent>())
        {
            ref var health = ref entity.GetComponent<HealthComponent>();
            float damage = effect.Amount;
            
            // Apply resistance if present
            if (entity.HasComponent<ResistanceComponent>())
            {
                var resistance = entity.GetComponent<ResistanceComponent>();
                damage *= (1f - resistance.DamageReduction);
            }
            
            health.Damage(damage);
            
            // Entity died
            if (!health.IsAlive && !entity.HasComponent<DeadTag>())
            {
                entity.AddComponent(new DeadTag());
            }
        }
    }

    private void ApplyHealEffect(Entity entity, HealEffect effect)
    {
        if (entity.HasComponent<HealthComponent>())
        {
            ref var health = ref entity.GetComponent<HealthComponent>();
            health.Heal(effect.Amount);
        }
        else if (entity.HasComponent<ManaComponent>())
        {
            // Some heal effects apply to mana
            ref var mana = ref entity.GetComponent<ManaComponent>();
            mana.RegenerateMana(effect.Amount);
        }
    }

    private void ApplyBuffEffect(Entity entity, BuffEffect effect)
    {
        // Buffs typically modify stats via components
        // Implementation depends on game-specific stat system
        // Could add/modify StatComponent, SpeedComponent, etc.
    }

    private void ApplyDebuffEffect(Entity entity, DebuffEffect effect)
    {
        // Similar to buff but with negative modifiers
    }

    private void ApplySummonEffect(Entity entity, SummonEffect effect)
    {
        // Summoning creates new entities
        // Implementation depends on game-specific summon system
    }
}

/// <summary>
/// Cooldown check result. Defined here since AbilityCommandTypes was trimmed.
/// </summary>
public readonly record struct CooldownCheckResult(
    bool IsValid,
    string? FailureReason
)
{
    public static CooldownCheckResult Success() => new(true, null);
    public static CooldownCheckResult Failure(string reason) => new(false, reason);
}
