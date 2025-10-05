using System.Collections.Immutable;
using Clockhunt.Audio.Effectors.Weather.Fog;
using Clockhunt.Config;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public class Weather2Effector : MappedSelector<ClockhuntMusicContext, WeatherType>
{
    protected override void BuildMap(ref Dictionary<WeatherType, EnvironmentEffector<ClockhuntMusicContext>> map)
    {
        map.Add(WeatherType.None, new NoneWeatherEffector());
        map.Add(WeatherType.Fog, new Fog2WeatherEffector());
        map.Add(WeatherType.Rain, new Rain2WeatherEffector());
    }

    protected override WeatherType Selector(ClockhuntMusicContext context)
    {
        return ClockhuntConfig.WeatherType.Value;
    }
    
    public override Enum Track => EffectorTracks.Weather;
}