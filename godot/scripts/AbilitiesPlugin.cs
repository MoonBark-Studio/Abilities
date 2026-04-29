using Godot;
using MoonBark.Abilities.Core;
using MoonBark.Abilities.Godot;
using MoonBark.Abilities.ECS;
using Friflo.Engine.ECS;

namespace MoonBark.Abilities.Godot;

/// <summary>
/// Editor plugin for the Abilities system.
/// Manages the global ability registry and world state.
/// </summary>
/// <remarks>
/// This is a thin Godot layer. All logic lives in Core/ECS.
/// This plugin provides:
/// - Global singleton access to WorldManager
/// - Editor UI for ability definitions
/// - Scene integration
/// </remarks>
[Tool]
public partial class AbilitiesPlugin : EditorPlugin
{
    private WorldManager? _worldManager;
    private Control? _dock;
    private Button? _refreshButton;
    private ItemList? _abilityList;
    
    public WorldManager? WorldManager => _worldManager;
    public AbilityRegistry? Registry => _worldManager?.Registry;
    
    public override void _EnterTree()
    {
        GD.Print("[AbilitiesPlugin] Entering tree");
        
        // Create or find WorldManager
        var root = GetTree().Root;
        _worldManager = FindWorldManager(root);
        
        if (_worldManager == null)
        {
            GD.Print("[AbilitiesPlugin] Creating WorldManager");
            _worldManager = new WorldManager(autoUpdateSystems: true, manaRegenRate: 5f);
            root.AddChild(_worldManager);
            _worldManager.Owner = root;
        }
        
        _worldManager.InitializeWorld();
        
        // Create editor dock
        CreateDock();
        
        GD.Print("[AbilitiesPlugin] Ready");
    }
    
    public override void _ExitTree()
    {
        GD.Print("[AbilitiesPlugin] Exiting tree");

        if (_refreshButton != null)
        {
            _refreshButton.Pressed -= OnRefreshPressed;
        }

        if (_dock != null)
        {
            RemoveControlFromBottomPanel(_dock);
            _dock.QueueFree();
            _dock = null;
        }
    }
    
    private WorldManager? FindWorldManager(Node root)
    {
        var children = root.GetChildren();
        foreach (Node child in children)
        {
            if (child is WorldManager wm)
                return wm;
            
            if (child.GetChildCount() > 0)
            {
                var found = FindWorldManager(child);
                if (found != null)
                    return found;
            }
        }
        return null;
    }
    
    private void CreateDock()
    {
        _dock = new Control
        {
            Name = "Abilities"
        };
        
        var vbox = new VBoxContainer
        {
            AnchorLeft = 0f,
            AnchorTop = 0f,
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        _dock.AddChild(vbox);
        
        // Title
        var title = new Label
        {
            Text = "Abilities",
            ThemeTypeVariation = "Header"
        };
        vbox.AddChild(title);
        
        // Separator
        vbox.AddChild(new HSeparator());
        
        // Refresh button
        _refreshButton = new Button
        {
            Text = "Refresh Abilities",
            SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill
        };
        _refreshButton.Pressed += OnRefreshPressed;
        vbox.AddChild(_refreshButton);
        
        // Ability list
        _abilityList = new ItemList
        {
            SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill,
            SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill
        };
        vbox.AddChild(_abilityList);
        
        // Add to bottom panel
        AddControlToBottomPanel(_dock, "Abilities");
        
        RefreshAbilityList();
    }
    
    private void OnRefreshPressed()
    {
        if (_worldManager == null)
            return;
        
        _worldManager.InitializeWorld();
        RefreshAbilityList();
        
        GD.Print("[AbilitiesPlugin] Refreshed");
    }
    
    private void RefreshAbilityList()
    {
        _abilityList?.Clear();
        
        if (Registry == null)
            return;
        
        foreach (var id in Registry.GetAllAbilityIds())
        {
            var def = Registry.Get(id);
            _abilityList!.AddItem($"{def.Name} ({def.Id})");
            
            var tooltip = $"ID: {def.Id}\n"
                        + $"Kind: {def.Kind}\n"
                        + $"Mana: {def.ManaCost}\n"
                        + $"Cooldown: {def.BaseCooldownSeconds}s\n"
                        + $"Range: {(def.Range > 0 ? def.Range.ToString() + " units" : "Unlimited")}\n"
                        + $"Effects: {def.Effects.Count}";
            
            _abilityList!.SetItemTooltip(_abilityList.GetItemCount() - 1, tooltip);
        }
    }
    
    /// <summary>
    /// Register an ability definition from the editor.
    /// </summary>
    public void RegisterAbilityFromEditor(AbilityDefinition definition)
    {
        if (_worldManager == null)
        {
            GD.PushError("[AbilitiesPlugin] WorldManager not available");
            return;
        }
        
        _worldManager.RegisterAbility(definition);
        RefreshAbilityList();
    }
    
    /// <summary>
    /// Create an ability entity from the editor.
    /// </summary>
    public GodotAbilityNode? CreateAbilityEntityFromEditor(string abilityId, Vector3 position)
    {
        if (_worldManager == null)
            return null;
        
        return _worldManager.CreateAbilityEntity(abilityId, position);
    }
}