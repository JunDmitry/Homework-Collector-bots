using System;
using System.Collections.Generic;

internal class EventTypeRegistry<TEvent> : IDisposable
    where TEvent : IEvent
{
    public EventTypeRegistry()
    {
        Sources = new();
        Subscriptions = new();
    }

    public List<IEventSource<TEvent>> Sources { get; }
    public List<SubscriptionInfo<TEvent>> Subscriptions { get; }

    public void Dispose()
    {
        foreach (SubscriptionInfo<TEvent> subscription in Subscriptions)
            subscription.Dispose();

        Sources.Clear();
        Subscriptions.Clear();
    }
}