using Assets.Scripts.Common.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MultipleBaseBehaviour : IBaseBehaviour
{
    private readonly BehaviourConditionType _conditionType;

    private List<IBaseBehaviour> _behaviours;
    private int _priority;

    public MultipleBaseBehaviour(BehaviourConditionType conditionType, params IBaseBehaviour[] behaviours)
        : this(behaviours, conditionType)
    { }

    public MultipleBaseBehaviour(IEnumerable<IBaseBehaviour> behaviours, BehaviourConditionType conditionType = BehaviourConditionType.AllTrue)
    {
        _conditionType = conditionType;

        Initialize(behaviours);
    }

    public int Priority => _priority;

    public static IBaseBehaviour Combine(BehaviourConditionType conditionType, params IBaseBehaviour[] behaviours)
    {
        return Combine(behaviours, conditionType);
    }

    public static IBaseBehaviour Combine(IEnumerable<IBaseBehaviour> behaviours, BehaviourConditionType conditionType)
    {
        return new MultipleBaseBehaviour(behaviours, conditionType);
    }

    public void Dispose()
    {
        _behaviours.Clear();
    }

    public bool CanExecute(BehaviourContext behaviourContext = null)
    {
        if (_conditionType == BehaviourConditionType.AllTrue)
            return _behaviours.TrueForAll(b => b.CanExecute(behaviourContext));
        else if (_conditionType == BehaviourConditionType.AnyTrue)
            return _behaviours.Any(b => b.CanExecute(behaviourContext));
        else if (_conditionType == BehaviourConditionType.AllFalse)
            return _behaviours.TrueForAll(b => b.CanExecute(behaviourContext)) == false;
        else if (_conditionType == BehaviourConditionType.AnyFalse)
            return _behaviours.Any(b => b.CanExecute(behaviourContext) == false);

        return true;
    }

    public IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null)
    {
        if (cancellationToken?.IsCancellationRequested ?? false)
            yield break;

        foreach (IBaseBehaviour baseBehaviour in _behaviours.ToList())
            yield return baseBehaviour.Execute(behaviourContext, cancellationToken);
    }

    private void Initialize(IEnumerable<IBaseBehaviour> behaviours)
    {
        PriorityQueue<IBaseBehaviour, int> queue = new();
        int totalPriority = 0;
        int totalBehaviours = 0;

        foreach (IBaseBehaviour behaviour in behaviours)
        {
            queue.Enqueue(behaviour, behaviour.Priority);
            totalPriority += behaviour.Priority;
            totalBehaviours++;
        }

        ThrowIf.Argument(totalBehaviours == 0, $"Total count {nameof(IBaseBehaviour)} should be positive for work", nameof(behaviours));

        _priority = totalPriority / totalBehaviours;
        _behaviours = queue.ToList();
    }
}