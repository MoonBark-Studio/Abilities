namespace MoonBark.Abilities.Core.Execution;

using MoonBark.Abilities.ECS;
using MoonBark.Framework.Commands;
using MoonBark.Framework.Commands.Bus;
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
            mana.ConsumeMana(abilityDef.ManaCost);
        }

        // Start cooldown
        if (caster.HasComponent<AbilityCooldownComponent>())
        {
            ref var cooldown = ref caster.GetComponent<AbilityCooldownComponent>();
            cooldown.StartCooldown();

            if (!caster.HasComponent<OnCooldownTag>())
                caster.AddComponent(new OnCooldownTag());
        }

        return AbilityCommandResult.Success($"Ability '{abilityDef.Name}' executed successfully");
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
