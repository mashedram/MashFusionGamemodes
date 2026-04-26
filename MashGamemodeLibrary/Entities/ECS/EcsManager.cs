using LabFusion.Network;
using MashGamemodeLibrary.Entities.Association;
using MashGamemodeLibrary.Entities.Association.Impl;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.ECS.Instance;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.ECS;

[RequireStaticConstructor]
public class EcsManager
{
    private static readonly Dictionary<EcsIndex, EcsInstance> LocalComponents = new();
    private static readonly Dictionary<Type, Dictionary<int, HashSet<EcsInstance>>> ComponentLookup = new();
    private static readonly Dictionary<Type, HashSet<EcsIndex>> EcsIndexLookup = new();
    
    internal static readonly FactoryTypedRegistry<IComponent> ComponentRegistry = new();

    private static readonly SyncedDictionary<EcsIndex, IComponent> NetworkComponents =
        new(
            "sync.ECS",
            new InstanceEncoder<EcsIndex>(),
            new DynamicInstanceEncoder<IComponent>(ComponentRegistry)
        );
    
    static EcsManager()
    {
        NetworkComponents.OnValueAdded += OnNetworkComponentAdded;
        NetworkComponents.OnValueRemoved += OnNetworkComponentRemoved;
    }
    
    // Networking
    
    private static void OnNetworkComponentAdded(EcsIndex key, IComponent component)
    {
        if (!NetworkInfo.HasServer || NetworkInfo.IsHost)
            return;

        Add(new EcsInstance(key, component));
    }

    private static void OnNetworkComponentRemoved(EcsIndex key, IComponent oldValue)
    {
        if (!NetworkInfo.HasServer || NetworkInfo.IsHost)
            return;

        Remove(key);
    }

    // Accessors

    public static void Add(EcsInstance instance)
    {
        var index = instance.Index;
        // Checks

        // TODO : Check back up on this
        // if (instance.PlayerOnly && index.EntityID.ID > PlayerIDManager.MaxPlayerID)
        //     throw new Exception($"Failed to add tag meant for players to prop ({instance.Component.GetType().FullName})");

        // Logic
        if (!LocalComponents.TryAdd(index, instance))
            return;

        if (index.Association != null)
            ComponentLookup
                .GetValueOrCreate(index.Association.GetType())
                .GetValueOrCreate(index.Association.GetID())
                .Add(instance);
        
        EcsIndexLookup
            .GetValueOrCreate(instance.ComponentType)
            .Add(index);

        // Networking
        if (!instance.IsNetworked)
            return;
        
        Executor.RunIfHost(() =>
        {
            NetworkComponents[index] = instance.Component;
        });
    }

    public static void Remove(EcsIndex index)
    {
        if (!LocalComponents.Remove(index, out var instance))
            return;

        instance.Remove();

        EcsIndexLookup.GetValueOrDefault(instance.ComponentType)?.Remove(index);
        if (index.Association != null)
            ComponentLookup
                .GetValueOrDefault(index.Association.GetType())?
                .GetValueOrDefault(index.Association.GetID())?
                .Remove(instance);
        
        // Always remove on the network to ensure it's not there by accident, even if it's a local component
        Executor.RunIfHost(() =>
        {
            NetworkComponents.Remove(index);
        });
    }
    
    public static void Add(EcsIndex index, IComponent component)
    {
        Add(new EcsInstance(index, component));
    }
    
    public static TComponent? Get<TComponent>(EcsIndex ecsIndex) where TComponent : class, IComponent
    {
        if (!LocalComponents.TryGetValue(ecsIndex, out var instance))
            return null;

        return instance.Component as TComponent;
    }
    
    public static EcsInstance? GetInstance(EcsIndex ecsIndex)
    {
        return LocalComponents.GetValueOrDefault(ecsIndex);
    }
    
    public static void Clear(IEcsAssociation association)
    {
        if (!ComponentLookup.TryGetValue(association.GetType(), out var associationLookup) ||
            !associationLookup.TryGetValue(association.GetID(), out var instances))
            return;

        // Create a copy of the list to avoid modification during enumeration
        var instancesCopy = instances.ToList();
        foreach (var instance in instancesCopy)
        {
            Remove(instance.Index);
        }
    }
    
    
    
    public static IEnumerable<TAssociation> GetAllAssociated<TAssociation>(Type? component = null) where TAssociation : class, IEcsAssociation
    {
        if (!ComponentLookup.TryGetValue(typeof(TAssociation), out var associationLookup))
            yield break;

        foreach (var (_, instances) in associationLookup)
        {
            // If a component filter is provided, check if any instance matches the component type
            if (component != null && instances.All(i => i.ComponentType != component))
                continue;

            // Yield the association for the first instance (all instances share the same association)
            if (instances.FirstOrDefault() is { Index.Association: TAssociation association })
            {
                yield return association;
            }
        }
    }
    
    public static IEnumerable<EcsIndex> GetAllIndices<TComponent>() where TComponent : IComponent
    {
        var componentType = typeof(TComponent);
        if (!EcsIndexLookup.TryGetValue(componentType, out var indices))
            yield break;

        foreach (var index in indices)
        {
            yield return index;
        }
    }
    
    public static IEnumerable<TComponent> GetAll<TComponent>() where TComponent : class, IComponent
    {
        var componentType = typeof(TComponent);
        if (!EcsIndexLookup.TryGetValue(componentType, out var indices))
            yield break;

        foreach (var index in indices)
        {
            if (LocalComponents.TryGetValue(index, out var instance) && instance.Component is TComponent component)
            {
                yield return component;
            }
        }
    }
    
    // Global reset and setup
    public static void RegisterAll<T>()
    {
        ComponentRegistry.RegisterAll<T>();
    }
    
    public static void Reset()
    {
        var indices = LocalComponents.Keys.ToList();
        foreach (var index in indices)
        {
            Remove(index);
        }

        
        LocalComponents.Clear();
        ComponentLookup.Clear();
        EcsIndexLookup.Clear();
        Executor.RunIfHost(() =>
        {
            NetworkComponents.Clear();
        });
    }
}