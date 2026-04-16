namespace MashGamemodeLibrary.Util;

public static class StableHash
{
    public static ulong Fnv1A64(string input)
    {
        const ulong fnvOffset = 14695981039346656037;
        const ulong fnvPrime = 1099511628211;
        var hash = fnvOffset;
        foreach (var c in input)
        {
            hash ^= c;
            hash *= fnvPrime;
        }

        return hash;
    }

    public static ulong GetStableHash(this string input)
    {
        return Fnv1A64(input);
    }

    public static ulong GetStableHash(this Type type)
    {
        return Fnv1A64(type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
    }

    public static ulong GetStableHash<T>(this T instance) where T : Enum
    {
        return Fnv1A64($"{typeof(T).Name}-{instance}");
    }
}