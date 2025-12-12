public interface ISubscribeCondition<in TEvent>
    where TEvent : IEvent
{
    bool CanSubscribe(IEventSource<TEvent> eventSource);
}