using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Behaviour.Helpers;

internal class InheritanceCache
{
    private Dictionary<Type, HashSet<Type>> _dictionary = new();

    public void AddComponent(IBehaviour behaviour)
    {
        var t = behaviour.GetType();

        if (_dictionary.ContainsKey(t))
            return;

        var baseComponents = t.GetInterfaces();
        foreach (var baseComponent in baseComponents)
        {
            if (!baseComponent.IsAssignableTo(typeof(IBehaviour)))
                continue;

            _dictionary
                .GetValueOrCreate(t, () => new HashSet<Type>())
                .Add(baseComponent);

        }
    }

    public IEnumerable<Type> GetBaseTypes(IBehaviour behaviour)
    {
        return _dictionary.TryGetValue(behaviour.GetType(), out var set) ? set : Array.Empty<Type>();
    }
}