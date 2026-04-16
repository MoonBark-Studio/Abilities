namespace MoonBark.Abilities.Tests;

using Xunit;

public sealed class AbilityRegistryTests
{
    [Fact]
    public void Register_NewAbility_AddsToRegistry()
    {
        AbilityRegistry registry = new();
        AbilityDefinition ability = new("fireball", "Fireball", 30.0f, 5.0f, AbilityKind.Damage);

        registry.Register(ability);

        Assert.Equal(1, registry.Count);
        Assert.True(registry.Exists("fireball"));
    }

    [Fact]
    public void Get_RegisteredAbility_ReturnsAbility()
    {
        AbilityRegistry registry = new();
        IAbilityDefinition ability = new AbilityDefinition("fireball", "Fireball", 30.0f, 5.0f, AbilityKind.Damage);
        registry.Register(ability);

        IAbilityDefinition result = registry.Get("fireball");

        Assert.Equal("fireball", result.Id);
        Assert.Equal("Fireball", result.Name);
        Assert.Equal(30.0f, result.ManaCost);
        Assert.Equal(5.0f, result.BaseCooldownSeconds);
    }

    [Fact]
    public void Get_UnregisteredAbility_ThrowsKeyNotFoundException()
    {
        AbilityRegistry registry = new();

        Assert.Throws<KeyNotFoundException>(() => registry.Get("nonexistent"));
    }

    [Fact]
    public void Register_DuplicateAbility_ThrowsArgumentException()
    {
        AbilityRegistry registry = new();
        IAbilityDefinition ability1 = new AbilityDefinition("fireball", "Fireball", 30.0f, 5.0f, AbilityKind.Damage);
        IAbilityDefinition ability2 = new AbilityDefinition("fireball", "Fireball 2", 25.0f, 4.0f, AbilityKind.Damage);
        registry.Register(ability1);

        Assert.Throws<ArgumentException>(() => registry.Register(ability2));
    }

    [Fact]
    public void Exists_RegisteredAbility_ReturnsTrue()
    {
        AbilityRegistry registry = new();
        registry.Register(new AbilityDefinition("fireball", "Fireball", 30.0f, 5.0f, AbilityKind.Damage));

        Assert.True(registry.Exists("fireball"));
        Assert.False(registry.Exists("iceball"));
    }

    [Fact]
    public void GetAllAbilityIds_ReturnsAllRegisteredIds()
    {
        AbilityRegistry registry = new();
        registry.Register(new AbilityDefinition("fireball", "Fireball", 30.0f, 5.0f, AbilityKind.Damage));
        registry.Register(new AbilityDefinition("iceball", "Iceball", 25.0f, 4.0f, AbilityKind.Damage));

        var ids = registry.GetAllAbilityIds();

        Assert.Contains("fireball", ids);
        Assert.Contains("iceball", ids);
        Assert.Equal(2, ids.Count);
    }

    [Fact]
    public void Count_EmptyRegistry_ReturnsZero()
    {
        AbilityRegistry registry = new();

        Assert.Equal(0, registry.Count);
    }

    [Fact]
    public void Count_MultipleAbilities_ReturnsCorrectCount()
    {
        AbilityRegistry registry = new();
        registry.Register(new AbilityDefinition("a1", "Ability 1", 10.0f, 1.0f, AbilityKind.Damage));
        registry.Register(new AbilityDefinition("a2", "Ability 2", 20.0f, 2.0f, AbilityKind.Damage));
        registry.Register(new AbilityDefinition("a3", "Ability 3", 30.0f, 3.0f, AbilityKind.Damage));

        Assert.Equal(3, registry.Count);
    }
}
