using System.Collections;
using UnityEngine;

public class ScaleSetterBehaviour<T> : IBaseBehaviour
    where T : Component
{
    private readonly Vector3 _scaleToSet;

    public ScaleSetterBehaviour(Vector3 scaleToSet, int priority = (int)BaseBehaviourPriority.Normal)
    {
        _scaleToSet = scaleToSet;
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

        if (behaviourContext.TryGetComponent(out UnitComponent<T> unitComponent))
        {
            T unit = unitComponent.Unit;
            Vector3 initialScale = unit.transform.localScale;

            cancellationToken?.RegisterCallback(() => unit.transform.localScale = initialScale);

            unit.transform.localScale = _scaleToSet;
            behaviourContext.SetData(new InitialScaleComponent(initialScale));
        }
    }
}