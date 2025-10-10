using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Variable;

public abstract class SyncedVariable<T> : GenericRemoteEvent<T>, ICatchup, IResettable
{
    public delegate void OnChangedHandler(T newValue);
    public delegate bool ValidatorHandler(T newValue);

    private readonly string _name;
    private T _value;

    public event OnChangedHandler? OnValueChanged;
    public event ValidatorHandler? OnValidate;

    protected SyncedVariable(string name, T defaultValue, CatchupMoment moment) : base($"sync.{name}")
    {
        _name = name;
        _value = defaultValue;

        Moment = moment;

        
    }
    
    public static implicit operator T(SyncedVariable<T> variable) => variable.Value;

    protected abstract bool Equals(T a, T b);
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

    public CatchupMoment Moment { get; }

    public void OnCatchup(PlayerID playerId)
    {
        Relay(_value, playerId.SmallID);
    }

    public void Reset()
    {
        _value = default!;
    }
}