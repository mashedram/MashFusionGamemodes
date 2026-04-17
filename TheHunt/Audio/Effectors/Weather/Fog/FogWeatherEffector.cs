namespace TheHunt.Audio.Effectors.Weather.Fog;

public class FogWeatherEffector : TheHuntWeatherEffector
{
    public FogWeatherEffector() : base(new[]
    {
        "FirePura.BoneWeater.Spawnable.Fog"
    }, true)
    {
    }
}