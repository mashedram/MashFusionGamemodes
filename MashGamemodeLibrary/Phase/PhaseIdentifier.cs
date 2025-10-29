using System.Diagnostics.CodeAnalysis;

namespace MashGamemodeLibrary.Phase;

public struct PhaseIdentifier
{
    private readonly ulong? _id;

    public bool IsEmpty()
    {
        return !_id.HasValue;
    }

    public bool TryGetValue([MaybeNullWhen(false)] out ulong value)
    {
        value = _id.GetValueOrDefault();
        return _id.HasValue;
    }

    public PhaseIdentifier()
    {
        _id = null;
    }

    private PhaseIdentifier(ulong id)
    {
        _id = id;
    }
    
    public static PhaseIdentifier Of<T>() where T : GamePhase
    {
        var id = GamePhaseManager.Registry.CreateID<T>();
        return new PhaseIdentifier(id);
    }

    public static PhaseIdentifier Empty()
    {
        return new PhaseIdentifier();
    }
}