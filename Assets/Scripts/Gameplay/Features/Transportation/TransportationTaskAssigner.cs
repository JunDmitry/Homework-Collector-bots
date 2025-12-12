using System;
using System.Collections;
using System.Collections.Generic;

public class TransportationTaskAssigner<T> : IDisposable
    where T : IDeliverable
{
    private readonly Queue<ITransportationWorker<T>> _freeWorkers;
    private readonly HashSet<ITransportationWorker<T>> _busyWorkers;
    private readonly ICoroutineRunner _coroutineStarter;

    private bool _disposed = false;
    private int _requestWorkerCount = 0;

    public TransportationTaskAssigner(ICoroutineRunner coroutineStarter)
    {
        ThrowIf.Null(coroutineStarter, nameof(coroutineStarter));

        _freeWorkers = new();
        _busyWorkers = new();
        _coroutineStarter = coroutineStarter;
    }

    public TransportationTaskAssigner(IEnumerable<ITransportationWorker<T>> initialWorkers, ICoroutineRunner coroutineStarter)
        : this(coroutineStarter)
    {
        ThrowIf.Null(initialWorkers, nameof(initialWorkers));

        initialWorkers.ForEach(w => AddWorker(w));
    }

    public event Action<ITransportationWorker<T>> BecameFreeWorker;

    public int CountFreeWorkers => _freeWorkers.Count - _requestWorkerCount;
    public int CountBusyWorkers => _busyWorkers.Count;

    public void AddWorker(ITransportationWorker<T> worker)
    {
        ThrowIfDisposed();
        ThrowIf.Null(worker, nameof(worker));
        ThrowIf.Invalid(CanAddWorker(worker) == false,
            $"{nameof(worker)} already working in {nameof(TransportationTaskAssigner<T>)}");

        _freeWorkers.Enqueue(worker);
    }

    public bool CanAddWorker(ITransportationWorker<T> worker)
    {
        ThrowIfDisposed();

        return _freeWorkers.Contains(worker) == false && _busyWorkers.Contains(worker) == false;
    }

    public IEnumerator TakeFreeWorker(Action<ITransportationWorker<T>> takeAction)
    {
        ThrowIf.Null(takeAction, nameof(takeAction));

        _requestWorkerCount++;

        while (CountFreeWorkers < 0)
            yield return null;

        ITransportationWorker<T> workerForTake = _freeWorkers.Dequeue();
        takeAction(workerForTake);
        _requestWorkerCount--;
    }

    public bool IsBusy(ITransportationWorker<T> worker)
    {
        ThrowIf.Null(worker, nameof(worker));

        return _busyWorkers.Contains(worker);
    }

    public void AssignTask(TransportationTask<T> transportationTask)
    {
        ThrowIfDisposed();
        ThrowIf.Null(transportationTask, nameof(transportationTask));
        ThrowIf.Invalid(CanAssignTask() == false, $"{nameof(CountFreeWorkers)} must be positive");

        ITransportationWorker<T> worker = _freeWorkers.Dequeue();
        _busyWorkers.Add(worker);

        _coroutineStarter?.StartCoroutine(ExecuteTransportationTask(worker, transportationTask));
    }

    public bool CanAssignTask()
    {
        ThrowIfDisposed();

        return CountFreeWorkers > 0;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _freeWorkers.Clear();
        _busyWorkers.Clear();
        _disposed = true;
    }

    private IEnumerator ExecuteTransportationTask(ITransportationWorker<T> worker, TransportationTask<T> task)
    {
        ISubscribeProvider subscribeProvider = null;
        bool taskCompleted = false;

        try
        {
            subscribeProvider = SubscribeProvider<TransportationTask<T>, Action<TransportationTask<T>, ITransportationWorker<T>>>.Create(
                task,
                (_, w) =>
                {
                    taskCompleted = true;

                    if (w != null && _busyWorkers.Contains(w))
                        ReturnToFree(w);
                },
                (s, h) => s.Completed += h,
                (s, h) => s.Completed -= h);
            subscribeProvider.Subscribe();

            yield return task.Work(worker);
        }
        finally
        {
            subscribeProvider?.Dispose();

            if (taskCompleted == false && worker != null && _busyWorkers.Contains(worker))
                ReturnToFree(worker);
        }
    }

    private void ReturnToFree(ITransportationWorker<T> worker)
    {
        ThrowIfDisposed();
        ThrowIf.Argument(
            _busyWorkers.Contains(worker) == false,
            $"{nameof(worker)} must be in busy state",
            nameof(worker));

        _busyWorkers.Remove(worker);
        _freeWorkers.Enqueue(worker);
        BecameFreeWorker?.Invoke(worker);
    }

    private void ThrowIfDisposed()
    {
        ThrowIf.Disposed(_disposed, nameof(TransportationTaskAssigner<T>));
    }
}