using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Implementations;
using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.SDK.Gamemodes;
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
    public static Clockhunt Instance = null!;
    
    public override string Title => "Clockhunt";
    public override string Author => "Mash";

    public override void OnGamemodeRegistered()
    {
        base.OnGamemodeRegistered();
        
        Instance = this;
        EntityTagManager.RegisterAll<Mod>();
        
        NightmareManager.Register<EntityNightmareDescriptor>();
    }

    public override void OnGamemodeStarted()
    {
        Context.PhaseManager.ResetPhases();
    }
}