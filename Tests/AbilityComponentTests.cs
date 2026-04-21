using Xunit;

namespace MoonBark.Abilities.Tests;
using MoonBark.Abilities.Core;
using MoonBark.Abilities.ECS;

public sealed class AbilityComponentTests
{
    [Fact]
    public void Constructor_AssignsValues()
    {
        AbilityComponent component = new("fireball", "Fireball", 30.0f, 5.0f);

        Assert.Equal("fireball", component.Id);
        Assert.Equal("Fireball", component.Name);
        Assert.Equal(30.0f, component.ManaCost);
        Assert.Equal(5.0f, component.BaseCooldownSeconds);
    }

    [Fact]
    public void Cooldown_StartAndReduce_TracksState()
    {
        AbilityCooldownComponent cooldown = new("fireball", 5.0f);

        cooldown.StartCooldown();
        cooldown.ReduceCooldown(2.0f);

        Assert.True(cooldown.IsOnCooldown);
        Assert.Equal(3.0f, cooldown.RemainingCooldownSeconds);
        Assert.Equal(0.6f, cooldown.GetCooldownProgress(), 3);
    }

    [Fact]
    public void Mana_ConsumeAndRegenerate_UpdatesBounds()
    {
        ManaComponent mana = new(40.0f, 50.0f);

        bool consumed = mana.ConsumeMana(10.0f);
        mana.RegenerateMana(25.0f);

        Assert.True(consumed);
        Assert.Equal(50.0f, mana.CurrentMana);
    }

    [Fact]
    public void LearningComponent_StartUpdateComplete_Remove_Works()
    {
        AbilityLearningComponent learning = new();

        bool started = learning.StartLearning("heal");
        float? progress = learning.UpdateProgress("heal", 0.4f);
        bool completed = learning.CompleteLearning("heal");
        bool removed = learning.RemoveLearning("heal");

        Assert.True(started);
        Assert.Equal(0.4f, progress);
        Assert.True(completed);
        Assert.True(removed);
        Assert.Empty(learning.GetAllLearningProgress());
    }
}
