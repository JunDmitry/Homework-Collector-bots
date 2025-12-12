using System;
using System.Collections.Generic;
using UnityEngine;

public class UniqueItemCollection<T> : IDisposable
    where T : Component
{
    private readonly HashSet<T> _items;
    private readonly ISubscribeProvider _scannerSubscription;
    private readonly IntervalScanner<T> _intervalScanner;
    private readonly IEventAggregator _eventAggregator;
    private readonly List<IEventSubscription> _subscriptions;

    private bool _enabled;
    private bool _disposed;

    public UniqueItemCollection(IntervalScanner<T> intervalScanner, IEventAggregator eventAggregator)
    {
        ThrowIf.Null(intervalScanner, nameof(intervalScanner));

        _items = new();
        _scannerSubscription = SubscribeProvider<IntervalScanner<T>, Action<T>>.Create(
            intervalScanner,
            OnFoundedItem,
            (s, h) => s.FoundedItem += h,
            (s, h) => s.FoundedItem -= h);
        _intervalScanner = intervalScanner;
        _eventAggregator = eventAggregator;
        _subscriptions = new();
    }

    public event Action<T> FoundedUniqueItem;

    public bool Enabled => _enabled && _disposed == false;

    public void Enable()
    {
        if (Enabled)
            return;

        _enabled = true;

        SubscribeAll();
        _scannerSubscription.Subscribe();
        _intervalScanner.BeginScan();
    }

    public void Disable()
    {
        if (Enabled == false)
            return;

        _enabled = false;
        UnsubscribeAll();
        _scannerSubscription.Unsubscribe();
        _intervalScanner.StopScan();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _subscriptions.ForEach(s => s.Dispose());
        _scannerSubscription?.Dispose();
        _intervalScanner.StopScan();
    }

    private void OnFoundedItem(T item)
    {
        if (item == null || _items.Contains(item))
            return;

        _items.Add(item);
        FoundedUniqueItem?.Invoke(item);
    }

    private void SubscribeAll()
    {
        _subscriptions.Add(_eventAggregator.Subscribe(new AlwaysTrueCondition<CollectedEvent<T>>(), OnCollectedItem));
    }

    private void UnsubscribeAll()
    {
        _subscriptions.ForEach(s => s.Unsubscribe());
    }

    private void OnCollectedItem(CollectedEvent<T> collectedEvent)
    {
        T item = collectedEvent.CollectedItem;

        _items.Remove(item);
    }
}