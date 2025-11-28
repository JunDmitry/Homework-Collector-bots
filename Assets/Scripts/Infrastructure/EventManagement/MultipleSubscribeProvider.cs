using System;
using System.Collections.Generic;
using System.Linq;

public class MultipleSubscribeProvider : ISubscribeProvider
{
    private readonly List<ISubscribeProvider> _subscribeProviders;

    private readonly object _lock = new();
    private bool _subscribed;
    private bool _disposed;

    public MultipleSubscribeProvider(IEnumerable<ISubscribeProvider> subscribeProviders)
    {
        ThrowIf.Null(subscribeProviders, nameof(subscribeProviders));
        ThrowIf.Argument(
            subscribeProviders.Any(s => s == null),
            $"{subscribeProviders} collection contains null elements",
            nameof(subscribeProviders));

        _subscribeProviders = subscribeProviders.ToList();
    }

    public bool IsSubscribed => _subscribed && _disposed == false;

    public static ISubscribeProvider Combine(params ISubscribeProvider[] subscribeProviders)
    {
        return Combine((IEnumerable<ISubscribeProvider>)subscribeProviders);
    }

    public static ISubscribeProvider Combine(IEnumerable<ISubscribeProvider> subscribeProviders)
    {
        return new MultipleSubscribeProvider(subscribeProviders);
    }

    public void Subscribe()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (IsSubscribed)
                return;

            List<Exception> exceptions = null;

            _subscribeProviders.ForEach(provider =>
            {
                exceptions = MakeActionWithCatch(provider.Subscribe, exceptions);
            });

            _subscribed = true;

            ThrowIfWasException(exceptions, nameof(Subscribe));
        }
    }

    public void Unsubscribe()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (IsSubscribed == false)
                return;

            List<Exception> exceptions = null;

            _subscribeProviders.ForEach(provider =>
            {
                exceptions = MakeActionWithCatch(provider.Unsubscribe, exceptions);
            });

            _subscribed = false;

            ThrowIfWasException(exceptions, nameof(Unsubscribe));
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Unsubscribe();
        _disposed = true;
    }

    private static List<Exception> MakeActionWithCatch(Action action, List<Exception> exceptions)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception exception)
        {
            exceptions ??= new List<Exception>();
            exceptions.Add(exception);
        }

        return exceptions;
    }

    private void ThrowIfWasException(List<Exception> exceptions, string actionName)
    {
        if (exceptions != null)
            throw new AggregateException($"Failed to {actionName} one or more providers", exceptions);
    }

    private void ThrowIfDisposed()
    {
        ThrowIf.Disposed(_disposed, nameof(MultipleSubscribeProvider));
    }
}