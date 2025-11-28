public readonly struct DeliveryContext<T>
    where T : IDeliverable
{
    public DeliveryContext(
        T item,
        IItemSource<T> source,
        IItemDestination<T> destination)
    {
        Item = item;
        Source = source;
        Destination = destination;
    }

    public T Item { get; }
    public IItemSource<T> Source { get; }
    public IItemDestination<T> Destination { get; }
}
