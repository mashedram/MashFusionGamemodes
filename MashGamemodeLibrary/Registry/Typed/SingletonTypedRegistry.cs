using System.Diagnostics.CodeAnalysis;

namespace MashGamemodeLibrary.Registry.Typed;

public class SingletonTypedRegistry<TValue> : TypedRegistry<TValue, TValue> where TValue : notnull
{
    public void Register<T>(T value) where T : TValue
    {
        base.Register<TValue>(value);
    }
    
    protected override TValue Create<T>()
    {
        return new T();
    }
    
    protected override bool TryToValue(TValue? from, [MaybeNullWhen(false)] out TValue value)
    {
        if (from is null)
        {
            value = default!;
            return false;
        }

        value = from;
        return true;
    }
}