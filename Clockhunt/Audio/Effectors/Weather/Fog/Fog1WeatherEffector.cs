using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather.Fog;

public class Fog1WeatherEffector : ClockhuntWeatherEffector
{
    public Fog1WeatherEffector() : base(new []
    {
        "FirePura.BoneWeater.Spawnable.FogLight"
    })
    {
    }
}