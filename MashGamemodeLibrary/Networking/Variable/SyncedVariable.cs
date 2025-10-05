using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Variable;

public abstract class SyncedVariable<T> : GenericRemoteEvent<T>
{
    public delegate void OnChangedHandler(T newValue);
    public delegate bool ValidatorHandler(T newValue);
    
    private readonly string _name;
    private T _value;
    
    public event OnChangedHandler? OnValueChanged;
    public event ValidatorHandler? OnValidate;

    protected SyncedVariable(string name, T defaultValue) : base($"sync.{name}")
    {
        _name = name;
        _value = defaultValue;

        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
    }
    
    ~SyncedVariable() {
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoined;
    }
    
    public static implicit operator T(SyncedVariable<T> variable) => variable.Value;

    protected abstract T ReadValue(NetReader reader);
    protected abstract void WriteValue(NetWriter writer, T value);

    protected override void Write(NetWriter writer, T data)
    {
        WriteValue(writer, data);
    }

    protected override void Read(NetReader reader)
    {
        _value = ReadValue(reader);
        OnValueChanged?.Invoke(_value);
    }
    
    public T Value
    {
        get => _value;
        set => SetValue(value);
    }
    
    private void SetValue(T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(_value, newValue))
            return;
        
        var isValid = OnValidate?.Invoke(newValue) ?? true;
        if (!isValid)
        {
            #if DEBUG
            MelonLogger.Warning($"[SyncedVariable<{typeof(T)}>] Attempted to set invalid value for variable '{_name}'");
            #endif
            return;
        }
        
        _value = newValue;
        OnValueChanged?.Invoke(_value);
        Relay(_value);
    }
    
    // TODO: Fix a bug where a player joining triggers this on the sender side too
    private void OnPlayerJoined(PlayerID playerId)
    {
        Executor.RunIfHost(() =>
        {
            Relay(_value, playerId.SmallID);
        });
    }
}