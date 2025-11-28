using System;

public class Factory<T> : IEmptyFactory<T>
{
    private static readonly Func<T> s_defaultFactory = Activator.CreateInstance<T>;

    private readonly Func<T> _factoryMethod;
    private readonly Action<T> _initialConfiguration;

    public Factory() : this(s_defaultFactory)
    { }

    public Factory(Func<T> factoryMethod, Action<T> initialConfiguration = null)
    {
        ThrowIf.Null(factoryMethod, nameof(factoryMethod));

        _factoryMethod = factoryMethod;
        _initialConfiguration = initialConfiguration;
    }

    public T Create()
    {
        T obj = _factoryMethod.Invoke();

        Configure(obj);

        return obj;
    }

    protected virtual void Configure(T objectToConfigure)
    {
        ThrowIf.Null(objectToConfigure, nameof(objectToConfigure));

        _initialConfiguration?.Invoke(objectToConfigure);
    }
}