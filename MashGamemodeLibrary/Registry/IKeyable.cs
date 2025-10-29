using System.Reflection;

namespace MashGamemodeLibrary.Registry;

public interface IKeyable<in TKey>
{
    public ulong CreateID<T>() where T : notnull, TKey;
    public ulong CreateID(TKey instance);
}