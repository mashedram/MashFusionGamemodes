using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Player.Data.Rules.Rules;

public class PlayerAmmunitionLimitRule : IPlayerRule
{
    private int? _ammunitionLimit;

    public int? AmmunitionLimit
    {
        get => _ammunitionLimit;
        set => _ammunitionLimit = value;
    }
    public bool IsEnabled => _ammunitionLimit != null;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _ammunitionLimit);
    }
    
    public int GetHash()
    {
        return _ammunitionLimit ?? -1;
    }
}