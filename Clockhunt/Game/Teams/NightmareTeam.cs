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

    private int _nightmareID;
    public int NightmareID => _nightmareID;

    public NightmareTeam()
    {
    }

    public NightmareTeam(int nightmareID)
    {
        _nightmareID = nightmareID;
    }

    public override void OnAssigned()
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.SetRandomNightmare(Owner.PlayerID);
            MelonLogger.Msg("Assigned nightmare to player " + Owner.Username + " with nightmare ID " + NightmareID);
        });
    }

    public override void OnRemoved()
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.RemoveNightmare(Owner.PlayerID);
        });
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _nightmareID);
    }
}