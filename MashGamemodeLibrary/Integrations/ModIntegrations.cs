namespace MashGamemodeLibrary.Integrations;

public static class ModIntegrations
{
    public static void TryInitialize()
    {
        SpidermanModIntegrations.TryInitialize();
        MIDTIntegration.TryInitialize();
    }
}