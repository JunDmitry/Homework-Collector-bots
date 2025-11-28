using System;
using UnityEngine;

public class Resource : MonoBehaviour, IDeliverable, ICollectedEvent<Resource>, IIntractable
{
    private const int MinCount = 1;

    [SerializeField] private ItemDeliverableType _itemDeliverableType;
    [SerializeField, Min(MinCount)] private int _count = MinCount;
    [SerializeField, Min(0)] private float _minDistanceToTake;

    public event Action<Resource> Collected;

    public ItemDeliverableType ItemDeliverableType => _itemDeliverableType;
    public int Count => _count;
    public float MinDistanceToTake => _minDistanceToTake;

    public void Interact()
    {
        Collected?.Invoke(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green * new Color(1, 1, 1, 0.5f);
        Gizmos.DrawSphere(transform.position, _minDistanceToTake);
    }
}