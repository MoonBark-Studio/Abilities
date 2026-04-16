namespace MoonBark.Abilities;

using MoonBark.Framework.Targeting;
using Friflo.Engine.ECS;

/// <summary>
/// Helper class for validating ability targeting.
/// </summary>
public static class TargetingValidation
{
    /// <summary>
    /// Validates the targeting for an ability command.
    /// </summary>
    /// <param name="command">The ability command to validate.</param>
    /// <param name="caster">The entity casting the ability.</param>
    /// <param name="targetingValidator">The targeting validator to use.</param>
    /// <returns>The targeting result.</returns>
    public static TargetingResult ValidateTargeting(
        AbilityCommand command,
        Entity caster,
        ITargetingValidator targetingValidator)
    {
        // If there's a target entity, validate entity targeting
        if (!string.IsNullOrWhiteSpace(command.TargetEntityId))
        {
            // NOTE: This is a simplified implementation.
            // In a real implementation, you would resolve the target entity from the ID
            // and call targetingValidator.CanTarget(caster, target, command.AbilityId)

            // For now, assume targeting is valid
            return TargetingResult.Success();
        }

        // If there's a target position, validate position targeting
        if (!string.IsNullOrWhiteSpace(command.TargetPosition))
        {
            // NOTE: This is a simplified implementation.
            // In a real implementation, you would parse the position
            // and call targetingValidator.CanTargetPosition(caster, position, command.AbilityId)

            // For now, assume targeting is valid
            return TargetingResult.Success();
        }

        // No target specified - assume self-targeting or area effect
        return TargetingResult.Success();
    }
}
