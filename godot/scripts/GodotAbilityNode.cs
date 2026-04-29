using Godot;
using MoonBark.Abilities.Core;
using MoonBark.Abilities.ECS;
using Friflo.Engine.ECS;

namespace MoonBark.Abilities.Godot;

/// <summary>
/// Thin Godot node that wraps an ECS entity with ability functionality.
/// This is the primary bridge between Godot scene nodes and the ECS ability system.
/// Logic lives in Core; this is just thin wiring.
/// </summary>
[GlobalClass]
public partial class GodotAbilityNode : Node3D
{
    [Export] public string AbilityId { get; set; } = string.Empty;
    [Export] public float MaxMana { get; set; } = 100f;
    [Export] public float InitialMana { get; set; } = 100f;
    [Export] public bool AutoRegister { get; set; } = true;
    
    private Entity? _entity;
    private EntityStore? _world;
    private AbilityRegistry? _registry;
    private AbilityCommandHandler? _commandHandler;
    
    /// <summary>ECS entity backing this node.</summary>
    public Entity? Entity => _entity;
    
    /// <summary>The world/store this entity belongs to.</summary>
    public EntityStore? World => _world;
    
    /// <summary>Signal emitted when ability is cast successfully.</summary>
    [Signal] public delegate void AbilityCastEventHandler(string abilityId, Node3D caster);
    
    /// <summary>Signal emitted when ability cast fails.</summary>
    [Signal] public delegate void AbilityCastFailedEventHandler(string abilityId, string reason, Node3D caster);
    
    /// <summary>Signal emitted when mana changes.</summary>
    [Signal] public delegate void ManaChangedEventHandler(float current, float max, Node3D caster);
    
    /// <summary>Signal emitted when cooldown state changes.</summary>
    [Signal] public delegate void CooldownChangedEventHandler(string abilityId, float remaining, float max, Node3D caster);
    
    public GodotAbilityNode() { }
    
    public GodotAbilityNode(EntityStore world, AbilityRegistry registry, string abilityId)
    {
        _world = world;
        _registry = registry;
        AbilityId = abilityId;
    }
    
    public override void _Ready()
    {
        base._Ready();
        
        if (_world == null)
        {
            // Try to find a world manager in parent hierarchy
            var worldManager = FindParentWorldManager();
            if (worldManager == null)
            {
                GD.PushError($"GodotAbilityNode: No EntityStore found for {Name}. Set world via Initialize().");
                return;
            }
            _world = worldManager.World;
            _registry = worldManager.Registry;
        }
        
        if (_world == null || _registry == null)
        {
            GD.PushError($"GodotAbilityNode: World or Registry is null for {Name}");
            return;
        }
        
        _commandHandler = new AbilityCommandHandler(_registry);
        
        // Create ECS entity
        _entity = _world.CreateEntity();
        
        // Add ability component
        if (_registry.Exists(AbilityId))
        {
            var def = _registry.Get(AbilityId);
            _entity.AddComponent(new AbilityComponent(AbilityId, def.Name, def.ManaCost, def.BaseCooldownSeconds));
        }
        else
        {
            _entity.AddComponent(new AbilityComponent(AbilityId, AbilityId, 0f, 0f));
        }
        
        // Add mana
        _entity.AddComponent(new ManaComponent(InitialMana, MaxMana));
        
        // Add cooldown tracking
        _entity.AddComponent(new AbilityCooldownComponent(AbilityId, 
            _registry.Exists(AbilityId) ? _registry.Get(AbilityId).BaseCooldownSeconds : 0f));
        
        // Add ability book
        var book = new AbilityBookComponent();
        book.LearnAbility(AbilityId);
        _entity.AddComponent(book);
        
        // Add cast tag
        _entity.AddComponent(new CanCastAbilitiesTag());
        
        // Auto-register with world systems if needed
        if (AutoRegister)
        {
            // Systems query all entities with components, so no explicit registration needed
            GD.Print($"GodotAbilityNode '{Name}' ready with entity {_entity.Value.Index}");
        }
    }
    
    public void Initialize(EntityStore world, AbilityRegistry registry, AbilityCommandHandler? handler = null)
    {
        _world = world;
        _registry = registry;
        _commandHandler = handler ?? new AbilityCommandHandler(registry);
    }
    
    /// <summary>
    /// Cast the ability. Returns true if cast started successfully.
    /// </summary>
    public bool CastAbility()
    {
        if (_entity == null || _world == null || _commandHandler == null || string.IsNullOrEmpty(AbilityId))
        {
            GD.PushError($"GodotAbilityNode '{Name}': Cannot cast - not initialized");
            EmitSignal(SignalName.AbilityCastFailed, AbilityId, "Not initialized", this);
            return false;
        }
        
        var command = new AbilityCommand(AbilityId, AbilityAction.Cast);
        var result = _commandHandler.Handle(command, _world.Value, _entity.Value);
        
        if (result.Succeeded)
        {
            EmitSignal(SignalName.AbilityCast, AbilityId, this);
            EmitManaSignal();
            EmitCooldownSignal();
            return true;
        }
        else
        {
            EmitSignal(SignalName.AbilityCastFailed, AbilityId, result.Summary ?? "Unknown error", this);
            return false;
        }
    }
    
