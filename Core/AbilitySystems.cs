namespace MoonBark.Abilities;

using Friflo.Engine.ECS;

/// <summary>
/// System that manages ability cooldowns for all entities.
/// </summary>
public sealed class AbilityCooldownSystem
{
    /// <summary>
    /// Updates all ability cooldowns.
    /// </summary>
    /// <param name="world">The ECS world containing entities to process.</param>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    public void Update(EntityStore world, float deltaTime)
    {
        var query = world.Query<AbilityCooldownComponent>();

        foreach (var entity in query.Entities)
        {
            ref var cooldown = ref entity.GetComponent<AbilityCooldownComponent>();

            if (cooldown.IsOnCooldown)
            {
                cooldown.ReduceCooldown(deltaTime);

                // Remove OnCooldownTag if cooldown is complete
                if (!cooldown.IsOnCooldown && entity.HasComponent<OnCooldownTag>())
                {
                    entity.RemoveComponent<OnCooldownTag>();
                }
            }
        }
    }
}

/// <summary>
/// System that regenerates mana for all entities.
/// </summary>
public sealed class ManaRegenerationSystem
{
    private const float DefaultManaRegenRate = 5.0f; // Mana per second

    /// <summary>
    /// Regenerates mana for all entities.
    /// </summary>
    /// <param name="world">The ECS world containing entities to process.</param>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    public void Update(EntityStore world, float deltaTime)
    {
        var query = world.Query<ManaComponent>();

        foreach (var entity in query.Entities)
        {
            ref var mana = ref entity.GetComponent<ManaComponent>();
            mana.RegenerateMana(DefaultManaRegenRate * deltaTime);
        }
    }
}

