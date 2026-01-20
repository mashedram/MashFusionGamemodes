using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Behaviour.Helpers;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Execution;

namespace MashGamemodeLibrary.Entities.ECS.Caches;

internal record ComponentTarget(NetworkEntity NetworkEntity, MarrowEntity MarrowEntity);

public class ComponentInstance : IBehaviourHolder
{
    public readonly EcsIndex Index;
    public readonly Type ComponentType;
    public readonly IComponent Component;

    public Guid Guid { get; } = Guid.NewGuid();
    
    public readonly bool IsNetworked;
    public readonly bool PlayerOnly;

    public ushort EntityId => Index.EntityID.ID;

    private ComponentTarget? _componentTarget;
    public bool IsReady => _componentTarget != null;
    
    public NetworkEntity NetworkEntity => 
        _componentTarget?.NetworkEntity ??
        throw new InvalidOperationException("ComponentInstance is not ready");
    public MarrowEntity MarrowEntity => 
        _componentTarget?.MarrowEntity ??
        throw new InvalidOperationException("ComponentInstance is not ready");

    private List<Action<NetworkEntity, MarrowEntity>> _readyCallbacks = new();

    private CacheKey? _cacheKey;
    private List<BehaviourMember>? _behaviourMembers = null;
    
    public ComponentInstance(EcsIndex index, IComponent component)
    {
        Index = index;
        ComponentType = component.GetType();
        Component = component;
        
        IsNetworked = ComponentType.GetCustomAttribute<LocalOnly>() == null;
        PlayerOnly = ComponentType.GetInterfaces().Any(i => i.IsAssignableTo(typeof(IPlayerBehaviour)));
        
        Index.EntityID.WaitOnMarrowEntity((entity, marrowEntity) =>
        {
            _componentTarget = new ComponentTarget(entity, marrowEntity);
            
            // Unregister hooks
            entity.OnEntityUnregistered += OnUnregistered;
            _cacheKey = CachedQueryManager.Add(Component);
            _behaviourMembers = BehaviourManager.Add(this, component);
            
            // Invoke callbacks
            foreach (var readyCallback in _readyCallbacks)
            {
                readyCallback.Try(callback => callback.Invoke(_componentTarget.NetworkEntity, _componentTarget.MarrowEntity));
            }
            _readyCallbacks.Clear();
        });
    }

    ~ComponentInstance()
    {
        Remove();
    }

    public void HookOnReady(Action<NetworkEntity, MarrowEntity> callback)
    {
        if (_componentTarget != null)
        {
            callback(_componentTarget.NetworkEntity, _componentTarget.MarrowEntity);
            return;
        }
        
        _readyCallbacks.Add(callback);
    }

    public T GetAs<T>()
    {
        return (T)Component;
    }

    public bool TryGetAs<T>([MaybeNullWhen(false)] out T component)
    {
        if (Component is T typedComponent)
        {
            component = typedComponent;
            return true;
        }

        component = default;
        return false;
    }

    private void OnUnregistered(NetworkEntity networkEntity)
    {
        if (_behaviourMembers != null) 
            BehaviourManager.RemoveAll(_behaviourMembers);
        
        _cacheKey?.Remove();
        LocalEcsCache.Remove(Index);
        
        if (_componentTarget == null)
            return;
        
        NetworkEntity.OnEntityUnregistered -= OnUnregistered;
        _componentTarget = null;
    }

    public void Remove()
    {
        if (_componentTarget == null)
            return;
        
        OnUnregistered(NetworkEntity);
    }
}