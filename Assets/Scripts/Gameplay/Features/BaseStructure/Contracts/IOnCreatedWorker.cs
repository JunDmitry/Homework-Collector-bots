public interface IOnCreatedWorker<T>
    where T : IDeliverable
{
    void OnCreatedWorker(ITransportationWorker<T> worker);
}
