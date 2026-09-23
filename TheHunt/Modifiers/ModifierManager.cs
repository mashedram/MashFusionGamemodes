using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;

namespace TheHunt.Modifiers;

[RequireStaticConstructor]
public static class ModifierManager
{
    private static readonly SingletonTypedRegistry<IModifier> ModifierRegistry = new();
    
    private static readonly SyncedSet<IModifier> ActiveModifiers = new(
        "sync.modifiers",
        new DynamicInstanceEncoder<IModifier>(ModifierRegistry)
    );

    static ModifierManager()
    {
        ModifierRegistry.RegisterAll<Gamemode.TheHunt>();
        
        ActiveModifiers.OnValueAdded += OnModifierAdded;
        ActiveModifiers.OnValueRemoved += OnModifierRemoved;
    }

    public static void Clear()
    {
        ActiveModifiers.Clear();
    }

    public static void Randomize(int count, IEnumerable<Type>? requiredModifiers = null, IList<Type>? excludedModifiers = null)
    {
        Clear();   
        
        if (count > ModifierRegistry.Count)
            count = ModifierRegistry.Count;
        
        if (requiredModifiers != null)
        {
            foreach (var modifierType in requiredModifiers)
            {
                if (!ModifierRegistry.TryGet(modifierType, out var modifier))
                    continue;

                ActiveModifiers.Add(modifier);
                count--;
            }
        }
        
        if (count <= 0)
            return;

        var modifiers = ModifierRegistry
            .GetAll()
            .Where(modifier => !ActiveModifiers.Contains(modifier))
            .Where(modifier => excludedModifiers == null || !excludedModifiers.Contains(modifier.GetType()))
            .Shuffle()
            .Take(count);

        foreach (var modifier in modifiers)
        {
            ActiveModifiers.Add(modifier);
        }
    }
    
    // Remote event handlers

    private static void OnModifierAdded(IModifier value)
    {
        value.Apply();
    }
    
    private static void OnModifierRemoved(IModifier oldValue)
    {
        oldValue.Remove();
    }
}