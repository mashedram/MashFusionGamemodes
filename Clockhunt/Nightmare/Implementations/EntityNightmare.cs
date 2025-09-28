using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.SDK.Triggers;
using MashGamemodeLibrary.Player;

namespace Clockhunt.Nightmare.Implementations;

public class EntityNightmareInstance : NightmareInstance
{
    public EntityNightmareInstance(NetworkPlayer owner, EntityNightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override void OnAbilityKeyTapped(Handedness handedness)
    {
        
    }
}

public class EntityNightmareDescriptor : NightmareDescriptor
{
    public override string Name => "Entity";
    public override string Description => "Kill all players.";
    public override string Avatar => "Random.OWNTeamAvatars.Avatar.Entiity";
    public override int Weight => 10;
    public override float AbilityCooldown => 60f;

    public override PlayerStats Stats => new()
    {
        Vitality = 30f,
        UpperStrength = 8f,
        Speed = 3f,
        Agility = 2f,
        LowerStrength = 8f
    };
    
    public override NightmareInstance CreateInstance(NetworkPlayer owner)
    {
        return new EntityNightmareInstance(owner, this);
    }
}