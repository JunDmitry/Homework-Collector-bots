using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObservableList<T> : IDisposable
    where T : class
{
    private static readonly TimeSpan s_defaultInterval = TimeSpan.FromSeconds(2.5);

    private readonly Func<T, ISubscribeProvider> _getSubscribeProvider;
    private readonly Dictionary<WeakReference<T>, ISubscribeProvider> _subscribes;

    private bool _disposed;

    public ObservableList(Func<T, ISubscribeProvider> getSubscribeProvider, ICoroutineRunner coroutineRunner, TimeSpan? cleanupInterval = null)
    {
        ThrowIf.Null(getSubscribeProvider, nameof(getSubscribeProvider));

        _getSubscribeProvider = getSubscribeProvider;
        _subscribes = new(new WeakReferenceComparer<T>());

        cleanupInterval ??= s_defaultInterval;
        coroutineRunner?.StartCoroutine(CleanupRoutine(cleanupInterval.Value));
    }

    public void Subscribe(IEnumerable<T> observers)
    {
        ThrowIfDisposed();

        observers.ForEach(observer => Subscribe(observer));
    }

    public void Subscribe(T observer)
    {
        ThrowIfDisposed();

        WeakReference<T> weakRef = new(observer);

        if (_subscribes.TryGetValue(weakRef, out ISubscribeProvider provider) == false)
        {
            provider = _getSubscribeProvider.Invoke(observer);

            if (provider == null)
                return;

            _subscribes[weakRef] = provider;
        }

        if (provider.IsSubscribed == false)
            provider?.Subscribe();
    }

    public void Unsubscribe()
    {
        ThrowIfDisposed();

        _subscribes.Keys.ForEach(s => Unsubscribe(s));
    }

    public void Unsubscribe(IEnumerable<T> observers)
    {
        ThrowIfDisposed();

        observers.ForEach(observer => Unsubscribe(observer));
    }

    public void Unsubscribe(T observer)
    {
        ThrowIfDisposed();

        Unsubscribe(new WeakReference<T>(observer));
    }

    public void Clear()
    {
        Unsubscribe();
        _subscribes.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _subscribes.Values.ForEach(s => s.Dispose());
    }

    private IEnumerator CleanupRoutine(TimeSpan interval)
    {
        WaitForSeconds wait = new((float)interval.TotalSeconds);

        while (_disposed == false)
        {
            yield return wait;

            if (_disposed == false)
                CleanupDeadReference();
        }
    }

    private void CleanupDeadReference()
    {
        lock (_subscribes)
        {
            List<WeakReference<T>> deadKeys = _subscribes.Where(k => k.Key.TryGetTarget(out _) == false)
                .Select(k => k.Key)
                .ToList();

            deadKeys.ForEach(key =>
            {
                _subscribes[key]?.Dispose();
                _subscribes.Remove(key);
            });
        }
    }

    private void Unsubscribe(WeakReference<T> weakReference)
    {
        if (_subscribes.TryGetValue(weakReference, out ISubscribeProvider provider) == false)
            return;

        if (provider.IsSubscribed)
            provider.Unsubscribe();
    }

    private void ThrowIfDisposed()
    {
        ThrowIf.Disposed(_disposed, nameof(ObservableList<T>));
    }
}