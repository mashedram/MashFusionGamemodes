namespace TheHunt.Player.Speed;

public static class LocalSpeed
{
    private static float _speedModifier = 1f;
    public static float SpeedModifier
    {
        get => _speedModifier;
        set => _speedModifier = MathF.Min(MathF.Max(value, 0.1f), 1f);
    }
}