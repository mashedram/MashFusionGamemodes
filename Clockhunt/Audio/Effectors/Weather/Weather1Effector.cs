using Clockhunt.Audio.Effectors.Weather.Fog;
using Clockhunt.Audio.Effectors.Weather.FogLight;
using Clockhunt.Audio.Effectors.Weather.Night;
using Clockhunt.Config;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public class Weather1Effector : MappedSelector<ClockhuntMusicContext, WeatherType>
{
    public override Enum Track => EffectorTracks.Weather;

    protected override void BuildMap(ref Dictionary<WeatherType, EnvironmentEffector<ClockhuntMusicContext>> map)
    {
        map.Add(WeatherType.None, new NoneWeatherEffector());
        map.Add(WeatherType.Fog, new Fog1WeatherEffector());
        map.Add(WeatherType.Rain, new Rain1WeatherEffector());
        map.Add(WeatherType.Night, new NightWeatherEffector());
        map.Add(WeatherType.FogLight, new FogLightWeatherEffector());
    }

    protected override WeatherType Selector(ClockhuntMusicContext context)
    {
        return Clockhunt.Config.WeatherType;
    }
}