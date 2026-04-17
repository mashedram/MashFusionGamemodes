namespace TheHunt.Audio.Effectors.Weather.Rain;

public class RainWeatherEffector : TheHuntWeatherEffector
{
    public RainWeatherEffector() : base(new[]
    {
        "FirePura.BoneWeater.Spawnable.HeavyRain"
    }, true)
    {
    }
}