using LabFusion.Extensions;

namespace MashGamemodeLibrary.Data.Random;

public class BasicRandomProvider<TValue> : IRandomProvider<TValue> where TValue : class
{
    public delegate List<TValue> DataProvider();

    private readonly DataProvider _provider;

    public BasicRandomProvider(DataProvider provider)
    {
        _provider = provider;
    }

    public TValue? GetRandomValue()
    {
        var list = _provider.Invoke();

        return list.Count == 0 ? default : list.GetRandom();
    }
}