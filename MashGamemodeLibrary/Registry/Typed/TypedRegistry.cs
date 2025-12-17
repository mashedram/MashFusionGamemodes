using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using LabFusion.Extensions;
using MashGamemodeLibrary.Registry.Keyed;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Registry.Typed;

public abstract class TypedRegistry<TInternal, TValue> : KeyedRegistry<ulong, TInternal>, ITypedRegistry<TValue>
    where TValue : notnull
    where TInternal : notnull
{
    private readonly Dictionary<Type, ulong> _stableHashCache = new();
    private readonly Dictionary<ulong, Type> _typeCache = new();
    
    public ulong GetID(Type type)
    {
        return _stableHashCache[type];
    }
    
    public ulong GetID<T>()
    {
        return GetID(typeof(T));
    }
    
    public ulong GetID(TValue instance)
    {
        return GetID(instance.GetType());
    }
    
    public ulong CreateID(Type type)
    {
        var fullName = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
        return fullName.GetStableHash();
    }
    
    public ulong CreateID<T>() where T : TValue
    {
        return CreateID(typeof(T));
    }
    
    public ulong CreateID(TValue instance)
    {
        return CreateID(instance.GetType());
    }

    public ulong GetOrCreateId(Type type)
    {
        return _stableHashCache.GetOrCreate(type, () => CreateID(type));
    }
    
    public ulong GetOrCreateId<T>() where T : TValue
    {
        return GetOrCreateId(typeof(T));
    }
    
    protected abstract TInternal Create<T>() where T : TValue, new();
    protected abstract bool TryToValue(TInternal? from, [MaybeNullWhen(false)] out TValue value);

    public virtual void Register<T>() where T : TValue, new()
    {
        Register<T>(Create<T>());
    }
    
    internal void Register<T>(TInternal value) where T : TValue
    {
#if DEBUG
        // This function is connected to the registerall and needs better runtime logging
        var type = typeof(T);
        if (type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes) == null)
        {
            MelonLogger.Error($"Type: {type.Name} has no default constructor. Ensure it satisfiers the \"new()\" clause.");
            return;
        }
#endif

        var id = CreateID<T>();
        _stableHashCache[type] = id;
        _typeCache[id] = type;
#if DEBUG
        MelonLogger.Msg($"Registering type: {type.Name} with id: {id} to registry of: {typeof(TValue).Name}");
#endif
        Register(id, value); 
    }
    
    public void RegisterAll<T>()
    {
        var assembly = typeof(T).Assembly;
        var registerTypeMethod = GetType().GetMethod("Register", Type.EmptyTypes);

        if (registerTypeMethod == null)
            throw new Exception("Could not find register method.");
        
        assembly.GetTypes()
            .Where(t => typeof(TValue).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false, IsInterface: false })
            .ForEach(t => { registerTypeMethod.MakeGenericMethod(t).Invoke(this, null); });
    }

    public Type? GetType(ulong id)
    {
        return _typeCache.GetValueOrDefault(id);
    }

    public bool TryGetType(ulong id, [MaybeNullWhen(false)] out Type type)
    {
        return _typeCache.TryGetValue(id, out type);
    }

    public new TValue? Get(ulong id)
    {
        TryToValue(base.Get(id), out var value);
        return value;
    }

    public virtual T? Get<T>() where T : TValue
    {
        var id = GetID<T>();
        if (Get(id) is not T tag)
            return default!;

        return tag;
    }
    
    public bool TryGet(ulong key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!base.TryGet(key, out var maybeEntry) || !TryToValue(maybeEntry, out var internalValue))
        {
            value = default!;
            return false;
        }

        value = internalValue;
        return true;
    }

    public virtual bool TryGet<T>([MaybeNullWhen(false)] out T entry) where T : TValue
    {
        var id = GetID<T>();
        if (!TryGet(id, out var value))
        {
            entry = default!;
            return false;
        }

        entry = (T)value;
        return true;
    }
}

public abstract class TypedRegistry<TValue> : TypedRegistry<TValue, TValue> where TValue : notnull {}