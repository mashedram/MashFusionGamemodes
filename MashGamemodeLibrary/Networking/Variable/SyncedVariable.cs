using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Variable;

public abstract class SyncedVariable<T> : GenericRemoteEvent<T>, ICatchup, IResettable
{
    public delegate void OnChangedHandler(T newValue);
    public delegate bool ValidatorHandler(T newValue);

    private readonly string _name;
    private readonly T _default;
    private T _value;

    public event OnChangedHandler? OnValueChanged;
    public event ValidatorHandler? OnValidate;

    protected SyncedVariable(string name, T defaultValue, INetworkRoute? route = null) : base($"sync.{name}", route)
    {
        _name = name;
        _default = defaultValue;
        _value = defaultValue;
    }
    
    public static implicit operator T(SyncedVariable<T> variable) => variable.Value;

    protected abstract bool Equals(T a, T b);
    protected abstract T ReadValue(NetReader reader);
    protected abstract void WriteValue(NetWriter writer, T value);

    protected override void Write(NetWriter writer, T data)
    {
        WriteValue(writer, data);
    }

    protected override void Read(byte playerId, NetReader reader)
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
        if (Equals(_value, newValue))
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

    public void OnCatchup(PlayerID playerId)
    {
        Relay(_value, playerId.SmallID);
    }

    public void Reset()
    {
        _value = _default;
    }
}