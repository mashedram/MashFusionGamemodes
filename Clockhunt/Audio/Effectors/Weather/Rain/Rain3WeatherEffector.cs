using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public class Rain3WeatherEffector : ClockhuntWeatherEffector
{
    public Rain3WeatherEffector() : base(new []
    {
        "FirePura.BoneWeater.Spawnable.Fog",
        "FirePura.BoneWeater.Spawnable.ThunderStorm"
    }, true)
    {
    }
}