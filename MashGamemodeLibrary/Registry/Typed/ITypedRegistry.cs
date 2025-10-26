using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MashGamemodeLibrary.Registry.Keyed;

namespace MashGamemodeLibrary.Registry.Typed;

public interface ITypedRegistry<TValue> : IKeyable<TValue> where TValue : notnull
{
    public ulong GetID(Type type);
    public void Register<T>() where T : TValue, new();
    public void RegisterAll<T>();
    public Type? GetType(ulong id);
    public bool TryGetType(ulong id, [MaybeNullWhen(false)] out Type type);
    public TValue? Get(ulong id);
    public T? Get<T>() where T : TValue;
    public bool TryGet(ulong id, [MaybeNullWhen(false)] out TValue entry);
    public bool TryGet<T>([MaybeNullWhen(false)] out T entry) where T : TValue;
}