namespace MashGamemodeLibrary.Data.Random;

public class WeightedRandomProvider<TValue> : IRandomProvider<TValue> where TValue : class
{
    public delegate List<TValue> DataProvider();

    private readonly DataProvider _provider;
    private readonly Dictionary<TValue, int> _weights = new();
    private TValue? _lastSelectedValue;
    private int _sameSelectionCount;

    public WeightedRandomProvider(DataProvider provider)
    {
        _provider = provider;
    }
    
    private void RebuildWeights()
    {
        var values = _provider.Invoke();

        foreach (var value in _weights.Keys.Except(values)) _weights.Remove(value);
        foreach (var value in values.Except(_weights.Keys)) _weights[value] = 1;
    }

    private TValue? SelectValue()
    {
        const int baseWeight = 128;
        var inversedWeights = new Dictionary<TValue, int>();

        foreach (var (key, weight) in _weights)
        {
            var newWeight = Math.Max(1, baseWeight - weight);
            inversedWeights[key] = newWeight;
        }

        var totalWeight = inversedWeights.Values.Sum();
        if (totalWeight <= 0)
            return _weights.Keys.FirstOrDefault();

        var randomValue = UnityEngine.Random.Range(0, totalWeight);
        foreach (var kvp in inversedWeights)
        {
            if (randomValue < kvp.Value)
                return kvp.Key;

            randomValue -= kvp.Value;
        }

        return _weights.Keys.FirstOrDefault();
    }

    private int GetNewWeight(TValue value)
    {
        // Only gets called after the null check
        if (_lastSelectedValue == value)
            _sameSelectionCount++;
        else
            _sameSelectionCount = 1;
        
        _lastSelectedValue = value;

        return _weights[value] + (int)Math.Pow(2, _sameSelectionCount);
    }

    public TValue? GetRandomValue()
    {
        RebuildWeights();

        var value = SelectValue();
        if (value == null)
            return null;
        
        _weights[value] = GetNewWeight(value);

        return value;
    }
}