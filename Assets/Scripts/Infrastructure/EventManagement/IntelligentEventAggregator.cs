using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Base simple ThreadSafe version Absolutely unoptimized IntelligentEventAggregator
/// with possible memory leak and much a lot memory and time usage
/// </summary>
public class IntelligentEventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, object> _registries;
    private readonly object _lock = new();

    private bool _disposed = false;

    public IntelligentEventAggregator()
    {
        _registries = new();
    }

    public IEventSubscription Subscribe<TEvent>(
        ISubscribeCondition<TEvent> condition,
        Action<TEvent> handler,
        IEventFilter<TEvent> eventFilter = null)
        where TEvent : IEvent
    {
        ThrowIfDisposed();
        ThrowIf.Null(condition, nameof(condition));
        ThrowIf.Null(handler, nameof(handler));

        lock (_lock)
        {
            EventTypeRegistry<TEvent> registry = GetOrCreateRegistry<TEvent>();
            SubscriptionInfo<TEvent> subscription = new(condition, handler, eventFilter);

            registry.Subscriptions.Add(subscription);

            foreach (IEventSource<TEvent> item in registry.Sources)
            {
                subscription.TrySubscribeToSource(item);
            }

            return subscription;
        }
    }

    public void RegisterSource<TEvent>(IEventSource<TEvent> source)
        where TEvent : IEvent
    {
        ThrowIfDisposed();
        ThrowIf.Null(source, nameof(source));

        lock (_lock)
        {
            EventTypeRegistry<TEvent> registry = GetOrCreateRegistry<TEvent>();

            if (registry.Sources.Contains(source))
                return;

            registry.Sources.Add(source);

            foreach (SubscriptionInfo<TEvent> subscription in registry.Subscriptions)
            {
                subscription.TrySubscribeToSource(source);
            }
        }
    }

    public void UnregisterSource<TEvent>(IEventSource<TEvent> source)
        where TEvent : IEvent
    {
        ThrowIfDisposed();
        ThrowIf.Null(source, nameof(source));

        lock (_lock)
        {
            EventTypeRegistry<TEvent> registry = GetOrCreateRegistry<TEvent>();

            if (registry.Sources.Remove(source) == false)
                return;

            foreach (SubscriptionInfo<TEvent> subscription in registry.Subscriptions)
            {
                subscription.UnsubscribeFromSource(source);
            }
        }
    }

    public void RefreshSubscription<TEvent>()
        where TEvent : IEvent
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            EventTypeRegistry<TEvent> registry = GetOrCreateRegistry<TEvent>();

            foreach (SubscriptionInfo<TEvent> subscription in registry.Subscriptions)
            {
                if (subscription.IsSubscribed == false)
                    return;

                foreach (IEventSource<TEvent> source in registry.Sources)
                {
                    subscription.UnsubscribeFromSource(source);
                }

                foreach (IEventSource<TEvent> source in registry.Sources)
                {
                    subscription.TrySubscribeToSource(source);
                }
            }
        }
    }

#if DEBUG

    public (int sourceCount, int subscriptionCount) GetStatistics<TEvent>()
        where TEvent : class, IEvent
    {
        lock (_lock)
        {
            EventTypeRegistry<TEvent> registry = GetOrCreateRegistry<TEvent>();
            int activeSubscription = registry.Subscriptions.Count(s => s.IsSubscribed);

            return (registry.Sources.Count, activeSubscription);
        }
    }

#endif

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (object registryObj in _registries.Values)
            {
                if (registryObj is IDisposable disposable)
                    disposable.Dispose();
            }

            _registries.Clear();
        }
    }

    private EventTypeRegistry<TEvent> GetOrCreateRegistry<TEvent>()
        where TEvent : IEvent
    {
        Type eventType = typeof(TEvent);

        lock (_lock)
        {
            if (_registries.TryGetValue(eventType, out object registry) == false)
            {
                registry = new EventTypeRegistry<TEvent>();
                _registries[eventType] = registry;
            }

            return (EventTypeRegistry<TEvent>)registry;
        }
    }

    private void ThrowIfDisposed()
    {
        ThrowIf.Disposed(_disposed, nameof(IntelligentEventAggregator));
    }
}