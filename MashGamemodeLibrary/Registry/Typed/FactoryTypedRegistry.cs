using System.Diagnostics.CodeAnalysis;

namespace MashGamemodeLibrary.Registry.Typed;

public class FactoryTypedRegistry<TValue> : TypedRegistry<Func<TValue>, TValue> where TValue : class
{
    protected override Func<TValue> Create<T>()
    {
        return () => new T();
    }
    
    protected override bool TryToValue(Func<TValue>? from, [MaybeNullWhen(false)] out TValue value)
    {
        value = from?.Invoke();
        return value != null;
    }
}