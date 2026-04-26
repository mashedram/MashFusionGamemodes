using MashGamemodeLibrary.Player;
using TheHunt.Nightmare.Ability;
using TheHunt.Nightmare.Ability.Active;
using TheHunt.Nightmare.Ability.Passive;

namespace TheHunt.Nightmare.Nightmares.Entity;

public class StealthDescriptor : INightmareDescriptor
{
    public string AvatarBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Avatar.CharTallv4";
    public AvatarStats AvatarStats => new()
    {
        Vitality = 3f,
        UpperStrength = 8f,
        Speed = Gamemode.TheHunt.Config.NightmareSpeed * 1.15f,
        Agility = 2f,
        LowerStrength = 8f
    };
    
    public IEnumerable<IAbility> Abilities => new IAbility[]
    {
        new StealthAbility()
    };

    public float MinimumSpeed => 0.7f;
    public float SpeedPenaltyPerShot => 0.1f;
    public float SpeedPenaltyDuration => 3f;
    public float SpeedPenaltyHealDelay => 1f;

}