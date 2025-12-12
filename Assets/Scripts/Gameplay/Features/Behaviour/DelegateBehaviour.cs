using System;
using System.Collections;

public abstract class DelegateBehaviour : IBaseBehaviour
{
    private readonly Func<BehaviourContext, bool> _delegateCondition;
    private readonly Action<BehaviourContext> _configureContext;

    public DelegateBehaviour(
        Func<BehaviourContext, bool> delegateCondition = null,
        Action<BehaviourContext> configureContext = null,
        int priority = (int)BaseBehaviourPriority.Normal)
    {
        _delegateCondition = delegateCondition;
        _configureContext = configureContext;
        Priority = priority;
    }

    public int Priority { get; }

    public bool CanExecute(BehaviourContext behaviourContext = null)
    {
        return _delegateCondition?.Invoke(behaviourContext) ?? true;
    }

    public IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null)
    {
        if (cancellationToken?.IsCancellationRequested ?? false)
            yield break;

        _configureContext?.Invoke(behaviourContext);

        if (cancellationToken == null)
            yield return Execution(behaviourContext);
        else
            yield return Execution(behaviourContext).WithCancellation(cancellationToken);
    }

    public void Dispose()
    {
    }

    protected abstract IEnumerator Execution(BehaviourContext behaviourContext);
}