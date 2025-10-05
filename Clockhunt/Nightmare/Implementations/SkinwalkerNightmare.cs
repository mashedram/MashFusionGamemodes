using Clockhunt.Game;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network;
using MashGamemodeLibrary.Player;
using UnityEngine;

namespace Clockhunt.Nightmare.Implementations;

public class SkinwalkerNightmareInstance : NightmareInstance
{
    public SkinwalkerNightmareInstance(NetworkPlayer owner, NightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override bool CanGrab(NetworkEntity entity, MarrowEntity marrowEntity)
    {
        return true;
    }

    public override bool CanStartTensionMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return false;
    }

    public override bool CanStartChaseMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return false;
    }

    public override void OnApplied()
    {
        if (!NetworkInfo.IsHost) return;
        WinStateManager.SetLives(0, false);
    }
}

public class SkinwalkerNightmareDescriptor : NightmareDescriptor
{
    public  override string Name => "Skinwalker";
    public  override string HunterDescription => "Player lives have been reduced to 0. Kill them all before they escape.";
    public  override string SurvivorDescription => "You have no lives. Work with your team to escape or eliminate the nightmare.";
    public override string? Avatar => null;
    public override bool RegenerateHealth => true;
    public override int Weight => 4;

    public override PlayerStats Stats => new()
    {
        Vitality = 5f,
        UpperStrength = 2f,
        Speed = 1f,
        Agility = 1.5f,
        LowerStrength = 2f
    };

    public override NightmareInstance CreateInstance(NetworkPlayer player)
    {
        return new SkinwalkerNightmareInstance(player, this);
    }
}