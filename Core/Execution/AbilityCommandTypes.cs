namespace MoonBark.Abilities.Core.Execution;

/// Result type returned by the ability command handler.
public readonly record struct AbilityCommandResult(
    bool Succeeded,
    string? Summary
)
{
    public static AbilityCommandResult Success(string summary) => new(true, summary);
    public static AbilityCommandResult Failure(string reason) => new(false, reason);
}

/// Validation result for a command.
public readonly record struct CommandValidationResult(
    bool IsValid,
    string? FailureReason
)
{
    public static CommandValidationResult Success() => new(true, null);
    public static CommandValidationResult Failure(string reason) => new(false, reason);
}

/// Mana check result.
public readonly record struct ManaCheckResult(
    bool IsValid,
    string? FailureReason
)
{
    public static ManaCheckResult Success() => new(true, null);
    public static ManaCheckResult Failure(string reason) => new(false, reason);
}
