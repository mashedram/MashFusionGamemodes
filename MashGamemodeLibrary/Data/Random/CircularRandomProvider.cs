namespace MashGamemodeLibrary.Data.Random;

public class CircularRandomProvider<TValue> : IRandomProvider<TValue>
{
    public delegate List<TValue> DataProvider();

    private readonly DataProvider _provider;
    private int _index;

    public CircularRandomProvider(DataProvider provider)
    {
        _provider = provider;
    }

    public TValue? GetRandomValue()
    {
        var list = _provider.Invoke();

        if (list.Count == 0)
            return default;

        var stepSize = list.Count - 1;
        var step = UnityEngine.Random.RandomRange(1, stepSize);
        _index = (_index + step) % list.Count;

        return list[_index];
    }
}