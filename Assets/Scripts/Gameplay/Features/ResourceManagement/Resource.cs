using System;
using UnityEngine;

public class Resource : MonoBehaviour, IDeliverable, IIntractable, IEventSource<CollectedEvent<Resource>>
{
    private const int MinCount = 1;

    [SerializeField] private ItemDeliverableType _itemDeliverableType;
    [SerializeField, Min(MinCount)] private int _count = MinCount;
    [SerializeField, Min(0)] private float _minDistanceToTake;

    private IEventAggregator _eventAggregator;

    private event Action<CollectedEvent<Resource>> _collectedEvent;

    public ItemDeliverableType ItemDeliverableType => _itemDeliverableType;
    public int Count => _count;
    public float MinDistanceToTake => _minDistanceToTake;

    bool IEventSource<CollectedEvent<Resource>>.IsActive => enabled;

    event Action<CollectedEvent<Resource>> IEventSource<CollectedEvent<Resource>>.EventRaised
    {
        add => _collectedEvent += value;
        remove => _collectedEvent -= value;
    }

    private void OnEnable()
    {
        RegisterAll();
    }

    private void OnDisable()
    {
        UnregisterAll();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green * new Color(1, 1, 1, 0.5f);
        Gizmos.DrawSphere(transform.position, _minDistanceToTake);
    }

    public void Initialize(IEventAggregator eventAggregator)
    {
        ThrowIf.Null(eventAggregator, nameof(eventAggregator));

        if (_eventAggregator != null)
            UnregisterAll();

        _eventAggregator = eventAggregator;

        if (enabled)
            RegisterAll();
    }

    public void Interact()
    {
        _collectedEvent?.Invoke(new(this));
    }

    private void RegisterAll()
    {
        _eventAggregator?.RegisterSource(this);
    }

    private void UnregisterAll()
    {
        _eventAggregator?.UnregisterSource(this);
    }
}