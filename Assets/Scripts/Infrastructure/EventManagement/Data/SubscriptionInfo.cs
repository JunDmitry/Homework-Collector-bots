using System;
using System.Collections.Generic;
using UnityEngine;

internal class SubscriptionInfo<TEvent> : IEventSubscription
    where TEvent : IEvent
{
    private readonly ISubscribeCondition<TEvent> _condition;
    private readonly IEventFilter<TEvent> _filter;
    private readonly Action<TEvent> _handler;
    private readonly HashSet<IEventSource<TEvent>> _eventSources;
    private readonly object _lock = new object();

    private bool _disposed = false;

    public SubscriptionInfo(
        ISubscribeCondition<TEvent> condition,
        Action<TEvent> handler,
        IEventFilter<TEvent> eventFilter = null)
    {
        ThrowIf.Null(condition, nameof(condition));
        ThrowIf.Null(handler, nameof(handler));

        _condition = condition;
        _filter = eventFilter;
        _handler = handler;
        _eventSources = new();
    }

    public bool IsSubscribed => _disposed == false;

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (IEventSource<TEvent> eventSource in _eventSources)
            {
                eventSource.EventRaised -= OnEventRaised;
            }

            _eventSources.Clear();
        }
    }

    public void Unsubscribe()
    {
        Dispose();
    }

    public void TrySubscribeToSource(IEventSource<TEvent> eventSource)
    {
        lock (_lock)
        {
            if (_disposed || _eventSources.Contains(eventSource))
                return;

            if (_condition.CanSubscribe(eventSource))
            {
                eventSource.EventRaised += OnEventRaised;
                _eventSources.Add(eventSource);
            }
        }
    }

    public void UnsubscribeFromSource(IEventSource<TEvent> eventSource)
    {
        lock (_lock)
        {
            if (_eventSources.Remove(eventSource))
                eventSource.EventRaised -= OnEventRaised;
        }
    }

    private void OnEventRaised(TEvent @event)
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            if (_filter != null && _filter.ShouldHandle(@event) == false)
                return;

            try
            {
                _handler?.Invoke(@event);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in event handler: {ex}");
            }
        }
    }
}