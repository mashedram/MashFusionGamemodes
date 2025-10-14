using Clockhunt.Audio.Effectors.Weather.Fog;
using Clockhunt.Audio.Effectors.Weather.Night;
using Clockhunt.Config;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public class Weather3Effector : MappedSelector<ClockhuntMusicContext, WeatherType>
{
    public override Enum Track => EffectorTracks.Weather;

    protected override void BuildMap(ref Dictionary<WeatherType, EnvironmentEffector<ClockhuntMusicContext>> map)
    {
        map.Add(WeatherType.None, new NoneWeatherEffector());
        map.Add(WeatherType.Fog, new Fog3WeatherEffector());
        map.Add(WeatherType.Rain, new Rain3WeatherEffector());
        map.Add(WeatherType.Night, new NightWeatherEffector());
    }

    protected override WeatherType Selector(ClockhuntMusicContext context)
    {
        return ClockhuntConfig.WeatherType.Value;
    }
}