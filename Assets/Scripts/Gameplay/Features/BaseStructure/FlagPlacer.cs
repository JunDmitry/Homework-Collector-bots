using System;
using UnityEngine;

public class FlagPlacer : IOwnerEventSource<CancelExecutingBuildBehaviourEvent>, IDisposable
{
    private readonly OwnerInfo _owner;
    private readonly Flag _flag;
    private readonly IEventAggregator _eventAggregator;

    private bool _placed;

    public FlagPlacer(OwnerInfo owner, Flag flag, IEventAggregator eventAggregator)
    {
        _owner = owner;
        _flag = flag;
        _eventAggregator = eventAggregator;
        CanPlace = true;

        eventAggregator.RegisterSource(this);
        flag.gameObject.SetActive(false);
    }

    private event Action<CancelExecutingBuildBehaviourEvent> _canceledBehaviour;

    event Action<CancelExecutingBuildBehaviourEvent> IEventSource<CancelExecutingBuildBehaviourEvent>.EventRaised
    {
        add => _canceledBehaviour += value;
        remove => _canceledBehaviour -= value;
    }

    bool IEventSource<CancelExecutingBuildBehaviourEvent>.IsActive => true;

    int IOwnerEventSource<CancelExecutingBuildBehaviourEvent>.OwnerInstanceId => _owner.InstanceId;
    public bool Placed => _placed;
    public bool CanPlace { get; private set; }
    public Vector3 FlagPosition => _flag.transform.position;

    public void Dispose()
    {
        _eventAggregator.UnregisterSource(this);
    }

    public bool TryPlace(Vector3 position)
    {
        if (CanPlace == false)
            return false;

        if (_placed)
            TryUnplace();

        _placed = true;
        _flag.transform.position = position;
        _flag.gameObject.SetActive(true);

        return true;
    }

    public bool TryUnplace()
    {
        if (CanPlace == false)
            return false;

        if (_placed == false)
            return false;

        _placed = false;
        _flag.transform.position = _owner.Position;
        _flag.gameObject.SetActive(false);
        _canceledBehaviour?.Invoke(new());

        return true;
    }

    public void LockPosition()
    {
        CanPlace = false;
    }

    public void UnlockPosition()
    {
        CanPlace = true;
    }
}