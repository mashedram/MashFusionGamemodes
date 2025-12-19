namespace MashGamemodeLibrary.Data.Random;

public class WeightedRandomProvider<TValue> : IRandomProvider<TValue> where TValue : class
{
    public delegate List<TValue> DataProvider();

    private static readonly int BaseWeight = 128;
    private readonly DataProvider _provider;
    private readonly Dictionary<TValue, int> _weights = new();
    private TValue? _lastSelectedValue;
    private int _sameSelectionCount;

    public WeightedRandomProvider(DataProvider provider)
    {
        _provider = provider;
    }

    public TValue? GetRandomValue()
    {
        var value = SelectValue();
        if (value == null)
            return null;

        _weights[value] = GetNewWeight(value);

        return value;
    }

    private int GetWeight(TValue key)
    {
        return _weights.GetValueOrDefault(key, BaseWeight);
    }

    private TValue? SelectValue()
    {
        var values = _provider.Invoke();

        // Remove weights that don't matter anymore
        foreach (var value in _weights.Keys.Except(values)) _weights.Remove(value);

        // Calculate total Weight
        var totalWeight = _weights.Values.Sum() + (values.Count - _weights.Count) * BaseWeight;
        var currentWeight = UnityEngine.Random.RandomRange(0, totalWeight);

        foreach (var value in values)
        {
            var weight = GetWeight(value);
            if (currentWeight <= weight)
                return value;

            currentWeight -= weight;
        }

        return null;
    }

    private int GetNewWeight(TValue value)
    {
        // Only gets called after the null check
        if (_lastSelectedValue == value)
            _sameSelectionCount++;
        else
            _sameSelectionCount = 1;

        _lastSelectedValue = value;

        return BaseWeight - (int)Math.Pow(2, _sameSelectionCount);
    }
}