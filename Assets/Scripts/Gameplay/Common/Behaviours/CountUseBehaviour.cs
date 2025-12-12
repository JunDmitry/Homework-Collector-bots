using System;
using System.Collections;

public class CountUseBehaviour : IBaseBehaviour
{
    private readonly Action _removeAction;
    private readonly int _useCountToRemove;

    private int _currentUseCount = 0;

    public CountUseBehaviour(Action removeAction, int count, int priority = (int)BaseBehaviourPriority.VeryHigh)
    {
        ThrowIf.Null(removeAction, nameof(removeAction));
        ThrowIf.Invalid(count <= 0, nameof(count));

        _removeAction = removeAction;
        _useCountToRemove = count;
        Priority = priority;
    }

    public int Priority { get; }

    public bool CanExecute(BehaviourContext behaviourContext = null)
    {
        return true;
    }

    public IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null)
    {
        if (cancellationToken?.IsCancellationRequested ?? false)
        {
            _removeAction?.Invoke();
            yield break;
        }

        _currentUseCount++;

        if (_currentUseCount == _useCountToRemove)
            _removeAction?.Invoke();

        yield break;
    }

    public void Dispose()
    {
    }
}