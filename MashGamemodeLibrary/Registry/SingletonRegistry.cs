using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LabFusion.Extensions;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Registry;

public class SingletonRegistry<TValue> : Registry<TValue> where TValue : class
{
    private readonly Dictionary<ulong, TValue> _internalRegistry = new();
    
    public override void Register<T>(ulong id, T value)
    {
        _internalRegistry[id] = value;
    }

    public override TValue? Get(ulong id)
    {
        return _internalRegistry.GetValueOrDefault(id);
    }

    public override bool TryGet(ulong id, [MaybeNullWhen(false)] out TValue entry)
    {
        return _internalRegistry.TryGetValue(id, out entry);
    }
    
    public override bool Contains(ulong id)
    {
        return _internalRegistry.ContainsKey(id);
    }
}