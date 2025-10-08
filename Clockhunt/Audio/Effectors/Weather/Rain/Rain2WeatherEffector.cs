using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public class Rain2WeatherEffector : ClockhuntWeatherEffector
{
    public Rain2WeatherEffector() : base(new []
    {
        "FirePura.BoneWeater.Spawnable.HeavyRain",
    }, true)
    {
    }
}