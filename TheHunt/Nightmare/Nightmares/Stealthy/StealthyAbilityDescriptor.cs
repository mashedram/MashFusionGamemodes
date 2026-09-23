using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player;
using TheHunt.Nightmare.Ability;
using TheHunt.Nightmare.Ability.Active;
using TheHunt.Nightmare.Ability.Passive;

namespace TheHunt.Nightmare.Nightmares.Stealthy;

public class StealthyAbilityDescriptor //:  INightmareDescriptor
{
    public string AvatarBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Avatar.CharTallv4";

    public string Name => "The Stealthy";

    public AvatarStats AvatarStats => new()
    {
        Vitality = 3f,
        UpperStrength = 8f,
        Speed = Gamemode.TheHunt.Config.NightmareSpeed,
        Agility = 2f,
        LowerStrength = 8f
    };
    
    public IEnumerable<Func<IAbility>> AbilityFactories => new Func<IAbility>[]
    {
        () => new KillOnTouch(),
        () => new KillableEntity(
            5f,
            10,
            20f,
            25f
        ),
        () => new TrackAbility()
    };

    public IEnumerable<Type> RequiredModifiers { get; } = new List<Type>();
    public IList<Type>? BannedModifiers { get; } = new List<Type>();
}