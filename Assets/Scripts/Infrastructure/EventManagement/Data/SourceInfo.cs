using System.Collections.Generic;

internal class SourceInfo<TEvent>
    where TEvent : IEvent
{
    public SourceInfo(IEventSource<TEvent> source)
    {
        Source = source;
        Subscriptions = new();
    }

    public IEventSource<TEvent> Source { get; }
    public List<SubscriptionInfo<TEvent>> Subscriptions { get; }
}