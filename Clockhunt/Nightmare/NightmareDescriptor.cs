using Clockhunt.Nightmare.Config;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Util;

namespace Clockhunt.Nightmare;

public abstract class NightmareDescriptor
{
    public bool Enabled = false;
    public ulong ID => GetType().FullName?.GetStableHash() ?? throw new Exception("Type has no FullName");

    public abstract string Name { get; }
    public abstract string HunterDescription { get; }

    public abstract string SurvivorDescription { get; }

    public string Barcode => $"Nightmare.{Name}";
    public virtual bool RegenerateHealth => false;
    public virtual bool KillOnGrab => false;
    /**
     * When null, avatar will be the avatar the player is using
     */
    public abstract string? Avatar { get; }

    public virtual Type ConfigType => typeof(NightmareConfig);
    public virtual Func<NightmareConfig> ConfigFactory => () => new NightmareConfig();
    
    public T GetConfig<T>() where T : NightmareConfig
    {
        return NightmareManager.GetConfig<T>(this);
    }

    public abstract PlayerStats Stats { get; }

    public virtual PlayerStats GetStats()
    {
        return Stats;
    }

    public abstract NightmareInstance CreateInstance(byte ownerID);

    public virtual void Register()
    {
    }

    public virtual void Unregister()
    {
    }
}