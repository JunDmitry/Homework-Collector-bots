using System.Collections;
using UnityEngine;

public class ScaleAnimationToInitialBehaviour<T> : IBaseBehaviour
    where T : Component
{
    private readonly float _animationSeconds;

    public ScaleAnimationToInitialBehaviour(float animationSeconds, int priority = (int)BaseBehaviourPriority.Normal)
    {
        ThrowIf.Invalid(animationSeconds < 0, $"{nameof(animationSeconds)} can't be negative");

        _animationSeconds = animationSeconds;
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

        T unit = behaviourContext.GetComponent<UnitComponent<T>>().Unit;
        Vector3 initialScale = behaviourContext.GetComponent<InitialScaleComponent>().InitialScale;

        yield return unit.transform.DOSequenceScaleCompletion(unit.transform.localScale, initialScale, _animationSeconds)
            .WithCancellation(cancellationToken);
    }
}