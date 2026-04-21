namespace MoonBark.Abilities.Core.Execution;

/// Result of targeting validation.
public readonly record struct TargetingResult(
    bool CanTarget,
    string? FailureReason
)
{
    public static TargetingResult Success() => new(true, null);
    public static TargetingResult Failure(string reason) => new(false, reason);
}
