using System;
using System.Collections;

public class GetterWorkerBehaviour<T> : IBaseBehaviour
    where T : IDeliverable
{
    private readonly Func<Action<ITransportationWorker<T>>, IEnumerator> _getWorker;
    private readonly Action<ITransportationWorker<T>> _compensation;

    public GetterWorkerBehaviour(
        Func<Action<ITransportationWorker<T>>, IEnumerator> getWorker,
        Action<ITransportationWorker<T>> compensation = null,
        int priority = (int)BaseBehaviourPriority.Normal)
    {
        ThrowIf.Null(getWorker, nameof(getWorker));

        _getWorker = getWorker;
        _compensation = compensation;
        Priority = priority;
    }

    public int Priority { get; }

    public bool CanExecute(BehaviourContext behaviourContext = null)
    {
        return true;
    }

    public void Dispose()
    {
    }

    public IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null)
    {
        if (cancellationToken?.IsCancellationRequested ?? false)
            yield break;

        ThrowIf.Null(behaviourContext, nameof(behaviourContext));

        ITransportationWorker<T> worker = null;
        cancellationToken?.RegisterCallback(() => _compensation?.Invoke(worker));

        yield return _getWorker(w => worker = w).WithCancellation(cancellationToken);

        behaviourContext.SetData(new WorkerComponent<T>(worker));
    }
}