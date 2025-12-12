using Assets.Scripts.Common.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

public class BuilderManagement : IDisposable
{
    private readonly ITwoParametersFactory<SpendBehaviour, ResourceSpendRequest, int> _spendBehaviourFactory;
    private readonly ITwoParametersFactory<TimeBehaviour, TimeBehaviourContext, int> _timeBehaviourFactory;
    private readonly PriorityQueue<IBaseBehaviour, float> _behaviours;

    private readonly IEventAggregator _eventAggregator;
    private readonly OwnerInfo _ownerInfo;
    private readonly BehaviourRunner _behaviourRunner;

    private IEventSubscription _subscription;
    private IBaseBehaviour _executingBehaviour = null;

    public BuilderManagement(
        ITwoParametersFactory<SpendBehaviour, ResourceSpendRequest, int> spendBehaviourFactory,
        ITwoParametersFactory<TimeBehaviour, TimeBehaviourContext, int> timeBehaviourFactory,
        IEventAggregator eventAggregator,
        OwnerInfo ownerInfo,
        ICoroutineRunner coroutineRunner)
    {
        ThrowIf.Null(spendBehaviourFactory, nameof(spendBehaviourFactory));
        ThrowIf.Null(timeBehaviourFactory, nameof(timeBehaviourFactory));
        ThrowIf.Null(eventAggregator, nameof(eventAggregator));

        _behaviours = new();
        _spendBehaviourFactory = spendBehaviourFactory;
        _timeBehaviourFactory = timeBehaviourFactory;
        _eventAggregator = eventAggregator;
        _ownerInfo = ownerInfo;
        _behaviourRunner = new(coroutineRunner);
        _subscription = _eventAggregator.Subscribe(
            new SubscribeConditionBuilder<CancelExecutingBuildBehaviourEvent>(
                new SourceTypeCondition<CancelExecutingBuildBehaviourEvent, IOwnerEventSource<CancelExecutingBuildBehaviourEvent>>())
                .AndSourceProperty(
                    s => ((IOwnerEventSource<CancelExecutingBuildBehaviourEvent>)s).OwnerInstanceId,
                    p => p == _ownerInfo.InstanceId)
                .Build(),
            OnCancel);
    }

    public void AddBehaviour<T>(
        BuildBehaviourContext<T> buildBehaviourContext,
        TimeBehaviourContext timeBehaviourContext,
        params IBaseBehaviour[] additionalBehaviour)
    {
        AddBehaviour(buildBehaviourContext, timeBehaviourContext, (IEnumerable<IBaseBehaviour>)additionalBehaviour);
    }

    public void AddBehaviour<T>(
        BuildBehaviourContext<T> buildBehaviourContext,
        TimeBehaviourContext timeBehaviourContext,
        IEnumerable<IBaseBehaviour> additionalBehaviour)
    {
        ThrowIf.Null(buildBehaviourContext, nameof(buildBehaviourContext));
        ThrowIf.Null(timeBehaviourContext, nameof(timeBehaviourContext));

        IBaseBehaviour behaviour = Create(buildBehaviourContext, timeBehaviourContext, additionalBehaviour);
        _behaviours.Enqueue(behaviour, behaviour.Priority);
    }

    public void AddSingleUseBehaviour<T>(
        BuildBehaviourContext<T> buildBehaviourContext,
        TimeBehaviourContext timeBehaviourContext,
        params IBaseBehaviour[] additionalBehaviour)
    {
        AddCountUseBehaviour(buildBehaviourContext, timeBehaviourContext, 1, (IEnumerable<IBaseBehaviour>)additionalBehaviour);
    }

    public void AddSingleUseBehaviour<T>(
        BuildBehaviourContext<T> buildBehaviourContext,
        TimeBehaviourContext timeBehaviourContext,
        IEnumerable<IBaseBehaviour> additionalBehaviour)
    {
        AddCountUseBehaviour(buildBehaviourContext, timeBehaviourContext, 1, additionalBehaviour);
    }

    public void AddCountUseBehaviour<T>(
        BuildBehaviourContext<T> buildBehaviourContext,
        TimeBehaviourContext timeBehaviourContext,
        int count,
        params IBaseBehaviour[] additionalBehaviour)
    {
        AddCountUseBehaviour(buildBehaviourContext, timeBehaviourContext, count, (IEnumerable<IBaseBehaviour>)additionalBehaviour);
    }

    public void AddCountUseBehaviour<T>(
        BuildBehaviourContext<T> buildBehaviourContext,
        TimeBehaviourContext timeBehaviourContext,
        int count,
        IEnumerable<IBaseBehaviour> additionalBehaviour)
    {
        ThrowIf.Null(buildBehaviourContext, nameof(buildBehaviourContext));
        ThrowIf.Null(timeBehaviourContext, nameof(timeBehaviourContext));

        IBaseBehaviour behaviour = Create(buildBehaviourContext, timeBehaviourContext, additionalBehaviour);
        behaviour = MultipleBaseBehaviour.Combine(BehaviourConditionType.AllTrue,
            behaviour, new CountUseBehaviour(() =>
            {
                _behaviours.Remove(behaviour);
                behaviour.Dispose();
            },
            count,
            behaviour.Priority - 1));

        _behaviours.Enqueue(behaviour, behaviour.Priority);
    }

    public IEnumerator Execute()
    {
        foreach (IBaseBehaviour behaviour in _behaviours)
        {
            if (behaviour.CanExecute() == false)
                continue;

            _executingBehaviour = behaviour;
        }

        if (_executingBehaviour == null)
        {
            yield return null;
            yield break;
        }

        _behaviourRunner.ExecuteBehaviour(_executingBehaviour);

        while (_behaviourRunner.IsExecute(_executingBehaviour))
            yield return null;

        _executingBehaviour = null;
    }

    public void Dispose()
    {
        foreach (IDisposable disposable in _behaviours)
            disposable.Dispose();

        _subscription?.Dispose();
        _behaviours.Clear();
    }

    private IBaseBehaviour Create<T>(
        BuildBehaviourContext<T> buildBehaviourContext,
        TimeBehaviourContext timeContext,
        IEnumerable<IBaseBehaviour> additionalBehaviours)
    {
        List<IBaseBehaviour> behaviours = new();

        IBaseBehaviour spendBehaviour = _spendBehaviourFactory.Create(buildBehaviourContext.SpendRequest, (int)BaseBehaviourPriority.VeryHigh);
        IBaseBehaviour timeBehaviour = _timeBehaviourFactory.Create(timeContext, (int)BaseBehaviourPriority.Normal);
        BuildBehaviour<T> buildBehaviour = new(buildBehaviourContext, _ownerInfo, _eventAggregator, (int)BaseBehaviourPriority.VeryLow);

        behaviours.Add(spendBehaviour);
        behaviours.Add(timeBehaviour);
        behaviours.Add(buildBehaviour);

        if (additionalBehaviours != null)
            behaviours.AddRange(additionalBehaviours);

        return MultipleBaseBehaviour.Combine(behaviours, BehaviourConditionType.AllTrue);
    }

    private void OnCancel(CancelExecutingBuildBehaviourEvent behaviourEvent)
    {
        if (_executingBehaviour == null)
            return;

        if (_behaviourRunner.CanInterrupt(_executingBehaviour) == false)
            return;

        _behaviourRunner.CancelBehaviour(_executingBehaviour);
    }
}