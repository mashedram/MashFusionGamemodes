using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;

namespace MashGamemodeLibrary.networking.Variable;

public abstract class SyncedVariable<T> : GenericRemoteEvent<T>
{
    private readonly string _name;
    private T value;
    
    public SyncedVariable(string name, T defaultValue) : base($"sync.{name}")
    {
        _name = name;
        value = defaultValue;

        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
    }
    
    ~SyncedVariable() {
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoined;
    }

    protected abstract T ReadValue(NetReader reader);
    protected abstract void WriteValue(NetWriter writer, T value);

    protected override void Write(NetWriter writer, T data)
    {
        WriteValue(writer, data);
    }

    protected override void Read(NetReader reader)
    {
        value = ReadValue(reader);
    }
    
    public T Value
    {
        get => value;
        set => SetValue(value);
    }
    
    private void SetValue(T newValue)
    {
        value = newValue;
        Relay(value);
    }
    
    private void OnPlayerJoined(PlayerID playerId)
    {
        Relay(value);
    }
}