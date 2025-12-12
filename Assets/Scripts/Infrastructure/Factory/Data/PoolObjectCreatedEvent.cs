public class PoolObjectCreatedEvent<TObj> : ObjectPoolEvent<TObj>, IEvent
    where TObj : class
{
    public PoolObjectCreatedEvent(ObjectPool<TObj> objectPool, TObj poolObject)
        : base(objectPool, poolObject)
    {
    }
}