public class CollectedEvent<T> : IEvent
{
    public CollectedEvent(T collectedItem)
    {
        CollectedItem = collectedItem;
    }

    public T CollectedItem { get; }
}