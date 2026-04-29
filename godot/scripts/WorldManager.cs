using Godot;
using MoonBark.Abilities.Core;
using MoonBark.Abilities.ECS;
using Friflo.Engine.ECS;
using System.Collections.Generic;

namespace MoonBark.Abilities.Godot;

/// <summary>
/// Central manager for the ECS world and ability systems.
/// Attach this to a root node in your scene to bootstrap the ability system.
/// This is the main composition root for Godot integration.
/// </summary>
[GlobalClass]
public partial class WorldManager : Node
{
    [Export] public bool AutoUpdateSystems { get; set; } = true;
    [Export] public float ManaRegenRate { get; set; } = 5f;
    
    private EntityStore? _world;
    private AbilityRegistry? _registry;
    private AbilityCommandHandler? _commandHandler;
    private AbilityCooldownSystem? _cooldownSystem;
    private ManaRegenerationSystem? _manaRegenSystem;
    private AbilitiesModule? _module;
    
    /// <summary>The ECS world store.</summary>
    public EntityStore? World => _world;
    
    /// <summary>The ability registry.</summary>
    public AbilityRegistry? Registry => _registry;
    
    /// <summary>The command handler.</summary>
    public AbilityCommandHandler? CommandHandler => _commandHandler;
    
    /// <summary>All GodotAbilityNode instances managed by this world.</summary>
    private readonly List<GodotAbilityNode> _abilityNodes = new();
    
    public WorldManager() { }
    
    public WorldManager(bool autoUpdateSystems, float manaRegenRate)
    {
        AutoUpdateSystems = autoUpdateSystems;
        ManaRegenRate = manaRegenRate;
    }
    
    public override void _Ready()
    {
        base._Ready();
        InitializeWorld();
    }
    
    /// <summary>
    /// Initialize the ECS world, registry, and systems.
    /// Can be called manually or happens automatically in _Ready().
    /// </summary>
    public void InitializeWorld()
    {
        _world = new EntityStore();
        _registry = new AbilityRegistry();
        _commandHandler = new AbilityCommandHandler(_registry);
        _cooldownSystem = new AbilityCooldownSystem();
        _manaRegenSystem = new ManaRegenerationSystem();
        _module = new AbilitiesModule(_registry, _cooldownSystem, _manaRegenSystem);
        
        GD.Print($"WorldManager: ECS world initialized");
        
        // Register any pre-configured abilities from resources
        RegisterAbilitiesFromResources();
        
        // Find and initialize all GodotAbilityNode children
        InitializeAbilityNodes(this);
    }
    
    /// <summary>
    /// Register ability definitions from a JSON resource file.
    /// </summary>
    public void RegisterAbilitiesFromResource(string resourcePath)
    {
        var resource = GD.Load(resourcePath) as Resource;
        if (resource == null)
        {
            GD.PushError($"WorldManager: Could not load resource at {resourcePath}");
            return;
        }
        
        // Parse resource and register abilities
        // Implementation depends on resource format
    }
    
    private void RegisterAbilitiesFromResources()
    {
        // Auto-discover ability definition resources
        // Can be extended by game code
    }
    
    /// <summary>
    /// Register a new ability definition.
    /// </summary>
    public void RegisterAbility(AbilityDefinition definition)
    {
        if (_registry == null)
        {
            GD.PushError("WorldManager: Registry not initialized");
            return;
        }
        _registry.Register(definition);
        GD.Print($"WorldManager: Registered ability '{definition.Id}'");
    }
    
    /// <summary>
    /// Register multiple ability definitions.
    /// </summary>
    public void RegisterAbilities(IEnumerable<AbilityDefinition> definitions)
    {
        foreach (var def in definitions)
            RegisterAbility(def);
    }
    
    /// <summary>
    /// Create a new ability entity and attach it as a child node.
    /// </summary>
    public GodotAbilityNode CreateAbilityEntity(string abilityId, Vector3 position = default)
    {
        if (_world == null || _registry == null || _commandHandler == null)
        {
            GD.PushError("WorldManager: World not initialized");
            return null!;
        }
        
        var node = new GodotAbilityNode(_world, _registry, abilityId)
        {
            Position = position
        };
        
        AddChild(node);
        _abilityNodes.Add(node);
        
        return node;
    }
    
