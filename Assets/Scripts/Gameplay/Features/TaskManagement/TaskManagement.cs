using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManagement<T> : IDisposable, IOnFoundedUniqueItemHandler<T>, IOwnerEventSource<UpdatedCountInfoEvent>
    where T : Component, IDeliverable, IIntractable
{
    private readonly ICoroutineRunner _coroutineRunner;
    private readonly IEventAggregator _eventAggregator;

    private readonly TaskQueue<T> _taskQueue;
    private readonly TransportationTaskAssigner<T> _taskAssigner;

    private readonly IThreeParametersFactory<DeliveryContext<T>, T, IItemSource<T>, IItemDestination<T>> _contextFactory;
    private readonly ISingleParameterFactory<TransportationTask<T>, DeliveryContext<T>> _taskFactory;

    private readonly ItemSource<T> _itemSource;
    private readonly IItemDestination<T> _itemDestination;
    private readonly TaskManagementInfo _taskManagementInfo;

    private TaskEventManagement<T> _taskEventManagement;
    private TaskCoroutineManagament<T> _taskCoroutineManagement;
    private OwnerInfo _ownerInfo;

    private bool _disposed;

    public TaskManagement(
        TaskQueue<T> taskQueue,
        TransportationTaskAssigner<T> taskAssigner,
        IItemDestination<T> itemDestination,
        ICoroutineRunner coroutineRunner,
        IEventAggregator eventAggregator)
    {
        ThrowIf.Null(taskQueue, nameof(taskQueue));
        ThrowIf.Null(taskAssigner, nameof(taskAssigner));
        ThrowIf.Null(itemDestination, nameof(itemDestination));
        ThrowIf.Null(coroutineRunner, nameof(coroutineRunner));

        _taskQueue = taskQueue;
        _taskAssigner = taskAssigner;
        _itemDestination = itemDestination;
        _coroutineRunner = coroutineRunner;
        _eventAggregator = eventAggregator;
        _itemSource = new();
        _taskManagementInfo = new();

        _contextFactory = new ThreeParametersFactory<DeliveryContext<T>, T, IItemSource<T>, IItemDestination<T>>(
            (i, s, d) => new(i, s, d));
        _taskFactory = new SingleParameterFactory<TransportationTask<T>, DeliveryContext<T>>(
            c => new(c));
    }

    private Action<UpdatedCountInfoEvent> _updatedCountInfo;

    event Action<UpdatedCountInfoEvent> IEventSource<UpdatedCountInfoEvent>.EventRaised
    {
        add => _updatedCountInfo += value;
        remove => _updatedCountInfo -= value;
    }

    int IOwnerEventSource<UpdatedCountInfoEvent>.OwnerInstanceId => _ownerInfo.InstanceId;

    bool IEventSource<UpdatedCountInfoEvent>.IsActive => Enabled;
    public bool Enabled { get; private set; }
    public int CountFreeWorkers => _taskAssigner.CountFreeWorkers;
    public int TotalCountWorkers => CountFreeWorkers + _taskAssigner.CountBusyWorkers;

    public void Initialize(
        Func<Vector3> getWaitPosition,
        float assignInterval,
        IEnumerable<ISubscribeProvider> subscriptions,
        OwnerInfo ownerInfo)
    {
        ThrowIf.Null(getWaitPosition, nameof(getWaitPosition));
        ThrowIf.Null(subscriptions, nameof(subscriptions));

        _ownerInfo = ownerInfo;

        TaskManagementContext<T> taskManagementContext = new(
            _coroutineRunner, _eventAggregator,
            _ownerInfo, _taskQueue,
            _taskAssigner, _taskManagementInfo,
            c => _updatedCountInfo?.Invoke(c), assignInterval);
        _taskEventManagement = new TaskEventManagement<T>(taskManagementContext);
        _taskCoroutineManagement = new TaskCoroutineManagament<T>(taskManagementContext);

        _taskEventManagement.Initialize(getWaitPosition, subscriptions);
        InitializeIfEnabled();
    }

    public void Enable()
    {
        if (Enabled)
            return;

        Enabled = true;

        _taskCoroutineManagement?.Enable();
        _taskEventManagement?.Enable();
        _eventAggregator.RegisterSource(this);
    }

    public void Disable()
    {
        if (Enabled == false)
            return;

        Enabled = false;

        _taskCoroutineManagement?.Disable();
        _taskEventManagement?.Disable();
        _eventAggregator.UnregisterSource(this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _taskCoroutineManagement?.Dispose();
        _taskEventManagement?.Dispose();
        _eventAggregator.UnregisterSource(this);
    }

    public IEnumerator TakeFreeWorkerRoutine(Action<ITransportationWorker<T>> action)
    {
        ThrowIf.Null(action, nameof(action));

        yield return _taskAssigner.TakeFreeWorker(action);
    }

    public void OnFoundedUniqueItem(T item)
    {
        DeliveryContext<T> context = _contextFactory.Create(item, _itemSource, _itemDestination);
        TransportationTask<T> task = _taskFactory.Create(context);
        float priority = (item.transform.position - _ownerInfo.Position).sqrMagnitude;

        _taskQueue.Enqueue(task, priority);

        _taskEventManagement.SubscribeForTask(task, (t, w) =>
        {
            _taskManagementInfo.MoveToCompleted();
            t.Dispose();
            _taskEventManagement.UnsubscribeForTask(task);
        });
    }

    private void InitializeIfEnabled()
    {
        if (Enabled == false)
            return;

        _taskEventManagement.Enable();
        _taskCoroutineManagement.Enable();
    }
}