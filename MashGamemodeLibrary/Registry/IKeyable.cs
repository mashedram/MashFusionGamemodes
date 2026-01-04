namespace MashGamemodeLibrary.Registry;

public interface IKeyable<in TKey>
{
    public ulong CreateID(Type type);
    public ulong CreateID<T>() where T : notnull, TKey;
    public ulong CreateID(TKey instance);
    
    // Optional overload if lookups exist
    public ulong GetID<T>() where T : notnull, TKey
    {
        return CreateID<T>();
    }
    
    // Optional overload if lookups exist
    public ulong GetID(Type type)  
    {
        return CreateID(type);
    }

    // Optional overload if lookups exist
    public ulong GetID(TKey instance)
    {
        return CreateID(instance);
    }
}