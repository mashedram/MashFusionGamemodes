using MashGamemodeLibrary.Player;
using TheHunt.Nightmare.Ability;

namespace TheHunt.Nightmare;

public interface INightmareDescriptor
{
    public string Name { get; }
    
    public string AvatarBarcode { get; }
    public AvatarStats AvatarStats { get; }
    // These create new instances on every call
    public IEnumerable<Func<IAbility>> AbilityFactories { get; }
    public IEnumerable<Type> RequiredModifiers { get; }
    public IList<Type>? BannedModifiers { get; }
    
    // Hide Stage
    public bool LockedDuringHide => true;
}