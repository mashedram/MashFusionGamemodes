using MashGamemodeLibrary.Player;
using TheHunt.Nightmare.Ability;

namespace TheHunt.Nightmare;

public interface INightmareDescriptor
{
    public string AvatarBarcode { get; }
    public AvatarStats AvatarStats { get; }
    // These create new instances on every call
    public IEnumerable<IAbility> Abilities { get; }
    
    // Hide Stage
    public bool LockedDuringHide => true;
    
    // Speed config
    public float MinimumSpeed { get; }
    public float SpeedPenaltyPerShot { get; }
    public float SpeedPenaltyDuration { get; }
    public float SpeedPenaltyHealDelay { get; }
}