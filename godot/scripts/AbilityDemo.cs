using Godot;
using MoonBark.Abilities.Core;
using MoonBark.Abilities.Godot;
using MoonBark.Abilities.ECS;
using Friflo.Engine.ECS;

namespace MoonBark.Abilities.Godot.Examples;

/// <summary>
/// Example: How to use the Abilities system in Godot.
/// This demonstrates the complete workflow from setup to execution.
/// </summary>
public partial class AbilityDemo : Node3D
{
    private WorldManager? _worldManager;
    private GodotAbilityNode? _playerAbility;
    
    public override void _Ready()
    {
        GD.Print("=== Abilities System Demo ===");
        
        // Method 1: Use existing WorldManager in scene
        _worldManager = GetNodeOrNull<WorldManager>("WorldManager");
        
        if (_worldManager == null)
        {
            // Method 2: Create WorldManager programmatically
            _worldManager = new WorldManager(autoUpdateSystems: true, manaRegenRate: 5f);
            AddChild(_worldManager);
            _worldManager.InitializeWorld();
        }
        
        // Register abilities
        SetupAbilities();
        
        // Create player with abilities
        SetupPlayer();
        
        GD.Print("Demo ready! Press SPACE to cast fireball.");
    }
    
    private void SetupAbilities()
    {
        if (_worldManager?.Registry == null)
            return;
        
        // Fireball: Offensive ability with damage
        var fireballEffects = new IEffectDefinition[]
        {
            new DamageEffect(amount: 25f, isMagical: true)
        };
        
        var fireball = new AbilityDefinition(
            id: "fireball",
            name: "Fireball",
            manaCost: 20f,
            baseCooldownSeconds: 3f,
            kind: AbilityKind.Damage,
            requiresTarget: true,
            canTargetSelf: false,
            range: 15f,
            effects: fireballEffects
        );
        
        _worldManager.RegisterAbility(fireball);
        
        // Heal: Support ability
        var healEffects = new IEffectDefinition[]
        {
            new HealEffect(amount: 30f, isManaHeal: false)
        };
        
        var heal = new AbilityDefinition(
            id: "heal",
            name: "Heal",
            manaCost: 25f,
            baseCooldownSeconds: 5f,
            kind: AbilityKind.Heal,
            requiresTarget: true,
            canTargetSelf: true,
            range: 10f,
            effects: healEffects
        );
        
        _worldManager.RegisterAbility(heal);
        
        // Shield: Defensive buff
        var shieldEffects = new IEffectDefinition[]
        {
            new BuffEffect(
                buffId: "shield",
                duration: 10f,
                stats: new BuffStats { ArmorBonus = 5f, ResistanceBonus = 0.2f }
            )
        };
        
        var shield = new AbilityDefinition(
            id: "shield",
            name: "Magic Shield",
            manaCost: 30f,
            baseCooldownSeconds: 15f,
            kind: AbilityKind.Buff,
            requiresTarget: false,
            canTargetSelf: true,
            range: 0f,
            effects: shieldEffects
        );
        
        _worldManager.RegisterAbility(shield);
        
        GD.Print($"Registered {_worldManager.Registry.Count} abilities");
    }
    
    private void SetupPlayer()
    {
        // Create player entity with abilities
        _playerAbility = new GodotAbilityNode
        {
            AbilityId = "fireball",
            MaxMana = 100f,
            InitialMana = 100f,
            AutoRegister = true,
            Position = new Vector3(0, 0, 0)
        };
        
        // Add to WorldManager (which adds to scene tree)
        _worldManager?.AddChild(_playerAbility);
        
        // Connect signals
        _playerAbility.ConnectGodotAbilitySignals();
        
        // Give player multiple abilities
        if (_playerAbility.Entity != null && _worldManager?.Registry != null)
        {
            var book = _playerAbility.Entity.Value.GetComponent<AbilityBookComponent>();
            book.LearnAbility("fireball");
            book.LearnAbility("heal");
            book.LearnAbility("shield");
            _playerAbility.Entity.Value.SetComponent(book);
        }
        
        // Add health component
        if (_playerAbility.Entity != null)
        {
            _playerAbility.Entity.Value.AddComponent(new HealthComponent(100f, 100f));
        }
        
        GD.Print("Player created with 3 abilities");
    }
    
