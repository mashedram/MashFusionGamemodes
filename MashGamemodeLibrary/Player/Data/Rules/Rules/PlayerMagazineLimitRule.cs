using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Rules.Rules;

public class PlayerMagazineLimitRule : IPlayerRule
{
    private int? _magazineLimit;

    public int? MagazineLimit
    {
        get => _magazineLimit;
        set => _magazineLimit = value;
    }
    public bool IsEnabled => _magazineLimit != null;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _magazineLimit);
    }
}