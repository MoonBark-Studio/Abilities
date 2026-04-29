using Godot;
using MoonBark.Abilities.Core;
using MoonBark.Abilities.ECS;
using MoonBark.Framework.Effects;
using Friflo.Engine.ECS;

namespace MoonBark.Abilities.Godot;

/// <summary>
/// Applies ability effects to Godot nodes/entities.
/// This is the bridge between Core IEffectDefinition and Godot Node operations.
/// </summary>
/// <remarks>
/// Core defines WHAT effects do (IEffectDefinition).
/// Godot layer defines HOW they're applied to Nodes/Entities.
/// </remarks>
[GlobalClass]
public partial class GodotEffectApplier : Node
{
    // ===== Effect Application Methods =====
    
    /// <summary>
    /// Apply an effect definition to a target entity/node.
    /// </summary>
    public void ApplyEffect(IEffectDefinition effect, Entity targetEntity, Node3D? targetNode = null)
    {
        // Dispatch based on effect type
        switch (effect)
        {
            case DamageEffect damageEffect:
                ApplyDamage(damageEffect, targetEntity, targetNode);
                break;
            case HealEffect healEffect:
                ApplyHeal(healEffect, targetEntity, targetNode);
                break;
            case BuffEffect buffEffect:
                ApplyBuff(buffEffect, targetEntity, targetNode);
                break;
            case DebuffEffect debuffEffect:
                ApplyDebuff(debuffEffect, targetEntity, targetNode);
                break;
            case SummonEffect summonEffect:
                ApplySummon(summonEffect, targetEntity, targetNode);
                break;
            default:
                GD.PushWarning($"GodotEffectApplier: Unknown effect type {effect.GetType().Name}");
                break;
        }
    }
    
    /// <summary>
    /// Apply multiple effects.
    /// </summary>
    public void ApplyEffects(IEnumerable<IEffectDefinition> effects, Entity targetEntity, Node3D? targetNode = null)
    {
        foreach (var effect in effects)
        {
            ApplyEffect(effect, targetEntity, targetNode);
        }
    }
    
    // ===== Specific Effect Handlers =====
    
    private void ApplyDamage(DamageEffect effect, Entity targetEntity, Node3D? targetNode)
    {
        // Apply to Health component if present
        if (targetEntity.HasComponent<HealthComponent>())
        {
            ref var health = ref targetEntity.GetComponent<HealthComponent>();
            health.CurrentHealth -= effect.Amount;
            
            GD.Print($"GodotEffectApplier: Applied {effect.Amount} damage to entity {targetEntity.Index}, health now {health.CurrentHealth}");
            
            // Emit signal or trigger Godot animation
            if (targetNode != null)
            {
                // Could trigger damage popup, flash effect, etc.
                TriggerDamageEffect(targetNode, effect.Amount);
            }
            
            // Check for death
            if (health.CurrentHealth <= 0)
            {
                OnEntityDeath(targetEntity, targetNode);
            }
        }
    }
    
    private void ApplyHeal(HealEffect effect, Entity targetEntity, Node3D? targetNode)
    {
        if (targetEntity.HasComponent<HealthComponent>())
        {
            ref var health = ref targetEntity.GetComponent<HealthComponent>();
            health.CurrentHealth = Mathf.Min(health.CurrentHealth + effect.Amount, health.MaxHealth);
            
            GD.Print($"GodotEffectApplier: Applied {effect.Amount} heal to entity {targetEntity.Index}, health now {health.CurrentHealth}");
            
            if (targetNode != null)
            {
                TriggerHealEffect(targetNode, effect.Amount);
            }
        }
        else if (targetEntity.HasComponent<ManaComponent>())
        {
            // Some heal effects might apply to mana
            ref var mana = ref targetEntity.GetComponent<ManaComponent>();
            mana.RegenerateMana(effect.Amount);
        }
    }
    
