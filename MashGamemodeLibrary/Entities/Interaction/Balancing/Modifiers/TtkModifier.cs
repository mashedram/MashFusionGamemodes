namespace MashGamemodeLibrary.Entities.Interaction.Balancing.Modifiers;

public record TtkModifier(float Ttk, float MedianRps, float StepRps) : IGunModifier
{
    public float GetModifier(GunInfo gunInfo)
    {
        var offsetRps = gunInfo.RoundsPerSecond - MedianRps;
        var offsetSteps = offsetRps / StepRps;
        // We assume that players have 1 hp, and deal damage according to that
        return 1f / Ttk + offsetSteps;
    }
}