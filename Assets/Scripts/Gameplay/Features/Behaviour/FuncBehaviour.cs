using System;
using System.Collections;

public class FuncBehaviour : DelegateBehaviour
{
    private readonly Func<BehaviourContext, IEnumerator> _func;

    public FuncBehaviour(
        Func<BehaviourContext, IEnumerator> func = null,
        Func<BehaviourContext, bool> delegateCondition = null,
        Action<BehaviourContext> configureContext = null,
        int priority = (int)BaseBehaviourPriority.Normal)
        : base(delegateCondition, configureContext, priority)
    {
        _func = func;
    }

    protected override IEnumerator Execution(BehaviourContext behaviourContext)
    {
        if (_func == null)
            yield break;

        yield return _func(behaviourContext);
    }
}