namespace MoonBark.Abilities.Tests;

using MoonBark.Framework.Abilities;
using Xunit;

public sealed class AbilityDefinitionTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        AbilityDefinition def = new("fireball", "Fireball", 30.0f, 5.0f, AbilityKind.Damage);

        Assert.Equal("fireball", def.Id);
        Assert.Equal("Fireball", def.Name);
        Assert.Equal(30.0f, def.ManaCost);
        Assert.Equal(5.0f, def.BaseCooldownSeconds);
        Assert.Equal(AbilityKind.Damage, def.Kind);
    }

    [Fact]
    public void Constructor_AllAbilityKinds()
    {
        var kinds = new[] {
            AbilityKind.Buff,
            AbilityKind.Debuff,
            AbilityKind.Heal,
            AbilityKind.Damage,
            AbilityKind.Utility,
            AbilityKind.Summon
        };

        foreach (var kind in kinds)
        {
            AbilityDefinition def = new($"ability_{kind}", kind.ToString(), 10.0f, 1.0f, kind);
            Assert.Equal(kind, def.Kind);
        }
    }

    [Theory]
    [InlineData(0.0f, 1.0f)]
    [InlineData(100.0f, 300.0f)]
    [InlineData(0.5f, 0.25f)]
    public void Constructor_VariousManaAndCooldown(float manaCost, float cooldown)
    {
        AbilityDefinition def = new("test", "Test", manaCost, cooldown, AbilityKind.Damage);

        Assert.Equal(manaCost, def.ManaCost);
        Assert.Equal(cooldown, def.BaseCooldownSeconds);
    }
}
