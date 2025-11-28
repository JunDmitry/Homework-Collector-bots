using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManagement<T> : IDisposable, IOnFoundedUniqueItemHandler<T>, IOnCreatedWorker<T>
    where T : Component, IDeliverable, IIntractable
{
    private readonly ICoroutineRunner _coroutineRunner;

    private readonly TaskQueue<T> _taskQueue;
    private readonly TransportationTaskAssigner<T> _taskAssigner;
    private readonly Dictionary<TransportationTask<T>, ISubscribeProvider> _subscriptionAtTask;
    private readonly List<ISubscribeProvider> _subscriptions;

    private readonly IThreeParametersFactory<DeliveryContext<T>, T, IItemSource<T>, IItemDestination<T>> _contextFactory;
    private readonly ISingleParameterFactory<TransportationTask<T>, DeliveryContext<T>> _taskFactory;

    private readonly ItemSource<T> _itemSource;
    private readonly IItemDestination<T> _itemDestination;
    private readonly TaskManagementInfo _taskManagementInfo;

    private Func<Vector3> _getWaitPosition;
    private WaitForSeconds _waitAssignInterval;
    private Vector3 _ownerPosition;

    private bool _disposed;
    private Coroutine _assignTaskRoutine = null;

    public TaskManagement(
        TaskQueue<T> taskQueue,
        TransportationTaskAssigner<T> taskAssigner,
        IItemDestination<T> itemDestination,
        ICoroutineRunner coroutineRunner)
    {
        ThrowIf.Null(taskQueue, nameof(taskQueue));
        ThrowIf.Null(taskAssigner, nameof(taskAssigner));
        ThrowIf.Null(itemDestination, nameof(itemDestination));
        ThrowIf.Null(coroutineRunner, nameof(coroutineRunner));

        _taskQueue = taskQueue;
        _taskAssigner = taskAssigner;
        _itemDestination = itemDestination;
        _coroutineRunner = coroutineRunner;

        _subscriptionAtTask = new();
        _itemSource = new();
        _subscriptions = new();
        _taskManagementInfo = new();

        _contextFactory = new ThreeParametersFactory<DeliveryContext<T>, T, IItemSource<T>, IItemDestination<T>>(
            (i, s, d) => new(i, s, d));
        _taskFactory = new SingleParameterFactory<TransportationTask<T>, DeliveryContext<T>>(
            c => new(c));
    }

    public event Action<IReadonlyTaskManagementInfo> UpdatedCountInfo;

    public bool Enabled { get; private set; }

    public void Initialize(
        Func<Vector3> getWaitPosition,
        float assignInterval,
        IEnumerable<ISubscribeProvider> subscriptions,
        Vector3 ownerPosition)
    {
        ThrowIf.Null(getWaitPosition, nameof(getWaitPosition));
        ThrowIf.Null(subscriptions, nameof(subscriptions));

        _getWaitPosition = getWaitPosition;
        _waitAssignInterval = new(assignInterval);
        _subscriptions.AddRange(subscriptions);
        _ownerPosition = ownerPosition;

        InitializeSubscriptions();
        InitializeIfEnabled();
    }

    public void Enable()
    {
        if (Enabled)
            return;

        Enabled = true;
        _subscriptions.ForEach(s => s.Subscribe());
        _assignTaskRoutine = _coroutineRunner.StartCoroutine(AssignTaskRoutine());
    }

    public void Disable()
    {
        if (Enabled == false)
            return;

        Enabled = false;
        _subscriptions.ForEach(s =>
        {
            if (s.IsSubscribed)
                s.Unsubscribe();
        });
        _coroutineRunner?.StopCoroutine(_assignTaskRoutine);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _subscriptions.ForEach(s => s.Dispose());
        _subscriptionAtTask.ForEach(pair => pair.Value.Dispose());

        if (_assignTaskRoutine != null)
            _coroutineRunner?.StopCoroutine(_assignTaskRoutine);
    }

    public void OnFoundedUniqueItem(T item)
    {
        DeliveryContext<T> context = _contextFactory.Create(item, _itemSource, _itemDestination);
        TransportationTask<T> task = _taskFactory.Create(context);
        float priority = (item.transform.position - _ownerPosition).sqrMagnitude;

        _taskQueue.Enqueue(task, priority);
        _taskManagementInfo.AddReadyToAssign();
        UpdatedCountInfo?.Invoke(_taskManagementInfo);

        ISubscribeProvider provider = SubscribeProvider<TransportationTask<T>, Action<TransportationTask<T>, ITransportationWorker<T>>>.Create(
            task,
            (t, w) =>
            {
                _taskManagementInfo.MoveToCompleted();
                UpdatedCountInfo?.Invoke(_taskManagementInfo);

                t.Dispose();
                _subscriptionAtTask[t]?.Dispose();
                _subscriptionAtTask.Remove(t);
            },
            (s, h) => s.Completed += h,
            (s, h) => s.Completed -= h);
        _subscriptionAtTask[task] = provider;

        provider.Subscribe();
    }

    public void OnCreatedWorker(ITransportationWorker<T> worker)
    {
        ThrowIf.Null(worker, nameof(worker));

        _taskAssigner.AddWorker(worker);
    }

    private void InitializeIfEnabled()
    {
        if (Enabled == false)
            return;

        _subscriptions.ForEach(s => s.Subscribe());
        _assignTaskRoutine = _coroutineRunner?.StartCoroutine(AssignTaskRoutine());
    }

    private IEnumerator AssignTaskRoutine()
    {
        TransportationTask<T> task;

        while (Enabled)
        {
            while (_taskQueue.ReadyToAssignCount > 0 && _taskAssigner.CanAssignTask())
            {
                task = _taskQueue.Dequeue();
                _taskAssigner.AssignTask(task);

                _taskManagementInfo.MoveToAssigned();
                UpdatedCountInfo?.Invoke(_taskManagementInfo);
            }

            yield return _waitAssignInterval;
        }
    }

    private void InitializeSubscriptions()
    {
        ISubscribeProvider workerProvider = SubscribeProvider<TransportationTaskAssigner<T>, Action<ITransportationWorker<T>>>.Create(
            _taskAssigner,
             w => _coroutineRunner.StartCoroutine(w.MoveTo(_getWaitPosition())),
            (s, h) => s.BecameFreeWorker += h,
            (s, h) => s.BecameFreeWorker -= h);

        _subscriptions.Add(workerProvider);
    }
}