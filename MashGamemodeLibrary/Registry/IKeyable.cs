using System.Reflection;

namespace MashGamemodeLibrary.Registry;

public interface IKeyable<in TKey>
{
    public ulong GetID<T>() where T : notnull, TKey;
    public ulong GetID(TKey instance);
}