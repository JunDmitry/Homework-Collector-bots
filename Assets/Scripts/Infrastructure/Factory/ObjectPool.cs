using System;
using System.Collections.Generic;

public class ObjectPool<T> :
    IDisposable,
    IEventSource<PoolObjectCreatedEvent<T>>,
    IEventSource<PoolObjectGettedEvent<T>>,
    IEventSource<PoolObjectReleasedEvent<T>>
    where T : class
{
    private readonly Factory<T> _factory;
    private readonly IEventAggregator _eventAggregator;
    private readonly Stack<T> _pool;
    private readonly int _initialSize;

    private readonly HashSet<T> _pooledObjects;
    private readonly IEventSubscription _subscription;

    bool IEventSource<PoolObjectCreatedEvent<T>>.IsActive => true;

    bool IEventSource<PoolObjectGettedEvent<T>>.IsActive => true;

    bool IEventSource<PoolObjectReleasedEvent<T>>.IsActive => true;

    public ObjectPool(IEventAggregator eventAggregator, int initialSize = 1)
        : this(new Factory<T>(), eventAggregator, initialSize)
    { }

    public ObjectPool(Func<T> factoryMethod, IEventAggregator eventAggregator, int initialSize = 1)
        : this(new Factory<T>(factoryMethod), eventAggregator, initialSize)
    { }

    public ObjectPool(Factory<T> factory, IEventAggregator eventAggregator, int initialSize = 1)
    {
        _factory = factory;
        _eventAggregator = eventAggregator;
        _pool = new();
        _initialSize = initialSize;
        _pooledObjects = new();

        _eventAggregator.RegisterSource<PoolObjectCreatedEvent<T>>(this);
        _eventAggregator.RegisterSource<PoolObjectGettedEvent<T>>(this);
        _eventAggregator.RegisterSource<PoolObjectReleasedEvent<T>>(this);
        _subscription = _eventAggregator.Subscribe(new AlwaysTrueCondition<CollectedEvent<T>>(), OnPoolObjectCollected);
    }

    private event Action<PoolObjectCreatedEvent<T>> _createdEvent;

    private event Action<PoolObjectGettedEvent<T>> _gettedEvent;

    private event Action<PoolObjectReleasedEvent<T>> _releasedEvent;

    event Action<PoolObjectCreatedEvent<T>> IEventSource<PoolObjectCreatedEvent<T>>.EventRaised
    {
        add => _createdEvent += value;
        remove => _createdEvent -= value;
    }

    event Action<PoolObjectGettedEvent<T>> IEventSource<PoolObjectGettedEvent<T>>.EventRaised
    {
        add => _gettedEvent += value;
        remove => _gettedEvent -= value;
    }

    event Action<PoolObjectReleasedEvent<T>> IEventSource<PoolObjectReleasedEvent<T>>.EventRaised
    {
        add => _releasedEvent += value;
        remove => _releasedEvent -= value;
    }

    public void Dispose()
    {
        _eventAggregator.UnregisterSource<PoolObjectCreatedEvent<T>>(this);
        _eventAggregator.UnregisterSource<PoolObjectGettedEvent<T>>(this);
        _eventAggregator.UnregisterSource<PoolObjectReleasedEvent<T>>(this);
        _subscription?.Dispose();
    }

    public virtual T Get()
    {
        T obj = _pool.Count == 0 ? Create() : _pool.Pop();

        _pooledObjects.Add(obj);
        _gettedEvent?.Invoke(new(this, obj));

        return obj;
    }

    public virtual void Release(T obj)
    {
        _pool.Push(obj);
        _pooledObjects.Remove(obj);
        _releasedEvent?.Invoke(new(this, obj));
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

        _createdEvent?.Invoke(new(this, obj));

        return obj;
    }

    private void OnPoolObjectCollected(CollectedEvent<T> collectedEvent)
    {
        T item = collectedEvent.CollectedItem;

        if (_pooledObjects.Contains(item))
            Release(item);
    }
}