    public override void _Process(double delta)
    {
        // Cast fireball on SPACE
        if (Input.IsActionJustPressed("ui_accept"))
        {
            CastFireball();
        }
        
        // Cast heal on H
        if (Input.IsActionJustPressed("ui_home"))
        {
            CastHeal();
        }
        
        // Cast shield on S
        if (Input.IsActionJustPressed("ui_select"))
        {
            CastShield();
        }
        
        // Display status
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            DisplayStatus();
        }
    }
    
    private void CastFireball()
    {
        if (_playerAbility == null)
            return;
        
        // Method 1: Use GodotAbilityNode directly
        bool success = _playerAbility.CastAbility();
        
        if (success)
        {
            GD.Print("Fireball cast!");
        }
        else
        {
            GD.Print("Fireball failed (no mana or on cooldown)");
        }
    }
    
    private void CastHeal()
    {
        if (_playerAbility?.Entity == null || _worldManager?.CommandHandler == null)
            return;
        
        // Method 2: Use command handler directly
        var command = new AbilityCommand("heal", AbilityAction.Cast);
        var result = _worldManager.CommandHandler.Handle(
            command, 
            _worldManager.World!.Value, 
            _playerAbility.Entity.Value
        );
        
        GD.Print(result.Succeeded ? "Heal cast!" : $"Heal failed: {result.Summary}");
    }
    
    private void CastShield()
    {
        if (_playerAbility == null)
            return;
        
        // Method 3: Use WorldManager
        bool success = _worldManager?.ExecuteAbility("shield", _playerAbility) ?? false;
        
        GD.Print(success ? "Shield activated!" : "Shield failed");
    }
    
    private void DisplayStatus()
    {
        if (_playerAbility?.Entity == null)
            return;
        
        var entity = _playerAbility.Entity.Value;
        
        string status = "=== Player Status ===\n";
        
        if (entity.HasComponent<ManaComponent>())
        {
            var mana = entity.GetComponent<ManaComponent>();
            status += $"Mana: {mana.CurrentMana:F0}/{mana.MaxMana:F0}\n";
        }
        
        if (entity.HasComponent<HealthComponent>())
        {
            var health = entity.GetComponent<HealthComponent>();
            status += $"Health: {health.CurrentHealth:F0}/{health.MaxHealth:F0}\n";
        }
        
        if (entity.HasComponent<AbilityCooldownComponent>())
        {
            var cd = entity.GetComponent<AbilityCooldownComponent>();
            status += $"Cooldown: {cd.AbilityId} - {cd.RemainingCooldownSeconds:F1}s\n";
        }
        
        if (entity.HasComponent<AbilityBookComponent>())
        {
            var book = entity.GetComponent<AbilityBookComponent>();
            status += $"Known abilities: {book.KnownAbilityCount}\n";
        }
        
        GD.Print(status);
    }
}

/// <summary>
/// Extension methods for easy signal connection.
/// </summary>
public static class GodotAbilityNodeExtensions
{
    /// <summary>
    /// Connect all GodotAbilityNode signals to the target node.
    /// </summary>
    public static void ConnectGodotAbilitySignals(this GodotAbilityNode abilityNode, Node target)
    {
        abilityNode.Connect(GodotAbilityNode.SignalName.AbilityCast, 
            Callable.From<string, Node3D>((abilityId, caster) => 
                GD.Print($"[Signal] AbilityCast: {abilityId}")));
        
        abilityNode.Connect(GodotAbilityNode.SignalName.AbilityCastFailed, 
            Callable.From<string, string, Node3D>((abilityId, reason, caster) => 
                GD.Print($"[Signal] AbilityCastFailed: {abilityId} - {reason}")));
        
        abilityNode.Connect(GodotAbilityNode.SignalName.ManaChanged, 
            Callable.From<float, float, Node3D>((current, max, caster) => 
                GD.Print($"[Signal] ManaChanged: {current:F0}/{max:F0}")));
        
        abilityNode.Connect(GodotAbilityNode.SignalName.CooldownChanged, 
            Callable.From<string, float, float, Node3D>((abilityId, remaining, max, caster) => 
                GD.Print($"[Signal] CooldownChanged: {abilityId} - {remaining:F1}/{max:F1}s")));
    }
    
    /// <summary>
    /// Connect all GodotAbilityNode signals to self.
    /// </summary>
    public static void ConnectGodotAbilitySignals(this GodotAbilityNode abilityNode)
    {
        abilityNode.ConnectGodotAbilitySignals(abilityNode);
    }
}