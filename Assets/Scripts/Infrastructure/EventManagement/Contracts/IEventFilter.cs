public interface IEventFilter<in TEvent>
    where TEvent : IEvent
{
    bool ShouldHandle(TEvent @event);
}