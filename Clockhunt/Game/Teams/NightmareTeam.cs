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

    protected override void OnAssigned()
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.SetRandomNightmare(Owner.PlayerID);
            MelonLogger.Msg("Assigned nightmare to player " + Owner + " with nightmare ID " + NightmareID);
        });
    }

    protected override void OnRemoved()
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.RemoveNightmare(Owner.PlayerID);
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