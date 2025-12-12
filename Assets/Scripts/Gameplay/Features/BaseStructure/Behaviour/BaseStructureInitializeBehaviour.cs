using System.Collections;

public class BaseStructureInitializeBehaviour : IBaseBehaviour
{
    private readonly BaseStructureInitializer _initializer;

    public BaseStructureInitializeBehaviour(BaseStructureInitializer initializer, int priority = (int)BaseBehaviourPriority.Normal)
    {
        _initializer = initializer;
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

        cancellationToken?.EnterNonInterruptableState();

        BaseStructure baseStructure = behaviourContext.GetComponent<UnitComponent<BaseStructure>>().Unit;
        ITransportationWorker<Resource> worker = behaviourContext.GetComponent<WorkerComponent<Resource>>().Worker;

        _initializer.Initialize(baseStructure, new ITransportationWorker<Resource>[] { worker });
    }
}