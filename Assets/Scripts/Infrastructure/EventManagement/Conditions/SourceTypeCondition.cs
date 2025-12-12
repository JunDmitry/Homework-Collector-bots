public class SourceTypeCondition<TEvent, TAsType> : ISubscribeCondition<TEvent>
    where TEvent : IEvent
    where TAsType : class, IEventSource<TEvent>
{
    public bool CanSubscribe(IEventSource<TEvent> eventSource)
    {
        return eventSource is TAsType;
    }
}