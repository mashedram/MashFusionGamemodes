using MashGamemodeLibrary.Environment.Effector;
using MashGamemodeLibrary.Environment.Effector.Weather;

namespace TheHunt.Audio.Effectors.Weather;

public abstract class TheHuntWeatherEffector : WeatherEffector<EnvironmentContext>
{
    protected TheHuntWeatherEffector(string[] barcodes, bool ignoreNightmare = false) :
        base(barcodes, ignoreNightmare ? _ => !EnvironmentContext.IsLocalNightmare : null)
    {
    }

    public override Enum Track => EffectorTracks.Weather;
}