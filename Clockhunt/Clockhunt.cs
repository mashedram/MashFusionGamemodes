using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Implementations;
using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt;

public class Clockhunt : GamemodeWithContext<ClockhuntContext>
{
    public override string Title => "Clockhunt";
    public override string Author => "Mash";

    public override void OnGamemodeRegistered()
    {
        base.OnGamemodeRegistered();
        
        EntityTagManager.RegisterAll<Mod>();
        NightmareManager.RegisterAll<Mod>();
    }

    public override void OnGamemodeStarted()
    {
        Context.PhaseManager.ResetPhases();
    }
}