using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Weather;

public abstract class ClockhuntWeatherEffector : WeatherEffector<ClockhuntMusicContext>
{
    protected ClockhuntWeatherEffector(string[] barcodes, bool ignoreNightmare = false) :
        base(barcodes, ignoreNightmare ? context => !ClockhuntMusicContext.IsLocalNightmare : null)
    {
    }

    public override Enum Track => EffectorTracks.Weather;
}