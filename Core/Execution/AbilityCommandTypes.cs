namespace MoonBark.Abilities.Core.Execution;

using System;
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
public interface IAbilityCommand
{
    string AbilityId { get; }
    AbilityAction AbilityAction { get; }
    string? TargetEntityId { get; }
    string? TargetPosition { get; }
}

public readonly record struct AbilityCommand(
    string AbilityId,
    AbilityAction AbilityAction,
    string? TargetEntityId,
    string? TargetPosition
) : IAbilityCommand, IEquatable<AbilityCommand>
{
    public bool Equals(AbilityCommand other) =>
        AbilityId == other.AbilityId &&
        AbilityAction == other.AbilityAction &&
        TargetEntityId == other.TargetEntityId &&
        TargetPosition == other.TargetPosition;
    public override bool Equals(object? obj) => obj is AbilityCommand other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(AbilityId, AbilityAction, TargetEntityId, TargetPosition);
    public static bool operator ==(AbilityCommand left, AbilityCommand right) => left.Equals(right);
    public static bool operator !=(AbilityCommand left, AbilityCommand right) => !left.Equals(right);
}

public readonly record struct CommandValidationResult(
    bool IsValid,
    string? FailureReason
) : IEquatable<CommandValidationResult>
{
    public static CommandValidationResult Success() => new(true, null);
    public static CommandValidationResult Failure(string reason) => new(false, reason);
    public bool Equals(CommandValidationResult other) => IsValid == other.IsValid && FailureReason == other.FailureReason;
    public override bool Equals(object? obj) => obj is CommandValidationResult other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(IsValid, FailureReason);
    public static bool operator ==(CommandValidationResult left, CommandValidationResult right) => left.Equals(right);
    public static bool operator !=(CommandValidationResult left, CommandValidationResult right) => !left.Equals(right);
}

public readonly record struct ManaCheckResult(
    bool IsValid,
    string? FailureReason
) : IEquatable<ManaCheckResult>
{
    public static ManaCheckResult Success() => new(true, null);
    public static ManaCheckResult Failure(string reason) => new(false, reason);
    public bool Equals(ManaCheckResult other) => IsValid == other.IsValid && FailureReason == other.FailureReason;
    public override bool Equals(object? obj) => obj is ManaCheckResult other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(IsValid, FailureReason);
    public static bool operator ==(ManaCheckResult left, ManaCheckResult right) => left.Equals(right);
    public static bool operator !=(ManaCheckResult left, ManaCheckResult right) => !left.Equals(right);
}

public readonly record struct CooldownCheckResult(
    bool IsValid,
    string? FailureReason
) : IEquatable<CooldownCheckResult>
{
    public static CooldownCheckResult Success() => new(true, null);
    public static CooldownCheckResult Failure(string reason) => new(false, reason);
    public bool Equals(CooldownCheckResult other) => IsValid == other.IsValid && FailureReason == other.FailureReason;
    public override bool Equals(object? obj) => obj is CooldownCheckResult other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(IsValid, FailureReason);
    public static bool operator ==(CooldownCheckResult left, CooldownCheckResult right) => left.Equals(right);
    public static bool operator !=(CooldownCheckResult left, CooldownCheckResult right) => !left.Equals(right);
}

public readonly record struct AbilityExecuteResult(
    bool Succeeded,
    string? FailureReason = null
) : IEquatable<AbilityExecuteResult>
{
    public static AbilityExecuteResult Success() => new(true);
    public static AbilityExecuteResult Failure(string reason) => new(false, reason);
    public bool Equals(AbilityExecuteResult other) => Succeeded == other.Succeeded && FailureReason == other.FailureReason;
    public override bool Equals(object? obj) => obj is AbilityExecuteResult other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Succeeded, FailureReason);
    public static bool operator ==(AbilityExecuteResult left, AbilityExecuteResult right) => left.Equals(right);
    public static bool operator !=(AbilityExecuteResult left, AbilityExecuteResult right) => !left.Equals(right);
}

public readonly record struct EffectApplyResult(
    bool Succeeded,
    string? FailureReason = null
) : IEquatable<EffectApplyResult>
{
    public static EffectApplyResult Success() => new(true);
    public static EffectApplyResult Failure(string reason) => new(false, reason);
    public bool Equals(EffectApplyResult other) => Succeeded == other.Succeeded && FailureReason == other.FailureReason;
    public override bool Equals(object? obj) => obj is EffectApplyResult other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Succeeded, FailureReason);
    public static bool operator ==(EffectApplyResult left, EffectApplyResult right) => left.Equals(right);
    public static bool operator !=(EffectApplyResult left, EffectApplyResult right) => !left.Equals(right);
}

public readonly record struct AbilityCommandResult(
    bool Succeeded,
    string? Summary
) : IEquatable<AbilityCommandResult>
{
    public static AbilityCommandResult Success(string summary) => new(true, summary);
    public static AbilityCommandResult Failure(string reason) => new(false, reason);
    public bool Equals(AbilityCommandResult other) => Succeeded == other.Succeeded && Summary == other.Summary;
    public override bool Equals(object? obj) => obj is AbilityCommandResult other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Succeeded, Summary);
    public static bool operator ==(AbilityCommandResult left, AbilityCommandResult right) => left.Equals(right);
    public static bool operator !=(AbilityCommandResult left, AbilityCommandResult right) => !left.Equals(right);
}