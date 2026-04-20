using MoonBark.Framework.Core;

namespace MoonBark.Abilities.Core;

/// <summary>
/// Registers Abilities services with the Framework module registry.
/// </summary>
public sealed class AbilitiesModule : IFrameworkModule
{
    private readonly AbilityRegistry _registry;
    private readonly AbilityCooldownSystem _cooldownSystem;
    private readonly ManaRegenerationSystem _manaRegenSystem;

    public AbilitiesModule(AbilityRegistry registry, AbilityCooldownSystem cooldownSystem, ManaRegenerationSystem manaRegenSystem)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _cooldownSystem = cooldownSystem ?? throw new ArgumentNullException(nameof(cooldownSystem));
        _manaRegenSystem = manaRegenSystem ?? throw new ArgumentNullException(nameof(manaRegenSystem));
    }

    public void ConfigureServices(IServiceRegistry services)
    {
        services.Register(_registry);
        services.Register(_cooldownSystem);
        services.Register(_manaRegenSystem);
    }
}
