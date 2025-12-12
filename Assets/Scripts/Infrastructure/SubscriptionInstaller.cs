using System;

public class SubscriptionInstaller : IDisposable
{
    private readonly IEventAggregator _eventAggregator;

    private readonly StructureSelector _structureSelector;
    private readonly MouseRaycaster _mouseRaycaster;

    public SubscriptionInstaller(IEventAggregator eventAggregator)
    {
        ThrowIf.Null(eventAggregator, nameof(eventAggregator));

        _eventAggregator = eventAggregator;
        _structureSelector = new(eventAggregator);
        _mouseRaycaster = new MouseRaycaster(eventAggregator);
    }

    public void Dispose()
    {
        _structureSelector?.Dispose();
        _mouseRaycaster?.Dispose();
    }
}