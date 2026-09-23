using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player;
using TheHunt.Modifiers.Modifiers;
using TheHunt.Nightmare.Ability;
using TheHunt.Nightmare.Ability.Active;
using TheHunt.Nightmare.Ability.Passive;

namespace TheHunt.Nightmare.Nightmares.Base;

public class BaseNightmareDescriptor : INightmareDescriptor
{
    public string AvatarBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Avatar.CharTallv4";

    public string Name => "The Entity";

    public AvatarStats AvatarStats => new()
    {
        Vitality = 3f,
        UpperStrength = 8f,
        Speed = Math.Max(Gamemode.TheHunt.Config.NightmareSpeed, Gamemode.TheHunt.Config.HiderSpeed) + 0.15f,
        Agility = 2f,
        LowerStrength = 8f
    };
    
    public IEnumerable<Func<IAbility>> AbilityFactories => new Func<IAbility>[]
    {
        () => new ScreamAbility(
            Handedness.BOTH,
            30f
        ),
        () => new FaintGlowAbility(),
        () => new SlowOnDamage(
            1f,
            0.5f,
            5f,
            2f,
            0.5f
        )
    };

    public IEnumerable<Type> RequiredModifiers { get; } = new List<Type>();
    public IList<Type>? BannedModifiers { get; } = new List<Type>()
    {
        typeof(BlackoutModifier)
    };
}