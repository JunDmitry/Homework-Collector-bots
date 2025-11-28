public interface IItemSource<T>
{
    DeliveryOperation<T> PrepareDelivery(T item);
}
