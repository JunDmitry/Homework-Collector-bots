using System;

public class SingleUseBehaviour : CountUseBehaviour
{
    public SingleUseBehaviour(Action removeAction, int priority = (int)BaseBehaviourPriority.VeryHigh)
        : base(removeAction, 1, priority)
    {
    }
}