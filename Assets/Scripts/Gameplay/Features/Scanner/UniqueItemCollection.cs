using System;
using System.Collections.Generic;
using UnityEngine;

public class UniqueItemCollection<T> : IDisposable
    where T : Component, ICollectedEvent<T>
{
    private readonly Dictionary<T, ISubscribeProvider> _items;
    private readonly ISubscribeProvider _scannerSubscription;
    private readonly IntervalScanner<T> _intervalScanner;

    private bool _enabled;
    private bool _disposed;

    public UniqueItemCollection(IntervalScanner<T> intervalScanner)
    {
        ThrowIf.Null(intervalScanner, nameof(intervalScanner));

        _items = new();
        _scannerSubscription = SubscribeProvider<IntervalScanner<T>, Action<T>>.Create(
            intervalScanner,
            OnFoundedItem,
            (s, h) => s.FoundedItem += h,
            (s, h) => s.FoundedItem -= h);
        _intervalScanner = intervalScanner;
    }

    public event Action<T> FoundedUniqueItem;

    public bool Enabled => _enabled && _disposed == false;

    public void Enable()
    {
        if (Enabled)
            return;

        _enabled = true;
        _items.Values.ForEach(s => s.Subscribe());
        _scannerSubscription.Subscribe();
        _intervalScanner.BeginScan();
    }

    public void Disable()
    {
        if (Enabled == false)
            return;

        _enabled = false;
        _items.Values.ForEach(s => s.Unsubscribe());
        _scannerSubscription.Unsubscribe();
        _intervalScanner.StopScan();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _items.Values.ForEach(s => s.Dispose());
        _scannerSubscription?.Dispose();
        _intervalScanner.StopScan();
    }

    private void OnFoundedItem(T item)
    {
        if (item == null || _items.ContainsKey(item))
            return;

        ISubscribeProvider provider = SubscribeProvider<T, Action<T>>.Create(
            item,
            i =>
            {
                IDisposable disposable = _items[item];
                _items.Remove(item);
                disposable?.Dispose();
            },
            (s, h) => s.Collected += h,
            (s, h) => s.Collected -= h);
        _items[item] = provider;
        provider.Subscribe();

        FoundedUniqueItem?.Invoke(item);
    }
}