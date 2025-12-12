public class PoolObjectGettedEvent<TObj> : ObjectPoolEvent<TObj>, IEvent
    where TObj : class
{
    public PoolObjectGettedEvent(ObjectPool<TObj> objectPool, TObj poolObject)
        : base(objectPool, poolObject)
    {
    }
}