public readonly struct PairWeight<T>
{
    public PairWeight(T target, float weight)
    {
        Target = target;
        Weight = weight;
    }

    public T Target { get; }
    public float Weight { get; }

    public static PairWeight<T> Create(T target, float weight)
    {
        return new(target, weight);
    }
}