using System.Collections.Immutable;
using Clockhunt.Audio.Effectors.Weather.Fog;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public class Weather3Effector : MappedSelector<ClockhuntMusicContext, WeatherType>
{
    protected override void BuildMap(ref Dictionary<WeatherType, EnvironmentEffector<ClockhuntMusicContext>> map)
    {
        map.Add(WeatherType.Fog, new Fog3WeatherEffector());
        map.Add(WeatherType.Rain, new Rain3WeatherEffector());
    }

    protected override WeatherType Selector(ClockhuntMusicContext context)
    {
        throw new NotImplementedException();
    }
}