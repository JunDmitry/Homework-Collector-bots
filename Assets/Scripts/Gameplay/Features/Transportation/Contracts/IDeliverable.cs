public interface IDeliverable
{
    ItemDeliverableType ItemDeliverableType { get; }
    int Count { get; }
    float MinDistanceToTake { get; }
}