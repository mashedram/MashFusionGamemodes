using TheHunt.Scene;

namespace TheHunt.Modifiers.Modifiers;

public class BlackoutModifier : IModifier
{
    public string Name { get; } = "Blackout";
 
    public void Apply()
    {
        LightManager.SetBlackout(true);
    }

    public void Remove()
    {
        LightManager.SetBlackout(false);
    }
}