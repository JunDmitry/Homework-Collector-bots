using System;
using System.Collections;

public class InnerBehaviour : IBaseBehaviour
{
    private readonly IBaseBehaviour _behaviour;
    private readonly Func<BehaviourContext, bool> _condition;

    public InnerBehaviour(IBaseBehaviour behaviour, Func<BehaviourContext, bool> condition)
    {
        ThrowIf.Null(behaviour, nameof(behaviour));
        ThrowIf.Null(condition, nameof(condition));

        _behaviour = behaviour;
        _condition = condition;
        Priority = behaviour.Priority;
    }

    public int Priority { get; }

    public void Dispose()
    {
        _behaviour.Dispose();
    }

    public bool CanExecute(BehaviourContext behaviourContext = null)
    {
        return _condition(behaviourContext);
    }

    public IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null)
    {
        while (_behaviour.CanExecute(behaviourContext) == false && cancellationToken?.IsCancellationRequested == false)
        {
            yield return null;
        }

        if (cancellationToken?.IsCancellationRequested ?? false)
            yield break;

        yield return _behaviour.Execute(behaviourContext).WithCancellation(cancellationToken);
    }
}