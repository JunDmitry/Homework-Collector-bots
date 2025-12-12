using System;
using System.Collections;

public class BuildBehaviour<T> : IOwnerEventSource<UnitCreatedEvent<T>>, IBaseBehaviour
{
    private readonly IEmptyFactory<T> _objectFactory;
    private readonly OwnerInfo _ownerInfo;
    private readonly IEventAggregator _eventAggregator;

    private event Action<UnitCreatedEvent<T>> _unitCreatedRaised;

    public BuildBehaviour(
        BuildBehaviourContext<T> context,
        OwnerInfo ownerInfo,
        IEventAggregator eventAggregator,
        int priority = (int)BaseBehaviourPriority.Normal)
        : this(context.ObjectFactory, ownerInfo, eventAggregator, priority)
    { }

    public BuildBehaviour(
        IEmptyFactory<T> objectFactory,
        OwnerInfo ownerInfo,
        IEventAggregator eventAggregator,
        int priority = (int)BaseBehaviourPriority.Normal)
    {
        ThrowIf.Null(objectFactory, nameof(objectFactory));

        _objectFactory = objectFactory;
        _ownerInfo = ownerInfo;
        _eventAggregator = eventAggregator;
        Priority = priority;

        _eventAggregator.RegisterSource(this);
    }

    event Action<UnitCreatedEvent<T>> IEventSource<UnitCreatedEvent<T>>.EventRaised
    {
        add => _unitCreatedRaised += value;
        remove => _unitCreatedRaised -= value;
    }

    public int Priority { get; }

    int IOwnerEventSource<UnitCreatedEvent<T>>.OwnerInstanceId => _ownerInfo.InstanceId;

    bool IEventSource<UnitCreatedEvent<T>>.IsActive => true;

    public bool CanExecute(BehaviourContext behaviourContext = null)
    {
        return true;
    }

    public IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null)
    {
        if (cancellationToken?.IsCancellationRequested ?? false)
            yield break;

        cancellationToken.EnterNonInterruptableState();
        T buildedObject = _objectFactory.Create();
        _unitCreatedRaised?.Invoke(new UnitCreatedEvent<T>(buildedObject));

        behaviourContext?.SetData(new UnitComponent<T>(buildedObject));

        yield break;
    }

    public void Dispose()
    {
        _eventAggregator?.UnregisterSource(this);
    }
}