using Assets.Scripts.Common.Utilities;

public class TaskQueue<T>
    where T : IDeliverable
{
    private const float DefaultPriority = 1000;

    private readonly PriorityQueue<TransportationTask<T>, float> _tasks;
    private readonly ISingleParameterFactory<TransportationTask<T>, DeliveryContext<T>> _factory;

    public TaskQueue(ISingleParameterFactory<TransportationTask<T>, DeliveryContext<T>> factory)
    {
        ThrowIf.Null(factory, nameof(factory));

        _tasks = new((a, b) => b.CompareTo(a));
        _factory = factory;
    }

    public int ReadyToAssignCount => _tasks.Count;

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
}