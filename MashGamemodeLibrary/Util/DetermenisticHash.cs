namespace MashGamemodeLibrary.Util;

public static class DetermenisticHash
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

    public static ulong GetDeterministicHash(this string input)
    {
        return Fnv1A64(input);
    }

    public static ulong GetDeterministicHash(this Type type)
    {
        return Fnv1A64(type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
    }

    public static ulong GetDeterministicHash<T>(this T instance) where T : Enum
    {
        return Fnv1A64($"{typeof(T).Name}-{instance}");
    }
}