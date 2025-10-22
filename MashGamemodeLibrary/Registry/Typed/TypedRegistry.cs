using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LabFusion.Extensions;
using MashGamemodeLibrary.Registry.Keyed;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Registry.Typed;

public abstract class TypedRegistry<TInternal, TValue> : KeyedRegistry<ulong, TInternal>, ITypedRegistry<TValue> 
    where TValue : notnull
    where TInternal : notnull
{
    public ulong GetID(MemberInfo type)
    {
        return type.Name.GetStableHash();
    }
    
    public ulong GetID<T>() where T : TValue
    {
        return GetID(typeof(T));
    }
    
    public ulong GetID(TValue instance)
    {
        return GetID(instance.GetType());
    }
    
    protected abstract TInternal Create<T>() where T : TValue, new();
    protected abstract bool TryToValue(TInternal? from, [MaybeNullWhen(false)] out TValue value);
    
    public virtual void Register<T>() where T : TValue, new()
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
        
        var id = GetID<T>();
#if DEBUG
        MelonLogger.Msg($"Registering type: {type.Name} with id: {id} to registry of: {typeof(TValue).Name}");
#endif
        Register(id, Create<T>());  
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
    
    public new TValue? Get(ulong id)
    {
        TryToValue(base.Get(id), out var value);
        return value;
    }

    public virtual TValue? Get<T>() where T : TValue
    {
        var id = GetID<T>();
        return Get(id);
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