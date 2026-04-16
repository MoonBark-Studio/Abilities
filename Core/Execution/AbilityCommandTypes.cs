namespace MoonBark.Abilities;

using MoonBark.Framework.Commands;
using MoonBark.Framework.Effects;

/// <summary>
/// Represents the action type for an ability command.
/// </summary>
public enum AbilityAction
{
    Activate,
    Deactivate,
    Trigger,
    Cancel
}

/// <summary>
/// Marker interface for ability commands. Extends ICommand to integrate with the framework command system.
/// </summary>
public interface IAbilityCommand : ICommand
{
    string AbilityId { get; }
    AbilityAction AbilityAction { get; }
    string? TargetEntityId { get; }
    string? TargetPosition { get; }
    TargetId Source { get; }
}

public readonly record struct AbilityCommand(
    string AbilityId,
    AbilityAction AbilityAction,
    string? TargetEntityId,
    string? TargetPosition,
    TargetId Source
) : IAbilityCommand
{
    public string CommandType => "ability";
    
    TargetId? ICommand.SubjectId => Source;
}

public readonly record struct CommandValidationResult(
    bool IsValid,
    string? FailureReason
)
{
    public static CommandValidationResult Success() => new(true, null);
    public static CommandValidationResult Failure(string reason) => new(false, reason);
}

public readonly record struct ManaCheckResult(
    bool IsValid,
    string? FailureReason
)
{
    public static ManaCheckResult Success() => new(true, null);
    public static ManaCheckResult Failure(string reason) => new(false, reason);
}

public readonly record struct CooldownCheckResult(
    bool IsValid,
    string? FailureReason
)
{
    public static CooldownCheckResult Success() => new(true, null);
    public static CooldownCheckResult Failure(string reason) => new(false, reason);
}

public readonly record struct AbilityExecuteResult(
    bool Succeeded,
    string? FailureReason = null
)
{
    public static AbilityExecuteResult Success() => new(true);
    public static AbilityExecuteResult Failure(string reason) => new(false, reason);
}

public readonly record struct EffectApplyResult(
    bool Succeeded,
    string? FailureReason = null
)
{
    public static EffectApplyResult Success() => new(true);
    public static EffectApplyResult Failure(string reason) => new(false, reason);
}

public readonly record struct AbilityCommandResult(
    bool Succeeded,
    string? Summary
)
{
    public static AbilityCommandResult Success(string summary) => new(true, summary);
    public static AbilityCommandResult Failure(string reason) => new(false, reason);
}