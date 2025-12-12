using System.Collections;
using UnityEngine;

public class WorkerMoverToStructureBehaviour<T> : IBaseBehaviour
    where T : IDeliverable
{
    public WorkerMoverToStructureBehaviour(int priority = (int)BaseBehaviourPriority.Normal)
    {
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

        ITransportationWorker<T> worker = behaviourContext.GetComponent<WorkerComponent<T>>().Worker;
        BaseStructure baseStructure = behaviourContext.GetComponent<UnitComponent<BaseStructure>>().Unit;

        float dimensionMultiplier = .5f;
        Vector2 dimensions = baseStructure.TotalArea.MaxDimensions();
        float minDistance = Mathf.Max(dimensions.x * dimensionMultiplier, dimensions.y * dimensionMultiplier);

        yield return worker.MoveTo(baseStructure.transform.position, minDistance).WithCancellation(cancellationToken);
    }
}