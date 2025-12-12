using System;
using System.Collections;

public readonly struct TimeBehaviourContext
{
    public TimeBehaviourContext(float interval = 0, float? executionWaitTime = null, Func<IEnumerator> makeAnimation = null, Action compensation = null)
    {
        Interval = interval;
        ExecutionWaitTime = executionWaitTime;
        MakeAnimation = makeAnimation;
        Compensation = compensation;
    }

    public float Interval { get; }
    public float? ExecutionWaitTime { get; }
    public Func<IEnumerator> MakeAnimation { get; }
    public Action Compensation { get; }
}