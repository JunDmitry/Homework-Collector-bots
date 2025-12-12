public class ResourceSpendRequest
{
    private readonly Storage<Resource> _targetStorage;
    private readonly ResourceSpendContext _requirement;

    public ResourceSpendRequest(Storage<Resource> targetStorage, ResourceSpendContext requirement)
    {
        _targetStorage = targetStorage;
        _requirement = requirement;
    }

    public bool CanSpend()
    {
        bool canSpend = CanTake(ItemDeliverableType.Stone, _requirement.StoneCount)
            && CanTake(ItemDeliverableType.Wood, _requirement.WoodCount)
            && CanTake(ItemDeliverableType.Iron, _requirement.IronCount);

        return canSpend;
    }

    public void Spend()
    {
        ThrowIf.Invalid(CanSpend() == false, $"There are not enough resources to build a building");

        if (_requirement.StoneCount > 0)
            _targetStorage.TryTakeResourceStrict(ItemDeliverableType.Stone, _requirement.StoneCount);

        if (_requirement.WoodCount > 0)
            _targetStorage.TryTakeResourceStrict(ItemDeliverableType.Wood, _requirement.WoodCount);

        if (_requirement.IronCount > 0)
            _targetStorage.TryTakeResourceStrict(ItemDeliverableType.Iron, _requirement.IronCount);
    }

    public void Compensation()
    {
        _targetStorage.AddResource(ItemDeliverableType.Stone, _requirement.StoneCount);
        _targetStorage.AddResource(ItemDeliverableType.Wood, _requirement.WoodCount);
        _targetStorage.AddResource(ItemDeliverableType.Iron, _requirement.IronCount);
    }

    private bool CanTake(ItemDeliverableType type, int count)
    {
        return count == 0 || _targetStorage.ContainsResourceAtLeast(type, count);
    }
}