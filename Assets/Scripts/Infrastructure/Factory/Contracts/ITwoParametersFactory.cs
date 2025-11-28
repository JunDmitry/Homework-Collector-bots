public interface ITwoParametersFactory<out TObj, in TArg1, in TArg2> : IFactory
{
    TObj Create(TArg1 arg1, TArg2 arg2);
}