    /// <summary>
    /// Cancel the ability (if it has a channel time or ongoing effect).
    /// </summary>
    public bool CancelAbility()
    {
        if (_entity == null || _world == null || _commandHandler == null || string.IsNullOrEmpty(AbilityId))
            return false;
        
        var command = new AbilityCommand(AbilityId, AbilityAction.Cancel);
        var result = _commandHandler.Handle(command, _world.Value, _entity.Value);
        return result.Succeeded;
    }
    
    /// <summary>
    /// Update cooldowns. Call this from _Process or _PhysicsProcess.
    /// </summary>
    public void UpdateCooldowns(float deltaTime)
    {
        if (_entity == null || !_entity.Value.HasComponent<AbilityCooldownComponent>())
            return;
        
        ref var cooldown = ref _entity.Value.GetComponent<AbilityCooldownComponent>();
        float prevRemaining = cooldown.RemainingCooldownSeconds;
        
        cooldown.ReduceCooldown(deltaTime);
        
        if (cooldown.RemainingCooldownSeconds != prevRemaining)
        {
            EmitCooldownSignal();
            
            // Manage OnCooldownTag
            if (_entity.Value.HasComponent<OnCooldownTag>() && !cooldown.IsOnCooldown)
            {
                _entity.Value.RemoveComponent<OnCooldownTag>();
            }
            else if (!_entity.Value.HasComponent<OnCooldownTag>() && cooldown.IsOnCooldown)
            {
                _entity.Value.AddComponent(new OnCooldownTag());
            }
        }
    }
    
    /// <summary>
    /// Regenerate mana. Call this from _Process or _PhysicsProcess.
    /// </summary>
    public void RegenerateMana(float deltaTime, float regenRate = 5f)
    {
        if (_entity == null || !_entity.Value.HasComponent<ManaComponent>())
            return;
        
        ref var mana = ref _entity.Value.GetComponent<ManaComponent>();
        mana.RegenerateMana(regenRate * deltaTime);
        EmitManaSignal();
    }
    
    /// <summary>
    /// Check if the ability is currently on cooldown.
    /// </summary>
    public bool IsOnCooldown()
    {
        return _entity?.Value.HasComponent<AbilityCooldownComponent>() == true &&
               _entity.Value.GetComponent<AbilityCooldownComponent>().IsOnCooldown;
    }
    
    /// <summary>
    /// Get remaining cooldown time.
    /// </summary>
    public float GetCooldownRemaining()
    {
        if (_entity?.Value.HasComponent<AbilityCooldownComponent>() != true)
            return 0f;
        return _entity.Value.GetComponent<AbilityCooldownComponent>().RemainingCooldownSeconds;
    }
    
    /// <summary>
    /// Get current mana.
    /// </summary>
    public float GetCurrentMana()
    {
        if (_entity?.Value.HasComponent<ManaComponent>() != true)
            return 0f;
        return _entity.Value.GetComponent<ManaComponent>().CurrentMana;
    }
    
    /// <summary>
    /// Get max mana.
    /// </summary>
    public float GetMaxMana()
    {
        if (_entity?.Value.HasComponent<ManaComponent>() != true)
            return 0f;
        return _entity.Value.GetComponent<ManaComponent>().MaxMana;
    }
    
    /// <summary>
    /// Check if can cast ability right now.
    /// </summary>
    public bool CanCast()
    {
        if (_entity == null || _world == null || _commandHandler == null || string.IsNullOrEmpty(AbilityId))
            return false;
        
        var command = new AbilityCommand(AbilityId, AbilityAction.Cast);
        var validation = _commandHandler.ValidateCommand(AbilityId, _entity.Value);
        if (!validation.IsValid)
            return false;
        
        var mana = _commandHandler.CheckMana(_entity.Value, 
            _registry?.Exists(AbilityId) == true ? _registry.Get(AbilityId).ManaCost : 0f);
        if (!mana.IsValid)
            return false;
        
        var cooldown = _commandHandler.CheckCooldown(_entity.Value, AbilityId);
        return cooldown.IsValid;
    }
    
    private void EmitManaSignal()
    {
        if (_entity?.Value.HasComponent<ManaComponent>() == true)
        {
            var mana = _entity.Value.GetComponent<ManaComponent>();
            EmitSignal(SignalName.ManaChanged, mana.CurrentMana, mana.MaxMana, this);
        }
    }
    
    private void EmitCooldownSignal()
    {
        if (_entity?.Value.HasComponent<AbilityCooldownComponent>() == true)
        {
            var cd = _entity.Value.GetComponent<AbilityCooldownComponent>();
            EmitSignal(SignalName.CooldownChanged, AbilityId, cd.RemainingCooldownSeconds, cd.BaseCooldownSeconds, this);
        }
    }
    
    private WorldManager? FindParentWorldManager()
    {
        var parent = GetParent();
        while (parent != null)
        {
            if (parent is WorldManager wm)
                return wm;
            parent = parent.GetParent();
        }
        return null;
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        // Auto-update cooldowns and mana if desired
        // (Alternatively, let a system handle this via WorldManager)
        if (_entity != null)
        {
            UpdateCooldowns((float)delta);
            // RegenerateMana((float)delta, 5f); // Uncomment for auto-regen
        }
    }
}