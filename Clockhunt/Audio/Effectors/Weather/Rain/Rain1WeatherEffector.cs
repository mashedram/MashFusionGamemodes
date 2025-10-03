using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public class Rain1WeatherEffector : ClockhuntWeatherEffector
{
    public Rain1WeatherEffector() : base(new []
    {
        "FirePura.BoneWeater.Spawnable.Night",
        "FirePura.BoneWeater.Spawnable.Rainy"
    })
    {
    }
}