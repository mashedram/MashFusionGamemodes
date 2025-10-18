using System.Reflection;

namespace MashGamemodeLibrary.Registry;

public interface IKeyable<in TKey>
    where TKey : notnull
{
    public ulong GetID<T>() where T : TKey;
    public ulong GetID<T>(T instance) where T : TKey;
}