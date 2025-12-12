public interface IOwnerEventSource<out TEvent> : IEventSource<TEvent>
    where TEvent : IEvent
{
    int OwnerInstanceId { get; }
}