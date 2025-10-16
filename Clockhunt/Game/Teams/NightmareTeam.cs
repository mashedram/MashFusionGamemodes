using Clockhunt.Config;
using Clockhunt.Nightmare;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Team;
using MelonLoader;

namespace Clockhunt.Game.Teams;

public class NightmareTeam : Team, INetSerializable
{
    public override string Name => "Nightmare";
    public override uint Capacity => 1;

    private ulong _nightmareID;
    public ulong NightmareID => _nightmareID;

    public NightmareTeam() {}

    public NightmareTeam(ulong nightmareID)
    {
        _nightmareID = nightmareID;
    }

    public override void OnAssigned(PlayerID player)
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.SetRandomNightmare(player);
            MelonLogger.Msg("Assigned nightmare to player " + player + " with nightmare ID " + NightmareID);
        });
    }

    public override void OnRemoved(PlayerID player)
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.RemoveNightmare(player);
        });
    }

    public int? GetSize()
    {
        return sizeof(ulong);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _nightmareID);
    }
}