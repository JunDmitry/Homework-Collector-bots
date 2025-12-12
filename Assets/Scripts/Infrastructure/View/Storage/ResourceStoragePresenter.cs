using System.Collections.Generic;
using UnityEngine;

public class ResourceStoragePresenter : IPresenter
{
    private readonly Dictionary<ItemDeliverableType, int> _cachedState;
    private readonly int _ownerId;

    private StorageView _view;
    private IEventSubscription _subscription;

    private bool _disposed = false;
    private bool _isDirty = true;

    public ResourceStoragePresenter(StorageView view, IEventAggregator eventAggregator, int ownerId)
    {
        _view = view;
        _ownerId = ownerId;
        _cachedState = new();
        _subscription = eventAggregator.Subscribe(
            new SubscribeConditionBuilder<ChangedResourceCountEvent>(
                new SourceTypeCondition<ChangedResourceCountEvent, IOwnerEventSource<ChangedResourceCountEvent>>())
                .AndSourceProperty(
                    s => ((IOwnerEventSource<ChangedResourceCountEvent>)s).OwnerInstanceId,
                    p => p == _ownerId)
                .Build(),
            OnChangedResourceCount);
    }

    public bool Enabled => _view.gameObject.activeSelf;

    public void Initialize()
    {
        Hide();
    }

    public void Show()
    {
        if (Enabled)
            return;

        _view.gameObject.SetActive(true);

        if (_isDirty)
            UpdateView();
    }

    public void Hide()
    {
        if (Enabled == false)
            return;

        _view.gameObject.SetActive(false);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _subscription?.Dispose();
        _cachedState.Clear();

        if (_view != null && _view.gameObject != null)
        {
            GameObject.Destroy(_view.gameObject);
            _view = null;
        }
    }

    private void OnChangedResourceCount(ChangedResourceCountEvent @event)
    {
        _cachedState[@event.DeliverableType] = @event.TotalCount;
        _isDirty = true;

        if (Enabled)
            UpdateView();
    }

    private void UpdateView()
    {
        foreach (KeyValuePair<ItemDeliverableType, int> pair in _cachedState)
        {
            _view.ChangeResourceText(pair.Key, pair.Value);
        }

        _isDirty = false;
    }
}