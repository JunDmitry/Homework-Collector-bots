public class ObjectPoolEvent<TObj>
    where TObj : class
{
    public ObjectPoolEvent(ObjectPool<TObj> objectPool, TObj poolObject)
    {
        ObjectPool = objectPool;
        PoolObject = poolObject;
    }

    public ObjectPool<TObj> ObjectPool { get; }
    public TObj PoolObject { get; }
}