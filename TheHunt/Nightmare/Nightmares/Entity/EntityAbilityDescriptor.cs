using MashGamemodeLibrary.Player;
using TheHunt.Nightmare.Ability;
using TheHunt.Nightmare.Ability.Active;
using TheHunt.Nightmare.Ability.Passive;

namespace TheHunt.Nightmare.Nightmares.Entity;

public class EntityAbilityDescriptor : INightmareDescriptor
{
    public string AvatarBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Avatar.CharTallv4";
    public AvatarStats AvatarStats => new()
    {
        Vitality = 3f,
        UpperStrength = 8f,
        Speed = Gamemode.TheHunt.Config.NightmareSpeed,
        Agility = 2f,
        LowerStrength = 8f
    };
    
    public IEnumerable<IAbility> Abilities => new IAbility[]
    {
        new ScreamAbility(),
        new DashAbility(),
        new FaintGlowAbility()
    };

    public float MinimumSpeed => 0.5f;
    public float SpeedPenaltyPerShot => 0.2f;
    public float SpeedPenaltyDuration => 5f;
    public float SpeedPenaltyHealDelay => 2f;

}