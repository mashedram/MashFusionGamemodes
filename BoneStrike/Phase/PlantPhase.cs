using BoneStrike.Tags;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Util.Timer;

namespace BoneStrike.Phase;

public class PlantPhase : GamePhase
{
    private static readonly string ClockBarcode = "SLZ.BONELAB.Content.Spawnable.AlarmClock";
    
    public override string Name => "Plant Phase";
    public override float Duration => BoneStrike.Config.PlantDuration;

    public static readonly SyncedVariable<bool> PhaseShouldQuit = new("SkipPlantPhase", new BoolEncoder(), false, CommonNetworkRoutes.AllToAll);
    
    protected override TimeMarker[] Markers => new[]
    {
        CommonTimeMarkerEvents.TimeRemaining(10f),
        CommonTimeMarkerEvents.TimeRemaining(60f)
    };

    public override PhaseIdentifier GetNextPhase()
    {
        if (HasReachedDuration() || PhaseShouldQuit.Value) return PhaseIdentifier.Of<DefusePhase>();
        
        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        PhaseShouldQuit.Value = false;
        LocalInventory.SetAmmo(2000);
        Executor.RunIfHost(() =>
        {
            var preset = new PalletLoadouts(BoneStrike.Config.PalletBarcode);
            
            foreach (var networkPlayer in NetworkPlayer.Players)
            {
                var loadout = preset.GetLoadout();
                loadout.Assign(networkPlayer.RigRefs);
            }

            var position = BoneStrike.Context.LocalPlayer.RigRefs.RightHand.transform.position;
            GameAssetSpawner.SpawnNetworkAsset(ClockBarcode, position, new BombMarker(), new DefusableTag(7f));
        });
    }

    public override void OnPlayerAction(PlayerID playerId, PlayerGameActions action, Handedness handedness)
    {
        if (!playerId.IsMe) 
            return;
        
        if (action != PlayerGameActions.Ability) 
            return;
        
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player))
            return;
        

        if (!PlayerGrabManager.IsHoldingTag<BombMarker>(player)) 
            return;

        PhaseShouldQuit.Value = true;
    }
}