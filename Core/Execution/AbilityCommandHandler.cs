namespace MoonBark.Abilities.Core.Execution;

using MoonBark.Framework.Commands;
using MoonBark.Framework.Commands.Bus;
using MoonBark.Framework.Targeting;
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
    private readonly ITargetingValidator _targetingValidator;

    public AbilityCommandHandler(AbilityRegistry abilityRegistry, ITargetingValidator targetingValidator)
    {
        _abilityRegistry = abilityRegistry;
        _targetingValidator = targetingValidator;
    }

    /// <summary>
    /// Handles an ability command dispatched through the Framework CommandEventBus.
    /// Resolves the caster entity from the command's source.
    /// </summary>
    public async Task<CommandResult> HandleAsync(AbilityCommand command)
    {
        try
        {
            // Entity resolution from command source is caller responsibility.
            // For bus-based dispatch, callers should use the ECS-context Handle() overload.
            throw new NotSupportedException(
                "AbilityCommandHandler.HandleAsync requires ECS entity context. " +
                "Use Handle(command, world, caster) for ECS-context calls, or resolve " +
                "the target entity before bus dispatch.");
        }
        catch (Exception ex)
        {
            return CommandResult.FailureResult(ex.Message);
        }
    }

    /// <summary>
    /// Handles an ability command with explicit ECS entity context.
    /// This is the primary entry point for ability execution within an ECS world.
    /// </summary>
    /// <param name="command">The ability command.</param>
    /// <param name="world">The ECS world.</param>
    /// <param name="caster">The entity casting the ability.</param>
    /// <returns>The result of handling the command.</returns>
    public AbilityCommandResult Handle(AbilityCommand command, EntityStore world, Entity caster)
    {
        // Map Framework AbilityAction to Abilities AbilityAction
        Abilities.AbilityAction mappedAction = MapAction(command.AbilityAction);
        if (mappedAction == Abilities.AbilityAction.Cancel)
        {
            return CancelAbility(caster, command.AbilityId);
        }

        // Validate the command
        var validationResult = ValidateCommand(command.AbilityId, caster);
        if (!validationResult.IsValid)
        {
            return AbilityCommandResult.Failure(validationResult.FailureReason ?? "Validation failed");
        }

        // Get the ability definition
        var abilityDefinition = _abilityRegistry.Get(command.AbilityId);

        // Check mana
        var manaResult = CheckMana(caster, abilityDefinition.ManaCost);
        if (!manaResult.IsValid)
        {
            return AbilityCommandResult.Failure(manaResult.FailureReason ?? "Mana check failed");
        }

        // Validate targeting
        var targetingResult = TargetingValidation.ValidateTargeting(
            new Abilities.AbilityCommand(command.AbilityId, mappedAction, command.TargetEntityId, command.TargetPosition),
            caster,
            _targetingValidator);
        if (!targetingResult.CanTarget)
        {
            return AbilityCommandResult.Failure(targetingResult.FailureReason ?? "Invalid target");
        }

        // Execute the ability
        var executionResult = ExecuteAbility(caster, abilityDefinition);

        return executionResult;
    }

    private static Abilities.AbilityAction MapAction(MoonBark.Framework.Commands.AbilityAction action)
    {
        return action switch
        {
            MoonBark.Framework.Commands.AbilityAction.Cast => Abilities.AbilityAction.Activate,
            MoonBark.Framework.Commands.AbilityAction.Cancel => Abilities.AbilityAction.Cancel,
            MoonBark.Framework.Commands.AbilityAction.Toggle => Abilities.AbilityAction.Toggle,
            _ => Abilities.AbilityAction.Deactivate
        };
    }

    private AbilityCommandResult CancelAbility(Entity caster, string abilityId)
    {
        return AbilityCommandResult.Success($"Ability '{abilityId}' cancelled");
    }

    private CommandValidationResult ValidateCommand(string abilityId, Entity caster)
    {
        // Check if caster can cast abilities
        if (!caster.HasComponent<CanCastAbilitiesTag>())
        {
            return CommandValidationResult.Failure("Caster cannot cast abilities");
        }

        // Check if caster knows the ability
        if (caster.HasComponent<AbilityBookComponent>())
        {
            ref var book = ref caster.GetComponent<AbilityBookComponent>();
            if (!book.KnowsAbility(abilityId))
            {
                return CommandValidationResult.Failure("Caster does not know this ability");
            }
        }

        // Check if caster is learning
        if (caster.HasComponent<LearningAbilityTag>())
        {
            return CommandValidationResult.Failure("Caster is currently learning");
        }

        // Check if ability exists
        if (!_abilityRegistry.Exists(abilityId))
        {
            return CommandValidationResult.Failure("Ability does not exist");
        }

        return CommandValidationResult.Success();
    }

    private ManaCheckResult CheckMana(Entity caster, float manaCost)
    {
        if (!caster.HasComponent<ManaComponent>())
        {
            return ManaCheckResult.Failure("Caster has no mana component");
        }

        ref var mana = ref caster.GetComponent<ManaComponent>();
        if (mana.CurrentMana < manaCost)
        {
            return ManaCheckResult.Failure($"Insufficient mana (need {manaCost}, have {mana.CurrentMana})");
        }

        return ManaCheckResult.Success();
    }

    private AbilityCommandResult ExecuteAbility(Entity caster, IAbilityDefinition abilityDefinition)
    {
        // Consume mana
        if (caster.HasComponent<ManaComponent>())
        {
            ref var mana = ref caster.GetComponent<ManaComponent>();
            mana.ConsumeMana(abilityDefinition.ManaCost);
        }

        // Start cooldown
        if (caster.HasComponent<AbilityCooldownComponent>())
        {
            ref var cooldown = ref caster.GetComponent<AbilityCooldownComponent>();
            cooldown.StartCooldown();

            // Add OnCooldownTag if not present
            if (!caster.HasComponent<OnCooldownTag>())
            {
                caster.AddComponent(new OnCooldownTag());
            }
        }

        return AbilityCommandResult.Success($"Ability '{abilityDefinition.Name}' executed successfully");
    }
}
