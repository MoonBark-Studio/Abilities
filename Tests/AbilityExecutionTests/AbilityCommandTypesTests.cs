using MoonBark.Abilities.Core;
using MoonBark.Abilities.Core.Execution;
using MoonBark.Abilities.ECS;
using Xunit;

namespace MoonBark.Abilities.Tests;

public sealed class AbilityCommandTypesTests
{
    [Fact]
    public void CommandValidationResult_Failure_PreservesReason()
    {
        CommandValidationResult result = CommandValidationResult.Failure("invalid");

        Assert.False(result.IsValid);
        Assert.Equal("invalid", result.FailureReason);
    }

    [Fact]
    public void AbilityCommandResult_Success_SetsSucceeded()
    {
        AbilityCommandResult result = AbilityCommandResult.Success("ok");

        Assert.True(result.Succeeded);
        Assert.Equal("ok", result.Summary);
    }

    [Fact]
    public void AbilityCommandResult_Failure_PreservesSummary()
    {
        AbilityCommandResult result = AbilityCommandResult.Failure("denied");

        Assert.False(result.Succeeded);
        Assert.Equal("denied", result.Summary);
    }

    [Fact]
    public void ManaCheckResult_Failure_PreservesReason()
    {
        ManaCheckResult result = ManaCheckResult.Failure("insufficient mana");

        Assert.False(result.IsValid);
        Assert.Equal("insufficient mana", result.FailureReason);
    }
}