    /// <summary>
    /// Create an ability entity from a scene.
    /// </summary>
    public GodotAbilityNode CreateAbilityEntityFromScene(PackedScene scene, string abilityId, Vector3 position = default)
    {
        var instance = scene.Instantiate<GodotAbilityNode>();
        if (instance == null)
        {
            GD.PushError($"WorldManager: Scene does not contain GodotAbilityNode");
            return null!;
        }
        
        instance.Position = position;
        instance.Initialize(_world!, _registry!, _commandHandler);
        
        AddChild(instance);
        _abilityNodes.Add(instance);
        
        return instance;
    }
    
    /// <summary>
    /// Find all GodotAbilityNode children and initialize them.
    /// </summary>
    public void InitializeAbilityNodes(Node root)
    {
        var nodes = root.GetChildren();
        foreach (var child in nodes)
        {
            if (child is GodotAbilityNode abilityNode)
            {
                if (_world != null && _registry != null && _commandHandler != null)
                {
                    abilityNode.Initialize(_world, _registry, _commandHandler);
                }
                _abilityNodes.Add(abilityNode);
            }
            
            // Recursively check children
            if (child.GetChildCount() > 0)
            {
                InitializeAbilityNodes(child);
            }
        }
    }
    
    /// <summary>
    /// Execute an ability command by ID on a specific entity.
    /// </summary>
    public AbilityCommandResult ExecuteAbility(string abilityId, Entity entity)
    {
        if (_commandHandler == null || _world == null)
            return AbilityCommandResult.Failure("World not initialized");
        
        var command = new AbilityCommand(abilityId, AbilityAction.Cast);
        return _commandHandler.Handle(command, _world, entity);
    }
    
    /// <summary>
    /// Execute an ability by ID on a GodotAbilityNode.
    /// </summary>
    public bool ExecuteAbility(string abilityId, GodotAbilityNode node)
    {
        if (node.Entity == null)
            return false;
        
        var result = ExecuteAbility(abilityId, node.Entity.Value);
        return result.Succeeded;
    }
    
    /// <summary>
    /// Get the ECS entity for a Godot node.
    /// </summary>
    public Entity? GetEntityForNode(Node3D node)
    {
        if (node is GodotAbilityNode abilityNode)
            return abilityNode.Entity;
        
        // Check if node has an entity component attached via metadata
        if (node.HasMeta("ecs_entity_index"))
        {
            int index = (int)node.GetMeta("ecs_entity_index");
            return _world?.GetEntity(index);
        }
        
        return null;
    }
    
    /// <summary>
    /// Update all ECS systems.
    /// </summary>
    public void UpdateSystems(float deltaTime)
    {
        _cooldownSystem?.Update(_world!, deltaTime);
        _manaRegenSystem?.Update(_world!, deltaTime);
        
        // Update any custom systems here
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (AutoUpdateSystems && _world != null)
        {
            UpdateSystems((float)delta);
        }
    }
    
    /// <summary>
    /// Get a query for entities with specific components.
    /// </summary>
    public Query<Inc1<AbilityComponent>> QueryAbilities()
    {
        return _world?.Query<Inc1<AbilityComponent>>() ?? default;
    }
    
    /// <summary>
    /// Get a query for entities with mana.
    /// </summary>
    public Query<Inc1<ManaComponent>> QueryManaEntities()
    {
        return _world?.Query<Inc1<ManaComponent>>() ?? default;
    }
    
    /// <summary>
    /// Get a query for entities on cooldown.
    /// </summary>
    public Query<Inc1<AbilityCooldownComponent>> QueryCooldownEntities()
    {
        return _world?.Query<Inc1<AbilityCooldownComponent>>() ?? default;
    }
    
    /// <summary>
    /// Debug: Print all entities and their components.
    /// </summary>
    public void DebugPrintEntities()
    {
        if (_world == null)
            return;
        
        GD.Print($"=== WorldManager: {_world.Stats().EntityCount} entities ===");
        foreach (var entity in _world.Query())
        {
            var components = new List<string>();
            if (entity.HasComponent<AbilityComponent>()) components.Add("Ability");
            if (entity.HasComponent<ManaComponent>()) components.Add("Mana");
            if (entity.HasComponent<AbilityCooldownComponent>()) components.Add("Cooldown");
            if (entity.HasComponent<AbilityBookComponent>()) components.Add("AbilityBook");
            if (entity.HasComponent<AbilityLearningComponent>()) components.Add("Learning");
            if (entity.HasComponent<CanCastAbilitiesTag>()) components.Add("CanCast");
            if (entity.HasComponent<OnCooldownTag>()) components.Add("OnCooldown");
            
            GD.Print($"  Entity {entity.Index}: [{string.Join(", ", components)}]");
        }
    }
}