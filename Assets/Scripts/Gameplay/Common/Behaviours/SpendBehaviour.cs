using System;
using System.Collections;

public class SpendBehaviour : IBaseBehaviour
{
    public static SpendBehaviour Empty = new(() => true, () => { }, () => { });

    private readonly Func<bool> _canSpend;
    private readonly Action _spend;
    private readonly Action _compensation;

    public SpendBehaviour(ResourceSpendRequest spendRequest, int priority = (int)BaseBehaviourPriority.Normal)
        : this(spendRequest.CanSpend, spendRequest.Spend, spendRequest.Compensation, priority)
    { }

    public SpendBehaviour(Func<bool> canSpend, Action spend, Action compensation, int priority = (int)BaseBehaviourPriority.Normal)
    {
        ThrowIf.Null(canSpend, nameof(canSpend));
        ThrowIf.Null(spend, nameof(spend));

        _canSpend = canSpend;
        _spend = spend;
        _compensation = compensation;
        Priority = priority;
    }

    public int Priority { get; }

    public bool CanExecute(BehaviourContext behaviourContext = null)
    {
        return _canSpend();
    }

    public IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null)
    {
        if (cancellationToken?.IsCancellationRequested ?? false)
            yield break;

        _spend();
        cancellationToken?.RegisterCallback(_compensation);

        yield break;
    }

    public void Dispose()
    {
    }
}