using System;
using System.Collections.Generic;

public class ObjectPool<T>
    where T : class
{
    private readonly Factory<T> _factory;
    private readonly Stack<T> _pool;
    private readonly int _initialSize;

    public ObjectPool(int initialSize = 1)
        : this(new Factory<T>(), initialSize)
    { }

    public ObjectPool(Func<T> factoryMethod, int initialSize = 1)
        : this(new Factory<T>(factoryMethod), initialSize)
    { }

    public ObjectPool(Factory<T> factory, int initialSize = 1)
    {
        _factory = factory;
        _pool = new();
        _initialSize = initialSize;
    }

    public event Action<T> Created;

    public event Action<T> Getted;

    public event Action<T> Released;

    public virtual T Get()
    {
        T obj = _pool.Count == 0 ? Create() : _pool.Pop();

        Getted?.Invoke(obj);

        return obj;
    }

    public virtual void Release(T obj)
    {
        _pool.Push(obj);
        Released?.Invoke(obj);
    }

    public void Initialize()
    {
        T obj;

        while (_pool.Count < _initialSize)
        {
            obj = Create();
            _pool.Push(obj);
        }
    }

    protected virtual T Create()
    {
        T obj = _factory.Create();

        Created?.Invoke(obj);

        return obj;
    }

    protected T CreateWithoutEvent()
    {
        return _factory.Create();
    }
}