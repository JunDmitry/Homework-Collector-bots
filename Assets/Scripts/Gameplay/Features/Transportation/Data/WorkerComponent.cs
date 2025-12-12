public class WorkerComponent<T> : IBehaviourComponent
    where T : IDeliverable
{
    public WorkerComponent(ITransportationWorker<T> worker)
    {
        Worker = worker;
    }

    public ITransportationWorker<T> Worker { get; set; }
}