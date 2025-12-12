using Assets.Scripts.Common.Utilities;
using System;
using System.Linq;

public class TaskQueue<T> : IDisposable
    where T : IDeliverable
{
    private const float DefaultPriority = 1000;

    private readonly PriorityQueue<TransportationTask<T>, float> _tasks;
    private readonly ISingleParameterFactory<TransportationTask<T>, DeliveryContext<T>> _factory;
    private IEventSubscription _subscription;

    public TaskQueue(ISingleParameterFactory<TransportationTask<T>, DeliveryContext<T>> factory, IEventAggregator eventAggregator)
    {
        ThrowIf.Null(factory, nameof(factory));

        _tasks = new((a, b) => -a.CompareTo(b));
        _factory = factory;
        Subscribe(eventAggregator);
    }

    public int ReadyToAssignCount => _tasks.Count;

    public void Dispose()
    {
        _subscription?.Dispose();
    }

    public void Enqueue(DeliveryContext<T> context, float priority = DefaultPriority)
    {
        TransportationTask<T> transportationTask = _factory.Create(context);

        Enqueue(transportationTask, priority);
    }

    public void Enqueue(TransportationTask<T> transportationTask, float priority = DefaultPriority)
    {
        ThrowIf.Null(transportationTask, nameof(transportationTask));

        _tasks.Enqueue(transportationTask, priority);
    }

    public TransportationTask<T> Dequeue()
    {
        ThrowIf.Invalid(_tasks.Count == 0, $"{nameof(ReadyToAssignCount)} {nameof(TransportationTask<T>)} must be positive");

        TransportationTask<T> task = _tasks.DequeueFirst();

        return task;
    }

    public bool TryDequeue(out TransportationTask<T> transportationTask)
    {
        transportationTask = default;

        if (_tasks.Count == 0)
            return false;

        transportationTask = _tasks.DequeueFirst();

        return true;
    }

    private void Subscribe(IEventAggregator eventAggregator)
    {
        _subscription = eventAggregator.Subscribe(
            new AlwaysTrueCondition<CollectedEvent<T>>(), OnCollected);
    }

    private void OnCollected(CollectedEvent<T> collectedEvent)
    {
        T item = collectedEvent.CollectedItem;

        TransportationTask<T> taskToRemove = _tasks.FirstOrDefault(t => t.Target.Equals(item));

        if (taskToRemove != null)
            _tasks.Remove(taskToRemove);
    }
}