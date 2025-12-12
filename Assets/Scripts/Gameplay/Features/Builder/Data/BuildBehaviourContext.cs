public readonly struct BuildBehaviourContext<T>
{
    public BuildBehaviourContext(IEmptyFactory<T> objectFactory, ResourceSpendRequest spendRequest)
    {
        ObjectFactory = objectFactory;
        SpendRequest = spendRequest;
    }

    public IEmptyFactory<T> ObjectFactory { get; }
    public ResourceSpendRequest SpendRequest { get; }
}