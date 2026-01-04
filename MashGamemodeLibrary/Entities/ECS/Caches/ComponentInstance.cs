using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Execution;

namespace MashGamemodeLibrary.Entities.ECS.Caches;

internal record ComponentTarget(NetworkEntity NetworkEntity, MarrowEntity MarrowEntity);

public class ComponentInstance : IBehaviourHolder
{
    public readonly EcsIndex Index;
    public readonly Type ComponentType;
    public readonly IComponent Component;
    
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
            
            // Invoke callbacks
            foreach (var readyCallback in _readyCallbacks)
            {
                readyCallback.Try(callback => callback.Invoke(_componentTarget.NetworkEntity, _componentTarget.MarrowEntity));
            }
            _readyCallbacks.Clear();
        });
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
}