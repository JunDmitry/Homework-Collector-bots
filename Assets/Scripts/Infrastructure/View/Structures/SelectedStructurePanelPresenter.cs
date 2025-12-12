using System;
using System.Collections.Generic;

public class SelectedStructurePanelPresenter : IPresenter
{
    private readonly Dictionary<Structure, IPresenter> _presenterByStructure;
    private readonly Func<Structure, IPresenter> _presenterFactory;

    private List<IEventSubscription> _subscriptions;
    private IPresenter _selectedStructure;

    public SelectedStructurePanelPresenter(IEventAggregator eventAggregator, Func<Structure, IPresenter> presenterFactory)
    {
        _presenterByStructure = new();
        _subscriptions = new()
        {
            eventAggregator.Subscribe(new AlwaysTrueCondition<StructureSelectedEvent>(), OnStructureSelect),
            eventAggregator.Subscribe(new AlwaysTrueCondition<UnitCreatedEvent<BaseStructure>>(), OnCreateStructure)
        };
        _presenterFactory = presenterFactory;
    }

    public bool Enabled { get; private set; } = true;

    public void Dispose()
    {
        _subscriptions.ForEach(s => s?.Dispose());
    }

    public void Hide()
    {
        if (Enabled == false)
            return;

        Enabled = false;
        _selectedStructure?.Hide();
    }

    public void Show()
    {
        if (Enabled)
            return;

        Enabled = true;
        _selectedStructure?.Show();
    }

    private void OnStructureSelect(StructureSelectedEvent @event)
    {
        Structure currentSelection = @event.SelectedStructure;

        if (currentSelection == null || _presenterByStructure.TryGetValue(currentSelection, out IPresenter currentPresenter) == false)
            return;

        if (_selectedStructure == currentPresenter)
            return;

        _selectedStructure?.Hide();
        _selectedStructure = currentPresenter;
        _selectedStructure?.Show();
    }

    private void OnCreateStructure(UnitCreatedEvent<BaseStructure> @event)
    {
        Structure structure = @event.CreatedUnit;

        if (_presenterByStructure.ContainsKey(structure))
            return;

        _presenterByStructure[structure] = _presenterFactory(structure);
    }
}