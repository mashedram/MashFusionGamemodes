using System.Collections.Immutable;
using Clockhunt.Audio.Effectors.Weather.Fog;
using Clockhunt.Config;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public class Weather1Effector : MappedSelector<ClockhuntMusicContext, WeatherType>
{
    protected override void BuildMap(ref Dictionary<WeatherType, EnvironmentEffector<ClockhuntMusicContext>> map)
    {
        map.Add(WeatherType.Fog, new Fog1WeatherEffector());
        map.Add(WeatherType.Rain, new Rain1WeatherEffector());
    }

    protected override WeatherType Selector(ClockhuntMusicContext context)
    {
        return ClockhuntConfig.WeatherType;
    }
}