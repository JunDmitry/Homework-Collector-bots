public class UnitCreatedEvent<T> : IEvent
{
    public UnitCreatedEvent(T createdUnit)
    {
        CreatedUnit = createdUnit;
    }

    public T CreatedUnit { get; }
}