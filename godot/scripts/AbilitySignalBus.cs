using Godot;
using MoonBark.Abilities.Core;
using MoonBark.Abilities.ECS;
using Friflo.Engine.ECS;
using System;

namespace MoonBark.Abilities.Godot;

/// <summary>
/// Bridges Core ability events to Godot signals.
/// Self-cleanup via Godot RefCounted; no parent hierarchy lookup needed —
/// initialize explicitly via <see cref="Initialize"/> after construction.
/// </summary>
[GlobalClass]
public partial class AbilitySignalBus : RefCounted
{
    // ===== Godot Signals =====

    [Signal] public delegate void AbilityCastEventHandler(Entity entity, string abilityId, Node3D? casterNode);
    [Signal] public delegate void AbilityFailedEventHandler(Entity entity, string abilityId, string reason, Node3D? casterNode);
    [Signal] public delegate void ManaChangedEventHandler(Entity entity, float currentMana, float maxMana, Node3D? casterNode);
    [Signal] public delegate void CooldownStartedEventHandler(Entity entity, string abilityId, float duration, Node3D? casterNode);
    [Signal] public delegate void CooldownEndedEventHandler(Entity entity, string abilityId, Node3D? casterNode);
    [Signal] public delegate void AbilityLearnedEventHandler(Entity entity, string abilityId, Node3D? casterNode);
    [Signal] public delegate void AbilityForgottenEventHandler(Entity entity, string abilityId, Node3D? casterNode);

    // ===== Fields =====

    private EntityStore? _world;
    private AbilityRegistry? _registry;
    private AbilityCommandHandler? _commandHandler;

    // Mapping from entity index to Godot node (for signal emission)
    private readonly Godot.Collections.Dictionary<int, WeakRef<Node3D>> _entityToNode = new();

    // ===== Initialization =====

    public AbilitySignalBus() { }

    public AbilitySignalBus(EntityStore world, AbilityRegistry registry, AbilityCommandHandler commandHandler)
    {
        Initialize(world, registry, commandHandler);
    }

    /// <summary>
    /// Initialize the bus. Call after construction, before use.
    /// </summary>
    public void Initialize(EntityStore world, AbilityRegistry registry, AbilityCommandHandler commandHandler)
    {
        _world = world;
        _registry = registry;
        _commandHandler = commandHandler;
    }

    // ===== Entity-Node Mapping =====

    /// <summary>
    /// Register a Godot node as the visual representation of an entity.
    /// </summary>
    public void RegisterEntityNode(Entity entity, Node3D node)
    {
        _entityToNode[entity.Index] = new WeakRef<Node3D>(node);
    }

    /// <summary>
    /// Unregister an entity's node mapping.
    /// </summary>
    public void UnregisterEntityNode(Entity entity)
    {
        _entityToNode.Remove(entity.Index);
    }

    /// <summary>
    /// Try to get the Godot node associated with an entity.
    /// </summary>
    public Node3D? GetNodeForEntity(Entity entity)
    {
        if (_entityToNode.TryGetValue(entity.Index, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var node))
                return node;
            _entityToNode.Remove(entity.Index);
        }
        return null;
    }

    // ===== Command Execution =====

    /// <summary>
    /// Execute an ability and emit appropriate signals.
    /// </summary>
    public AbilityCommandResult ExecuteAbility(string abilityId, Entity entity)
    {
        if (_commandHandler == null || _world == null)
            return AbilityCommandResult.Failure("SignalBus not initialized");

        var command = new AbilityCommand(abilityId, AbilityAction.Cast);
        var result = _commandHandler.Handle(command, _world, entity);

        var node = GetNodeForEntity(entity);

        if (result.Succeeded)
        {
            EmitSignal(SignalName.AbilityCast, entity, abilityId, node);
            EmitManaSignal(entity);
            EmitCooldownSignal(entity, abilityId);
        }
        else
        {
            EmitSignal(SignalName.AbilityFailed, entity, abilityId, result.Summary ?? "Unknown error", node);
        }

        return result;
    }

    /// <summary>
    /// Execute an ability by ID on a Godot node.
    /// </summary>
    public AbilityCommandResult ExecuteAbility(string abilityId, GodotAbilityNode node)
    {
        if (node.Entity == null)
            return AbilityCommandResult.Failure("Node has no entity");

        return ExecuteAbility(abilityId, node.Entity.Value);
    }

    // ===== Signal Emission Helpers =====

    public void EmitManaSignal(Entity entity)
    {
        if (entity.HasComponent<ManaComponent>())
        {
            var mana = entity.GetComponent<ManaComponent>();
            var node = GetNodeForEntity(entity);
            EmitSignal(SignalName.ManaChanged, entity, mana.CurrentMana, mana.MaxMana, node);
        }
    }

    public void EmitCooldownSignal(Entity entity, string abilityId)
    {
        if (entity.HasComponent<AbilityCooldownComponent>())
        {
            var cd = entity.GetComponent<AbilityCooldownComponent>();
            var node = GetNodeForEntity(entity);

            if (cd.IsOnCooldown && cd.RemainingCooldownSeconds >= cd.BaseCooldownSeconds - 0.001f)
            {
                EmitSignal(SignalName.CooldownStarted, entity, abilityId, cd.BaseCooldownSeconds, node);
            }
            else if (!cd.IsOnCooldown)
            {
                EmitSignal(SignalName.CooldownEnded, entity, abilityId, node);
            }
        }
    }

    public void EmitAbilityLearned(Entity entity, string abilityId)
    {
        var node = GetNodeForEntity(entity);
        EmitSignal(SignalName.AbilityLearned, entity, abilityId, node);
    }

    // ===== Utility Methods =====

    /// <summary>
    /// Connect a Godot node's method to an ability cast signal.
    /// Example: bus.ConnectAbilityCast(node, nameof(Node3D.OnAbilityCast));
    /// </summary>
    public void ConnectAbilityCast(Node target, string methodName, uint flags = 0)
    {
        target.Connect(SignalName.AbilityCast, new Callable(target, methodName), (uint)flags);
    }

    /// <summary>
    /// Connect a Godot node's method to an ability failed signal.
    /// </summary>
    public void ConnectAbilityFailed(Node target, string methodName, uint flags = 0)
    {
        target.Connect(SignalName.AbilityFailed, new Callable(target, methodName), (uint)flags);
    }
}
