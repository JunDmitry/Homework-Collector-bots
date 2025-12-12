public class ChangedResourceCountEvent : IEvent
{
    public ChangedResourceCountEvent(ItemDeliverableType deliverableType, int totalCount)
    {
        DeliverableType = deliverableType;
        TotalCount = totalCount;
    }

    public ItemDeliverableType DeliverableType { get; }
    public int TotalCount { get; }
}