using System;

public class FakeSource<TEvent> : IEventSource<TEvent>, IDisposable
    where TEvent : IEvent
{
    private readonly IEventAggregator _eventAggregator;

    public FakeSource(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _eventAggregator.RegisterSource(this);
    }

    public bool IsActive => true;

    public event Action<TEvent> EventRaised;

    public void Invoke(TEvent @event)
    {
        EventRaised?.Invoke(@event);
    }

    public void Dispose()
    {
        _eventAggregator.UnregisterSource(this);
    }
}