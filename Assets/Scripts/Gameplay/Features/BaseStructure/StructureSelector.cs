using System;
using System.Collections.Generic;
using UnityEngine;

public class StructureSelector : IDisposable
{
    private static readonly float s_maxGroundRaycastDistance = 100f;

    private readonly List<IEventSubscription> _subscriptions;
    private readonly AreaRaycaster _areaRaycaster;

    private BaseStructure _selectedStructure = null;

    public StructureSelector(IEventAggregator eventAggregator)
    {
        ThrowIf.Null(eventAggregator, nameof(eventAggregator));

        _subscriptions = new()
        {
            eventAggregator.Subscribe(new AlwaysTrueCondition<GroundClickedEvent>(), OnGroundClicked),
            eventAggregator.Subscribe(new AlwaysTrueCondition<StructureSelectedEvent>(), OnStructureSelected)
        };
        _areaRaycaster = new AreaRaycaster(s_maxGroundRaycastDistance);
    }

    public void Dispose()
    {
        _subscriptions.ForEach(s => s?.Dispose());
    }

    private void OnGroundClicked(GroundClickedEvent @event)
    {
        if (_selectedStructure == null)
            return;

        Vector3 clickPoint = @event.ClickPosition;

        if (_areaRaycaster.CanPlaceBuilding(clickPoint, _selectedStructure.TotalArea) == false)
            return;

        _selectedStructure.PlaceFlag(clickPoint);
    }

    private void OnStructureSelected(StructureSelectedEvent @event)
    {
        BaseStructure selectedStructure = @event.SelectedStructure as BaseStructure;

        if (selectedStructure == null)
            return;

        _selectedStructure = selectedStructure;
    }
}