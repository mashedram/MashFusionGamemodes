using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MashGamemodeLibrary.Entities.Association;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.Behaviour.Helpers;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Entities.ECS.Instance;

public class EcsInstance : IBehaviourHolder
{
    public EcsIndex Index { get; }
    public readonly Type ComponentType;
    public bool IsReady { get; private set; }
    public IComponent Component { get; }

    public readonly bool IsNetworked;
    
    // Configuration
    private CacheKey? _cacheKey;
    private List<BehaviourMember>? _behaviourMembers;
    
    public EcsInstance(EcsIndex index, IComponent component)
    {
        Index = index;
        ComponentType = component.GetType();
        Component = component;
        IsNetworked = ComponentType.GetCustomAttribute<LocalOnly>() == null;
        
        Index.HookCreation(OnReady);
    }

    private void OnReady()
    {
        IsReady = true;
        _cacheKey = CachedQueryManager.Add(Component);
        _behaviourMembers = BehaviourManager.Add(this, Component);
        
        Index.HookRemoval(OnRemoval);

        InternalLogger.Debug("Registered component: " + ComponentType.FullName);
    }
    
    private void OnRemoval()
    {
        // Prevent removing instances we haven't added
        if (!IsReady)
            return;
        
        try
        {
            _cacheKey?.Remove();
            if (_behaviourMembers != null)
                BehaviourManager.RemoveAll(_behaviourMembers);

            // TODO
            // LocalEcsCache.Remove(IndexDepricated);

            IsReady = false;
        }
        catch (Exception e)
        {
            MelonLogger.Error("Error while unregistering component: " + e);
        }
    }
    
    public void Remove()
    {
        OnRemoval();
    }
    
    // Accessors
    
    public TBehaviour? TryCast<TBehaviour>() where TBehaviour : class, IBehaviour
    {
        return Component as TBehaviour;
    }
    
    public bool TryCast<TBehaviour>([MaybeNullWhen(false)] out TBehaviour component) where TBehaviour : class, IBehaviour
    {
        component = TryCast<TBehaviour>();
        return component != null;
    }
}