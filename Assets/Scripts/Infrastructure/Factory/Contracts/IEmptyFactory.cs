public interface IEmptyFactory<out TObj> : IFactory
{
    TObj Create();
}
