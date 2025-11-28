using System;

public class SubscribeProvider<TObj, TDelegate> : ISubscribeProvider
    where TDelegate : Delegate
{
    private static readonly string s_defaultEventName = typeof(TDelegate).Name + " event";
    private static readonly Type s_delegateType = typeof(TDelegate);

    private readonly TObj _source;
    private readonly TDelegate _subscribeEvent;
    private readonly Action<TObj, TDelegate> _subscribe;
    private readonly Action<TObj, TDelegate> _unsubscribe;

    private readonly object _lock = new();
    private bool _isSubscribed = false;
    private bool _disposed = false;

    public SubscribeProvider(
        TObj source,
        TDelegate subscribeEvent,
        Action<TObj, TDelegate> subscribe,
        Action<TObj, TDelegate> unsubscribe,
        string eventName = null)
    {
        ThrowIf.Null(source, nameof(source));
        ThrowIf.Null(subscribeEvent, nameof(subscribeEvent));
        ThrowIf.Null(subscribe, nameof(subscribe));
        ThrowIf.Null(unsubscribe, nameof(unsubscribe));

        _source = source;
        _subscribeEvent = subscribeEvent;
        _subscribe = subscribe;
        _unsubscribe = unsubscribe;
        EventName = eventName ?? s_defaultEventName;
    }

    public static SubscribeProvider<TObj, TDelegate> Create(
        TObj source,
        TDelegate subscribeEvent,
        Action<TObj, TDelegate> subscribe,
        Action<TObj, TDelegate> unsubscribe)
    {
        return new(source, subscribeEvent, subscribe, unsubscribe);
    }

    public bool IsSubscribed => _isSubscribed && _disposed == false;
    public string EventName { get; }
    public Type DelegateType => s_delegateType;

    public void Subscribe()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (_isSubscribed)
                return;

            _subscribe.Invoke(_source, _subscribeEvent);
            _isSubscribed = true;
        }
    }

    public void Unsubscribe()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (_isSubscribed == false)
                return;

            _unsubscribe.Invoke(_source, _subscribeEvent);
            _isSubscribed = false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Unsubscribe();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ThrowIf.Disposed(_disposed, nameof(SubscribeProvider<TObj, TDelegate>));
    }
}