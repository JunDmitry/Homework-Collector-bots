using System;

public class SingleParameterFactory<TObj, TArg> : ISingleParameterFactory<TObj, TArg>
{
    private readonly Func<TArg, TObj> _factoryMethod;

    public SingleParameterFactory(Func<TArg, TObj> factoryMethod)
    {
        ThrowIf.Null(factoryMethod, nameof(factoryMethod));

        _factoryMethod = factoryMethod;
    }

    public TObj Create(TArg arg)
    {
        return _factoryMethod(arg);
    }
}