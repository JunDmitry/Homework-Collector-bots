using System;

public interface IEventAggregator : IDisposable
{
    IEventSubscription Subscribe<TEvent>(ISubscribeCondition<TEvent> condition, Action<TEvent> handler, IEventFilter<TEvent> eventFilter = null)
        where TEvent : IEvent;

    void RegisterSource<TEvent>(IEventSource<TEvent> source)
        where TEvent : IEvent;

    void UnregisterSource<TEvent>(IEventSource<TEvent> source)
        where TEvent : IEvent;
}