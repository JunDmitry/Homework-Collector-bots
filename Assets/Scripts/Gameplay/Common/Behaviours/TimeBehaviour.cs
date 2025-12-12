using System;
using System.Collections;
using UnityEngine;

public class TimeBehaviour : IBaseBehaviour
{
    private readonly float _interval;
    private readonly float _executionWaitTime;
    private readonly Func<IEnumerator> _makeAnimation;
    private readonly Action _compensation;

    private float _lastExecutionTime;

    public TimeBehaviour(TimeBehaviourContext context, int priority = (int)BaseBehaviourPriority.Normal)
        : this(context.Interval, context.ExecutionWaitTime, context.MakeAnimation, context.Compensation, priority)
    {
    }

    public TimeBehaviour(
        float interval = 0,
        float? executionWaitTime = null,
        Func<IEnumerator> makeAnimation = null,
        Action compensation = null,
        int priority = (int)BaseBehaviourPriority.Normal)
    {
        ThrowIf.Argument(interval < 0, $"{nameof(interval)} should be positive", nameof(interval));
        ThrowIf.Argument(executionWaitTime.HasValue && executionWaitTime.Value < 0, $"{nameof(executionWaitTime)} should be positive", nameof(executionWaitTime));

        _interval = interval;
        _makeAnimation = makeAnimation;
        _compensation = compensation;
        Priority = priority;
        _executionWaitTime = executionWaitTime ?? 0f;
    }

    public int Priority { get; }

    public static TimeBehaviour Create(
        float interval = 0,
        float? executionWaitTime = null,
        Func<IEnumerator> makeAnimation = null,
        Action compensation = null,
        int priority = (int)BaseBehaviourPriority.Normal)
    {
        return new TimeBehaviour(interval, executionWaitTime, makeAnimation, compensation, priority);
    }

    public static TimeBehaviour Create(TimeBehaviourContext context, int priority = (int)BaseBehaviourPriority.Normal)
    {
        return new TimeBehaviour(context, priority);
    }

    public bool CanExecute(BehaviourContext behaviourContext = null)
    {
        return Time.time - _lastExecutionTime > _interval;
    }

    public IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null)
    {
        if (cancellationToken?.IsCancellationRequested ?? false)
            yield break;

        cancellationToken?.RegisterCallback(_compensation);
        float executionTime = 0f;

        while (executionTime < _executionWaitTime && cancellationToken?.IsCancellationRequested == false)
        {
            executionTime += Time.deltaTime;
            yield return null;
        }

        _lastExecutionTime = Time.time;

        if (cancellationToken?.IsCancellationRequested == false && _makeAnimation != null)
            yield return _makeAnimation().WithCancellation(cancellationToken);

        yield break;
    }

    public void Dispose()
    {
    }
}