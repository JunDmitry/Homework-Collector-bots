using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage<T> : IDisposable, IItemDestination<T>, IOwnerEventSource<ChangedResourceCountEvent>
    where T : IDeliverable
{
    private const int PositiveCount = 1;

    private readonly Dictionary<ItemDeliverableType, int> _resourceMap;
    private readonly OwnerInfo _ownerInfo;
    private readonly IEventAggregator _eventAggregator;

    private Func<Vector3> _getPosition;
    private float _minDistanceToArrive;

    public Storage(OwnerInfo ownerInfo, IEventAggregator eventAggregator)
    {
        _resourceMap = Utils.GenerateEmptyDeliverableMap();
        _ownerInfo = ownerInfo;
        _eventAggregator = eventAggregator;
        _eventAggregator.RegisterSource(this);
    }

    public Storage(OwnerInfo ownerInfo, IEventAggregator eventAggregator, IEnumerable<KeyValuePair<ItemDeliverableType, int>> resources)
        : this(ownerInfo, eventAggregator)
    {
        resources.ForEach(pair => AddResource(pair.Key, pair.Value));
    }

    private event Action<ChangedResourceCountEvent> _changedResourceCount;

    event Action<ChangedResourceCountEvent> IEventSource<ChangedResourceCountEvent>.EventRaised
    {
        add => _changedResourceCount += value;
        remove => _changedResourceCount -= value;
    }

    int IOwnerEventSource<ChangedResourceCountEvent>.OwnerInstanceId => _ownerInfo.InstanceId;

    bool IEventSource<ChangedResourceCountEvent>.IsActive => true;

    public void Dispose()
    {
        _eventAggregator.UnregisterSource(this);
    }

    public DeliveryOperation<T> PrepareReceival(T item)
    {
        return new(_getPosition(), _minDistanceToArrive, ExecuteShipment(item));
    }

    public void Initialize(Func<Vector3> getPosition, float distanceToArrive = .1f)
    {
        ThrowIf.Null(getPosition, nameof(getPosition));

        _getPosition = getPosition;
        _minDistanceToArrive = distanceToArrive;
    }

    public void AddResource(T resource)
    {
        AddResource(resource.ItemDeliverableType, resource.Count);
    }

    public void AddResource(ItemDeliverableType type, int count)
    {
        _resourceMap[type] += count;

        _changedResourceCount?.Invoke(new(type, _resourceMap[type]));
    }

    public bool TryTakeResourceStrict(ItemDeliverableType type, int countForTake)
    {
        if (ContainsResourceAtLeast(type, countForTake) == false)
            return false;

        TakeResource(type, countForTake);

        return true;
    }

    public bool TryTakeResourceAtMostPositive(ItemDeliverableType type, int maxCountForTake, out int takeCount)
    {
        ThrowIfInvalidArguments(type, maxCountForTake);

        takeCount = 0;

        if (_resourceMap.TryGetValue(type, out int totalCount) == false
            || totalCount == 0)
            return false;

        takeCount = Math.Min(totalCount, maxCountForTake);
        TakeResource(type, takeCount);

        return true;
    }

    public bool ContainsResourcePositive(ItemDeliverableType type)
    {
        return ContainsResourceAtLeast(type, PositiveCount);
    }

    public bool ContainsResourceAtLeast(ItemDeliverableType type, int minCount)
    {
        ThrowIfInvalidArguments(type, minCount);

        if (_resourceMap.TryGetValue(type, out int totalCount) == false
            || totalCount < minCount)
            return false;

        return true;
    }

    public bool ContainsResourceType(ItemDeliverableType type)
    {
        ThrowIfUnknownType(type);

        return _resourceMap.ContainsKey(type);
    }

    private IEnumerator ExecuteShipment(T item)
    {
        AddResource(item);

        yield return null;
    }

    private void TakeResource(ItemDeliverableType type, int count)
    {
        _resourceMap[type] -= count;
        _changedResourceCount?.Invoke(new(type, _resourceMap[type]));
    }

    private void ThrowIfInvalidArguments(ItemDeliverableType type, int countForTake)
    {
        ThrowIfUnknownType(type);
        ThrowIfNotPositive(countForTake);
    }

    private void ThrowIfUnknownType(ItemDeliverableType type)
    {
        ThrowIf.Argument(type == ItemDeliverableType.Unknown,
            $"{nameof(ItemDeliverableType)} can't be {nameof(ItemDeliverableType.Unknown)} {nameof(type)}",
            nameof(type));
    }

    private static void ThrowIfNotPositive(int countForTake)
    {
        ThrowIf.Argument(countForTake <= 0,
            $"{nameof(T)} {nameof(countForTake)} must be positive",
            nameof(countForTake));
    }
}