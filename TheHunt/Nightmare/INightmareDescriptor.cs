using MashGamemodeLibrary.Player;
using TheHunt.Nightmare.Ability;

namespace TheHunt.Nightmare;

public interface INightmareDescriptor
{
    public string AvatarBarcode { get; }
    public AvatarStats AvatarStats { get; }
    // These create new instances on every call
    public IEnumerable<IAbility> Abilities { get; }
}