using LabFusion.Entities;
using LabFusion.SDK.Triggers;
using MashGamemodeLibrary.Player;

namespace Clockhunt.Nightmare;

public abstract class NightmareDescriptor
{
    public int ID => GetType().FullName?.GetHashCode() ?? throw new Exception("Type has no FullName");
    
    public bool IsEnabled { get; set; } = true;
    public abstract string Name { get; }
    public abstract string Description { get; }
    public string Barcode => $"Nightmare.{Name}";
    public virtual int Weight => 1;
    public virtual bool RegenerateHealth => false;
    public virtual bool KillOnGrab => false;
    
    public virtual float AbilityCooldown => 30f;

    /**
     * When null, avatar will be the avatar the player is using
     */
    public abstract string? Avatar { get; }

    public abstract PlayerStats Stats { get; }

    public abstract NightmareInstance CreateInstance(NetworkPlayer player);

    public virtual void Register()
    {
    }

    public virtual void Unregister()
    {
    }
}