using LabFusion.Extensions;
using MashGamemodeLibrary.Environment.Effector.Weather;

namespace MashGamemodeLibrary.Environment.Effector;

public class WeatherEffector<TContext> : EnvironmentEffector<TContext>
{
    private readonly string[] _barcodes;
    private readonly Func<TContext, bool>? _canApply;

    public WeatherEffector(string[] barcodes, Func<TContext, bool>? canApply = null)
    {
        _barcodes = barcodes;
        _canApply = canApply;
    }

    public override bool CanApply(TContext context)
    {
        if (_canApply == null)
            return true;

        return _canApply.Invoke(context);
    }

    public override void Apply(TContext context)
    {
        _barcodes.ForEach(LocalWeatherManager.SpawnLocalWeather);
    }

    public override void Remove(TContext context)
    {
        LocalWeatherManager.ClearLocalWeather();
    }
}