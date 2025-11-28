using System;

public class TwoParametersFactory<TObj, TArg1, TArg2> : ITwoParametersFactory<TObj, TArg1, TArg2>
{
    private readonly Func<TArg1, TArg2, TObj> _factoryMethod;

    public TwoParametersFactory(Func<TArg1, TArg2, TObj> factoryMethod)
    {
        ThrowIf.Null(factoryMethod, nameof(factoryMethod));

        _factoryMethod = factoryMethod;
    }

    public TObj Create(TArg1 arg1, TArg2 arg2)
    {
        return _factoryMethod(arg1, arg2);
    }
}