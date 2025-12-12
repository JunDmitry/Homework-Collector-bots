using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskEventManagement<T> : IDisposable
    where T : Component, IDeliverable, IIntractable
{
    private readonly Dictionary<TransportationTask<T>, ISubscribeProvider> _subscriptionByTask;
    private readonly List<ISubscribeProvider> _subscriptions;

    private readonly OwnerInfo _ownerInfo;
    private readonly IEventAggregator _eventAggregator;
    private readonly TransportationTaskAssigner<T> _taskAssigner;
    private readonly ICoroutineRunner _coroutineRunner;

    private IEventSubscription _workerCreatedSubscription;

    public TaskEventManagement(TaskManagementContext<T> taskManagementContext)
    {
        _subscriptionByTask = new();
        _subscriptions = new();
        _ownerInfo = taskManagementContext.OwnerInfo;
        _eventAggregator = taskManagementContext.EventAggregator;
        _taskAssigner = taskManagementContext.TaskAssigner;
        _coroutineRunner = taskManagementContext.CoroutineRunner;
    }

    public bool Enabled { get; private set; }

    public void Dispose()
    {
        _subscriptions.ForEach(s => s.Dispose());
        _subscriptionByTask.ForEach(pair => pair.Value?.Dispose());
        _workerCreatedSubscription?.Dispose();
        _workerCreatedSubscription = null;
    }

    public void Enable()
    {
        if (Enabled)
            return;

        Enabled = true;
        _subscriptions.ForEach(s => s.Subscribe());
        _workerCreatedSubscription ??= SubscribeUnitCreated<UnitCreatedEvent<ITransportationWorker<T>>>(OnUnitCreated);
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
        _workerCreatedSubscription?.Unsubscribe();
        _workerCreatedSubscription = null;
    }

    public void Initialize(Func<Vector3> getWaitPosition, IEnumerable<ISubscribeProvider> additionalSubscriptions)
    {
        ThrowIf.Null(getWaitPosition, nameof(getWaitPosition));
        ThrowIf.Null(additionalSubscriptions, nameof(additionalSubscriptions));

        _subscriptions.AddRange(additionalSubscriptions);

        ISubscribeProvider workerProvider = SubscribeProvider<TransportationTaskAssigner<T>, Action<ITransportationWorker<T>>>.Create(
            _taskAssigner,
             w => _coroutineRunner.StartCoroutine(w.MoveTo(getWaitPosition())),
            (s, h) => s.BecameFreeWorker += h,
            (s, h) => s.BecameFreeWorker -= h);

        _subscriptions.Add(workerProvider);
        _workerCreatedSubscription = SubscribeUnitCreated<UnitCreatedEvent<ITransportationWorker<T>>>(OnUnitCreated);
    }

    public void SubscribeForTask(TransportationTask<T> task, Action<TransportationTask<T>, ITransportationWorker<T>> onComplete)
    {
        ISubscribeProvider provider = SubscribeProvider<TransportationTask<T>, Action<TransportationTask<T>, ITransportationWorker<T>>>.Create(
            task,
            onComplete,
            (s, h) => s.Completed += h,
            (s, h) => s.Completed -= h);
        _subscriptionByTask[task] = provider;

        provider.Subscribe();
    }

    public void UnsubscribeForTask(TransportationTask<T> task)
    {
        if (_subscriptionByTask.TryGetValue(task, out ISubscribeProvider provider))
        {
            provider.Dispose();
            _subscriptionByTask.Remove(task);
        }
    }

    private IEventSubscription SubscribeUnitCreated<TEvent>(Action<TEvent> handler, IEventFilter<TEvent> eventFilter = null)
        where TEvent : class, IEvent
    {
        ISubscribeCondition<TEvent> condition = SequenceCondition<TEvent>.Create(
                new SourceTypeCondition<TEvent, IOwnerEventSource<TEvent>>())
            .Append(new SourcePropertyCondition<TEvent, int>(
                s => ((IOwnerEventSource<TEvent>)s).OwnerInstanceId,
                id => id == _ownerInfo.InstanceId),
                ConditionOperationType.And);

        return _eventAggregator.Subscribe(condition, handler, eventFilter);
    }

    private void OnUnitCreated(UnitCreatedEvent<ITransportationWorker<T>> unitCreatedEvent)
    {
        ThrowIf.Null(unitCreatedEvent, nameof(unitCreatedEvent));

        _taskAssigner.AddWorker(unitCreatedEvent.CreatedUnit);
    }
}