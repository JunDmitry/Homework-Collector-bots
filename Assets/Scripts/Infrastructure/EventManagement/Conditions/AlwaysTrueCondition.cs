public class AlwaysTrueCondition<TEvent> : ISubscribeCondition<TEvent>
    where TEvent : IEvent
{
    public bool CanSubscribe(IEventSource<TEvent> eventSource)
    {
        return true;
    }
}