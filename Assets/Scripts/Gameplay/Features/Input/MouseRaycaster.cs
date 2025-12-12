using System;
using UnityEngine;

public class MouseRaycaster : IDisposable, IEventSource<GroundClickedEvent>, IEventSource<StructureSelectedEvent>
{
    private const int MaxRayDistance = 10000;
    private const int BufferSize = 16;

    private readonly IEventAggregator _aggregator;
    private readonly RaycastHit[] _hitsBuffer;

    private IEventSubscription _subscription;

    public MouseRaycaster(IEventAggregator aggregator, int bufferSize = BufferSize)
    {
        _hitsBuffer = new RaycastHit[bufferSize];
        _aggregator = aggregator;
        _subscription = _aggregator.Subscribe(new AlwaysTrueCondition<LeftMouseClickEvent>(), OnLeftMouseClick);
        _aggregator.RegisterSource<GroundClickedEvent>(this);
        _aggregator.RegisterSource<StructureSelectedEvent>(this);
    }

    private event Action<GroundClickedEvent> _groundClickedEvent;

    private event Action<StructureSelectedEvent> _structureSelectedEvent;

    event Action<GroundClickedEvent> IEventSource<GroundClickedEvent>.EventRaised
    {
        add => _groundClickedEvent += value;
        remove => _groundClickedEvent -= value;
    }

    event Action<StructureSelectedEvent> IEventSource<StructureSelectedEvent>.EventRaised
    {
        add => _structureSelectedEvent += value;
        remove => _structureSelectedEvent -= value;
    }

    bool IEventSource<GroundClickedEvent>.IsActive => true;

    bool IEventSource<StructureSelectedEvent>.IsActive => true;

    public void Dispose()
    {
        _subscription?.Dispose();
        _aggregator.UnregisterSource<GroundClickedEvent>(this);
        _aggregator.UnregisterSource<StructureSelectedEvent>(this);
    }

    private void OnLeftMouseClick(LeftMouseClickEvent @event)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(@event.ScreenPoint);
        int hitsCount = Physics.RaycastNonAlloc(mouseRay, _hitsBuffer, MaxRayDistance, Physics.AllLayers);
        Vector3? groundHitPoint = null;

        for (int i = 0; i < hitsCount; i++)
        {
            RaycastHit hit = _hitsBuffer[i];
            Collider gameObject = hit.collider;

            if (gameObject.TryGetComponent(out Structure structure))
            {
                _structureSelectedEvent?.Invoke(new(structure));
                return;
            }
            else if (gameObject.TryGetComponent<Ground>(out _))
            {
                groundHitPoint = hit.point;
            }
        }

        if (groundHitPoint.HasValue)
            _groundClickedEvent?.Invoke(new(groundHitPoint.Value));
    }
}