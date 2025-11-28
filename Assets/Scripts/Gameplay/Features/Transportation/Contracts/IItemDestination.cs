public interface IItemDestination<T>
{
    DeliveryOperation<T> PrepareReceival(T item);
}
