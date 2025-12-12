public class PoolObjectReleasedEvent<TObj> : ObjectPoolEvent<TObj>, IEvent
    where TObj : class
{
    public PoolObjectReleasedEvent(ObjectPool<TObj> objectPool, TObj poolObject)
        : base(objectPool, poolObject)
    {
    }
}