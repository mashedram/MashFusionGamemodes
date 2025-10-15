using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LabFusion.Extensions;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Registry;

public class FactoryRegistry<TValue> : Registry<TValue> where TValue : class
{
    private readonly Dictionary<ulong, Func<TValue>> _internalRegistry = new();

    public override void Register<T>(ulong id, T value)
    {
        // Due to the nature of this registry type, we ignore value on purpose
        _internalRegistry[id] = () => new T();
    }
    
    public override TValue? Get(ulong id)
    {
        return _internalRegistry.GetValueOrDefault(id)?.Invoke();
    }

    public override bool TryGet(ulong id, [MaybeNullWhen(false)] out TValue entry)
    {
        if (!_internalRegistry.TryGetValue(id, out var constructor))
        {
            entry = null;
            return false;
        }

        entry = constructor.Invoke();
        return true;
    }

    public override bool Contains(ulong id)
    {
        return _internalRegistry.ContainsKey(id);
    }
}