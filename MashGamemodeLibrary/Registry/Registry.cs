using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LabFusion.Extensions;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Registry;

public class Registry<TValue> where TValue : class
{
    private readonly Dictionary<ulong, TValue> _internalRegistry = new();

    public ulong GetID(MemberInfo type)
    {
        return type.Name.GetStableHash();
    }
    
    public ulong GetID<T>() where T : TValue
    {
        return GetID(typeof(T));
    }
    
    public void Register(ulong id, TValue value)
    {
        _internalRegistry[id] = value;
    }
    
    public void Register<T>() where T : TValue, new()
    {
        var id = GetID<T>();
        Register(id, new T());
    }
    
    public void RegisterAll<T>()
    {
        var assembly = typeof(T).Assembly;
        var registerTypeMethod = typeof(Registry<TValue>).GetMethod(nameof(Register)) ??
                                 throw new Exception("Failed to find RegisterTag method");
        assembly.GetTypes()
            .Where(t => typeof(TValue).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .ForEach(t => { registerTypeMethod.MakeGenericMethod(t).Invoke(this, null); });
    }

    public TValue? Get(ulong id)
    {
        return _internalRegistry.GetValueOrDefault(id);
    }

    public bool TryGet(ulong id, [MaybeNullWhen(false)] out TValue entry)
    {
        return _internalRegistry.TryGetValue(id, out entry);
    }

    public TValue? Get<T>() where T : TValue
    {
        var id = GetID<T>();
        return Get(id);
    }

    public bool TryGet<T>([MaybeNullWhen(false)] out TValue entry) where T : TValue
    {
        var id = GetID<T>();
        return TryGet(id, out entry);
    }

    public bool Contains(ulong id)
    {
        return _internalRegistry.ContainsKey(id);
    }
}