using System;

public class ThreeParametersFactory<TObj, TArg1, TArg2, TArg3> : IThreeParametersFactory<TObj, TArg1, TArg2, TArg3>
{
    private readonly Func<TArg1, TArg2,TArg3, TObj> _factoryMethod;

    public ThreeParametersFactory(Func<TArg1, TArg2, TArg3, TObj> factoryMethod)
    {
        ThrowIf.Null(factoryMethod, nameof(factoryMethod));

        _factoryMethod = factoryMethod;
    }

    public TObj Create(TArg1 arg1, TArg2 arg2, TArg3 arg3)
    {
        return _factoryMethod(arg1, arg2, arg3);
    }
}