using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseStructure : Structure, ICoroutineRunner
{
    [SerializeField] private Area _waitArea;
    [SerializeField] private Area _shipmentArea;
    [SerializeField] private int _initialWorkerCount;
    [SerializeField] private float _minShipmentDistance;
    [SerializeField] private float _taskAssignInterval;

    private TaskManagement<Resource> _taskManagement;
    private UniqueItemCollection<Resource> _uniqueItemCollection;
    private Storage<Resource> _resourceStorage;

    public event Action<ITransportationWorker<Resource>> CreatedWorker;

    public Vector3 WaitArea => _waitArea.RandomPosition() + new Vector3(transform.position.x, 0f, transform.position.z);
    public Vector3 ShipmentArea => _shipmentArea.RandomPosition() + new Vector3(transform.position.x, 0f, transform.position.z);
    public bool EnabledLogics { get; private set; }

    private void OnDestroy()
    {
        _uniqueItemCollection?.Dispose();
        _taskManagement?.Dispose();
    }

    public void EnableLogics()
    {
        if (EnabledLogics)
            return;

        EnabledLogics = true;
        _uniqueItemCollection.Enable();
        _taskManagement.Enable();
    }

    public void DisableLogics()
    {
        if (EnabledLogics == false)
            return;

        _uniqueItemCollection.Disable();
        _taskManagement.Disable();
    }

    public void Initialize(
        IntervalScanner<Resource> scanner,
        Storage<Resource> storage,
        IEmptyFactory<ITransportationWorker<Resource>> workerFactory,
        TaskManagement<Resource> taskManagement)
    {
        _resourceStorage = storage;
        _taskManagement = taskManagement;
        _uniqueItemCollection = new(scanner);

        _shipmentArea.InitializeIfNeed();
        _waitArea.InitializeIfNeed();
        _resourceStorage.Initialize(() => ShipmentArea, _minShipmentDistance);
        _taskManagement.Initialize(
            () => WaitArea,
            _taskAssignInterval,
            CreateSubscriptionsForTaskManagement(),
            transform.position);
        _taskManagement.Enable();

        InitializeResourceWorkers(workerFactory);
    }

    private void InitializeResourceWorkers(IEmptyFactory<ITransportationWorker<Resource>> workerFactory)
    {
        workerFactory.CreateCount(_initialWorkerCount)
            .ForEach(w => CreatedWorker?.Invoke(w));
    }

    private IEnumerable<ISubscribeProvider> CreateSubscriptionsForTaskManagement()
    {
        ISubscribeProvider uniqueSubscription = SubscribeProvider<UniqueItemCollection<Resource>, Action<Resource>>.Create(
            _uniqueItemCollection,
            _taskManagement.OnFoundedUniqueItem,
            (s, h) => s.FoundedUniqueItem += h,
            (s, h) => s.FoundedUniqueItem -= h);
        ISubscribeProvider workerFactorySubscription = new SubscribeProvider<BaseStructure, Action<ITransportationWorker<Resource>>>(
            this,
            _taskManagement.OnCreatedWorker,
            (s, h) => s.CreatedWorker += h,
            (s, h) => s.CreatedWorker -= h);

        yield return uniqueSubscription;
        yield return workerFactorySubscription;
    }

    private void OnDrawGizmos()
    {
        Vector3 offset = new(transform.position.x, 0f, transform.position.z);

        Gizmos.color = new Color(1, 1, 0, .5f);
        DrawArea(offset, _waitArea);

        Gizmos.color = new Color(0, 1, 0, .5f);
        DrawArea(offset, _shipmentArea);
    }

    private void DrawArea(Vector3 offset, Area area)
    {
        foreach (Rectangle rectangle in area)
            Gizmos.DrawCube(rectangle.Bounds.center + offset, rectangle.Bounds.size);
    }
}