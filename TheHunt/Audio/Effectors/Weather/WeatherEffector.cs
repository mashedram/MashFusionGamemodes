using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector;
using TheHunt.Audio.Effectors.Weather.Fog;
using TheHunt.Audio.Effectors.Weather.FogLight;
using TheHunt.Audio.Effectors.Weather.Night;
using TheHunt.Audio.Effectors.Weather.Rain;

namespace TheHunt.Audio.Effectors.Weather;

public class WeatherEffector : MappedSelector<EnvironmentContext, WeatherType>
{
    public override Enum Track => EffectorTracks.Weather;

    protected override void BuildMap(ref Dictionary<WeatherType, EnvironmentEffector<EnvironmentContext>> map)
    {
        map.Add(WeatherType.None, new NoneWeatherEffector());
        map.Add(WeatherType.Fog, new FogWeatherEffector());
        map.Add(WeatherType.Rain, new RainWeatherEffector());
        map.Add(WeatherType.Night, new NightWeatherEffector());
        map.Add(WeatherType.FogLight, new FogLightWeatherEffector());
    }

    protected override WeatherType Selector(EnvironmentContext context)
    {
        return Gamemode.TheHunt.Config.WeatherType;
    }
}