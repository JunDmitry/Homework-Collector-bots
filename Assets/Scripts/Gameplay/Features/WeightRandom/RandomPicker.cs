using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomPicker<T>
{
    private readonly List<T> _values;
    private readonly List<float> _weights;

    private float _totalWeight;
    private System.Random _random;
    private int _seed;

    public RandomPicker() :
        this(UnityEngine.Random.Range(int.MinValue, int.MaxValue))
    { }

    public RandomPicker(int seed)
    {
        _values = new();
        _weights = new();
        _totalWeight = 0;
        _seed = seed;

        ChangeSeed(seed);
    }

    public IReadOnlyList<T> Values => _values.AsReadOnly();
    public int Count => _values.Count;
    public float TotalWeight => _totalWeight;

    public void ChangeSeed(int seed)
    {
        _seed = seed;
        _random = new(seed);
    }

    public T Pick()
    {
        return Pick(0f, TotalWeight);
    }

    public T Pick(float minWeightInclusive, float maxWeightExclusive)
    {
        ThrowIf.Invalid(Count == 0,
            $"{nameof(Count)} elements in {nameof(RandomPicker<T>)} should be positive");
        ThrowIf.Argument(minWeightInclusive < 0 || minWeightInclusive >= maxWeightExclusive,
            $"{nameof(minWeightInclusive)} can't be negative and more or equal than {nameof(maxWeightExclusive)}",
            nameof(minWeightInclusive));
        ThrowIf.Argument(maxWeightExclusive > TotalWeight,
            $"{nameof(maxWeightExclusive)} can't be more than {nameof(TotalWeight)}",
            nameof(maxWeightExclusive));

        float randomWeight = minWeightInclusive + (float)_random.NextDouble() * (maxWeightExclusive - minWeightInclusive);
        int index = Mathf.Max(0, _weights.FindIndexWithLessOrEqual(randomWeight));

        return _values[index];
    }

    public void Add(T value, float weight)
    {
        ThrowIf.Argument(weight <= 0f, $"{nameof(weight)} should be positive", nameof(weight));

        _totalWeight += weight;
        _values.Add(value);
        _weights.Add(_totalWeight);
    }

    public bool Remove(T value)
    {
        int index = _values.IndexOf(value);

        return Remove(index);
    }

    public bool Remove(int index)
    {
        if (index < 0 || index >= _values.Count)
            return false;

        int previousIndexStep = 1;
        float subtractWeight = index == 0 ? _weights.First() : _weights[index] - _weights[index - previousIndexStep];

        _values.RemoveAt(index);
        _weights.RemoveAt(index);

        while (index < _weights.Count)
            _weights[index++] -= subtractWeight;

        return true;
    }

    public void Clear()
    {
        _values.Clear();
        _weights.Clear();
        _totalWeight = 0;
        _random = new(_seed);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _values.GetEnumerator();
    }
}