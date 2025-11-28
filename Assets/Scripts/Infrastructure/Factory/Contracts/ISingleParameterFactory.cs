public interface ISingleParameterFactory<out TObj, in TArg> : IFactory
{
    TObj Create(TArg arg);
}
