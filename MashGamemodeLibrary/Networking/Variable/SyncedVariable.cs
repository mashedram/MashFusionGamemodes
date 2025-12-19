using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable.Encoder;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Variable;

public class SyncedVariable<TValue> : GenericRemoteEvent<TValue>, ICatchup, IResettable
{
    public delegate void OnChangedHandler(TValue newValue);

    public delegate bool ValidatorHandler(TValue newValue);
    private readonly TValue _default;

    private readonly IEncoder<TValue> _encoder;

    private readonly string _name;
    private TValue _value;

    public SyncedVariable(string name, IEncoder<TValue> encoder, TValue defaultValue) : this(name, encoder, defaultValue, CommonNetworkRoutes.HostToAll)
    {
    }

    public SyncedVariable(string name, IEncoder<TValue> encoder, TValue defaultValue, INetworkRoute route) : base($"sync.{name}", route)
    {
        _name = name;
        _encoder = encoder;
        _default = defaultValue;
        _value = defaultValue;
    }

    public TValue Value
    {
        get => _value;
        set => SetValue(value);
    }

    public void OnCatchup(PlayerID playerId)
    {
        Relay(_value, playerId.SmallID);
    }

    public void Reset()
    {
        _value = _default;
    }

    public event OnChangedHandler? OnValueChanged;
    public event ValidatorHandler? OnValidate;

    public static implicit operator TValue(SyncedVariable<TValue> variable)
    {
        return variable.Value;
    }

    protected override int? GetSize(TValue data)
    {
        return _encoder.GetSize(data);
    }

    protected override void Write(NetWriter writer, TValue data)
    {
        _encoder.Write(writer, data);
    }

    protected override void Read(byte smallId, NetReader reader)
    {
        _value = _encoder.Read(reader);
        OnValueChanged?.Invoke(_value);
    }

    public void SetAndSync(TValue value)
    {
        var isValid = OnValidate?.Invoke(value) ?? true;
        if (!isValid)
        {
#if DEBUG
            MelonLogger.Warning($"[SyncedVariable<{typeof(TValue)}>] Attempted to set invalid value for variable '{_name}'");
#endif
            return;
        }

        _value = value;
        OnValueChanged?.Invoke(_value);
        Sync();
    }

    public void Sync()
    {
        Relay(_value);
    }

    private void SetValue(TValue newValue)
    {
        if (Equals(_value, newValue))
            return;

        SetAndSync(newValue);
    }
}