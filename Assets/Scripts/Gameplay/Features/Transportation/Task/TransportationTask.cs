using System;
using System.Collections;

public class TransportationTask<T> : IDisposable
    where T : IDeliverable
{
    private readonly DeliveryContext<T> _context;

    private bool _disposed;

    public TransportationTask(DeliveryContext<T> context)
    {
        _context = context;
    }

    public event Action<TransportationTask<T>, ITransportationWorker<T>> Completed;

    public IEnumerator Work(ITransportationWorker<T> worker)
    {
        DeliveryOperation<T> loadOperation = _context.Source.PrepareDelivery(_context.Item);

        yield return worker.MoveTo(loadOperation.Position, loadOperation.MinDistance);
        yield return worker.MakeLoad(_context.Item, loadOperation.OnEndOperation);
        yield return loadOperation.ExecutionCoroutine;

        DeliveryOperation<T> shipmentOperation = _context.Destination.PrepareReceival(_context.Item);

        yield return worker.MoveTo(shipmentOperation.Position, shipmentOperation.MinDistance);
        yield return worker.MakeShipment(shipmentOperation.OnEndOperation);
        yield return shipmentOperation.ExecutionCoroutine;

        Completed?.Invoke(this, worker);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
    }
}