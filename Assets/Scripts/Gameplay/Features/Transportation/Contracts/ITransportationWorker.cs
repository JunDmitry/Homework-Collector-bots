public interface ITransportationWorker<T> : IWorkerLoader<T>, IWorkerShipment<T>
    where T : IDeliverable
{ }