    private void ApplyBuff(BuffEffect effect, Entity targetEntity, Node3D? targetNode)
    {
        GD.Print($"GodotEffectApplier: Applied buff '{effect.BuffId}' to entity {targetEntity.Index}");
        
        // Add buff component or modify stats
        // Implementation depends on game-specific buff system
        
        if (targetNode != null)
        {
            TriggerBuffEffect(targetNode, effect);
        }
    }
    
    private void ApplyDebuff(DebuffEffect effect, Entity targetEntity, Node3D? targetNode)
    {
        GD.Print($"GodotEffectApplier: Applied debuff '{effect.DebuffId}' to entity {targetEntity.Index}");
        
        // Similar to buff but with negative modifiers
        
        if (targetNode != null)
        {
            TriggerDebuffEffect(targetNode, effect);
        }
    }
    
    private void ApplySummon(SummonEffect effect, Entity targetEntity, Node3D? targetNode)
    {
        GD.Print($"GodotEffectApplier: Summoning '{effect.SummonId}' at entity {targetEntity.Index}");
        
        // Spawn new entity/unit
        // Implementation depends on game-specific summon system
        
        if (targetNode != null && effect.SummonScene != null)
        {
            var instance = effect.SummonScene.Instantiate<Node3D>();
            if (instance != null)
            {
                targetNode.GetParent()?.AddChild(instance);
                instance.GlobalPosition = targetNode.GlobalPosition + effect.Offset;
            }
        }
    }
    
    // ===== Visual Effect Triggers =====
    
    private void TriggerDamageEffect(Node3D node, float amount)
    {
        // Could spawn floating damage text, flash red, play sound, etc.
        // Example: Create a simple flash effect
        if (node is MeshInstance3D mesh)
        {
            // Store original material, apply red tint briefly
            // Implementation depends on specific needs
        }
    }
    
    private void TriggerHealEffect(Node3D node, float amount)
    {
        // Green flash, particle effect, etc.
    }
    
    private void TriggerBuffEffect(Node3D node, BuffEffect effect)
    {
        // Blue glow, particle effect, etc.
    }
    
    private void TriggerDebuffEffect(Node3D node, DebuffEffect effect)
    {
        // Purple/pink glow, particle effect, etc.
    }
    
    private void OnEntityDeath(Entity entity, Node3D? node)
    {
        GD.Print($"GodotEffectApplier: Entity {entity.Index} died");
        
        if (node != null)
        {
            // Play death animation, spawn loot, etc.
            // QueueFree or play death sequence
        }
        
        // Emit signal for other systems
        // GetTree().Root.EmitSignal(...)
    }
    
    // ===== Utility =====
    
    /// <summary>
    /// Calculate damage with resistances/weaknesses.
    /// </summary>
    public float CalculateDamage(float baseDamage, Entity targetEntity)
    {
        float multiplier = 1.0f;
        
        // Check for resistance/weakness components
        if (targetEntity.HasComponent<ResistanceComponent>())
        {
            var resistance = targetEntity.GetComponent<ResistanceComponent>();
            multiplier *= (1.0f - resistance.DamageReduction);
        }
        
        return baseDamage * multiplier;
    }
}

// ===== Effect Type Definitions (Godot-specific extensions) =====

// These extend or wrap Core effect definitions with Godot-specific data

[GlobalClass]
public partial class GodotDamageEffect : Resource
{
    [Export] public float Amount { get; set; }
    [Export] public bool IsMagical { get; set; }
    [Export] public PackedScene? DamagePopup { get; set; }
}

[GlobalClass]
public partial class GodotHealEffect : Resource
{
    [Export] public float Amount { get; set; }
    [Export] public PackedScene? HealEffect { get; set; }
}

[GlobalClass]
public partial class GodotSummonEffect : Resource
{
    [Export] public string SummonId { get; set; } = string.Empty;
    [Export] public PackedScene? SummonScene { get; set; }
    [Export] public Vector3 Offset { get; set; }
}