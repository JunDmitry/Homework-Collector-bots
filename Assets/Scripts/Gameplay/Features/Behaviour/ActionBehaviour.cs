using System;
using System.Collections;

public class ActionBehaviour : DelegateBehaviour
{
    private readonly Action<BehaviourContext> _action;

    public ActionBehaviour(
        Action<BehaviourContext> action = null,
        Func<BehaviourContext, bool> delegateCondition = null,
        Action<BehaviourContext> configureContext = null,
        int priority = (int)BaseBehaviourPriority.Normal)
        : base(delegateCondition, configureContext, priority)
    {
        _action = action;
    }

    protected override IEnumerator Execution(BehaviourContext behaviourContext)
    {
        _action?.Invoke(behaviourContext);

        yield break;
    }
}
