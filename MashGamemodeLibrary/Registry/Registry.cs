using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LabFusion.Extensions;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Registry;

public abstract class Registry<TValue> : IRegistry<TValue> where TValue : class
{
    public virtual ulong GetID(MemberInfo type)
    {
        return type.Name.GetStableHash();
    }
    
    public virtual ulong GetID<T>() where T : TValue
    {
        return GetID(typeof(T));
    }

    public virtual ulong GetID<T>(T instance) where T : TValue
    {
        return GetID(instance.GetType());
    }
    
    public abstract void Register<T>(ulong id, T value) where T : TValue, new();
    
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
        
        MelonLogger.Msg($"Registering type: {type.Name} to registry of: {typeof(TValue).Name}");
#endif
        
        var id = GetID<T>();
        Register(id, new T());  
    }
    
    public void RegisterAll<T>()
    {
        var assembly = typeof(T).Assembly;
        var registerTypeMethod = GetType().GetMethod("Register", Type.EmptyTypes);

        if (registerTypeMethod == null)
            throw new Exception("Could not find register method.");
        
        assembly.GetTypes()
            .Where(t => typeof(TValue).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .ForEach(t => { registerTypeMethod.MakeGenericMethod(t).Invoke(this, null); });
    }

    public abstract TValue? Get(ulong id);

    public abstract bool TryGet(ulong id, [MaybeNullWhen(false)] out TValue entry);

    public virtual TValue? Get<T>() where T : TValue
    {
        var id = GetID<T>();
        return Get(id);
    }

    public virtual bool TryGet<T>([MaybeNullWhen(false)] out TValue entry) where T : TValue
    {
        var id = GetID<T>();
        return TryGet(id, out entry);
    }

    public abstract bool Contains(ulong id);
}