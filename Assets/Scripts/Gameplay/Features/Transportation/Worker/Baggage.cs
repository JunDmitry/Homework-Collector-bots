using System;

public class Baggage<T> : IDisposable
    where T : IDeliverable
{
    private readonly ISubscribeProvider _subscription;

    private T _carriedItem;

    private bool _enabled = false;
    private bool _isEmpty = true;
    private bool _disposed = false;

    public Baggage(TransportationWorkerEventHandler handler, Action<T> onPut, Action<T> onTake)
    {
        ThrowIf.Null(onPut, nameof(onPut));
        ThrowIf.Null(onTake, nameof(onTake));

        _subscription = CreateProvider(handler, onPut, onTake);
    }

    public bool Enabled => _enabled;
    public bool IsEmpty => _isEmpty && _disposed == false;

    public void Enable()
    {
        if (_enabled)
            return;

        _enabled = true;
        _subscription.Subscribe();
    }

    public void Disable()
    {
        if (_enabled == false)
            return;

        _enabled = false;
        _subscription.Unsubscribe();
    }

    public void Put(T carriedItem)
    {
        ThrowIfDisposed();

        ThrowIf.Invalid(IsEmpty == false, $"{nameof(Baggage<T>)} is not empty");
        ThrowIf.Null(carriedItem, nameof(carriedItem));

        _carriedItem = carriedItem;
        _isEmpty = false;
    }

    public T Take()
    {
        ThrowIfDisposed();

        ThrowIf.Invalid(IsEmpty, $"{nameof(Baggage<T>)} is empty");

        T carriedItem = _carriedItem;
        _carriedItem = default;
        _isEmpty = true;

        return carriedItem;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _subscription?.Dispose();
    }

    private void ThrowIfDisposed()
    {
        ThrowIf.Disposed(_disposed, nameof(Baggage<T>));
    }

    private ISubscribeProvider CreateProvider(TransportationWorkerEventHandler handler, Action<T> onPut, Action<T> onTake)
    {
        return MultipleSubscribeProvider.Combine(
            SubscribeProvider<TransportationWorkerEventHandler, Action>.Create(
                handler,
                () =>
                {
                    onPut.Invoke(_carriedItem);
                },
                (s, h) => s.Grabbing += h,
                (s, h) => s.Grabbing -= h),
            SubscribeProvider<TransportationWorkerEventHandler, Action>.Create(
                handler,
                () =>
                {
                    onTake.Invoke(_carriedItem);
                },
                (s, h) => s.Bringing += h,
                (s, h) => s.Bringing -= h));
    }
}