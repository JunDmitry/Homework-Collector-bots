using UnityEngine;

public class ResourceTaskManagementPresenter : IPresenter
{
    private readonly int _ownerId;

    private ResourceTaskManagementView _view;
    private IEventSubscription _subscription;

    private bool _disposed;

    public ResourceTaskManagementPresenter(ResourceTaskManagementView view, IEventAggregator eventAggregator, int ownerId)
    {
        _view = view;
        _ownerId = ownerId;
        _subscription = eventAggregator.Subscribe(
            new SubscribeConditionBuilder<UpdatedCountInfoEvent>(
                new SourceTypeCondition<UpdatedCountInfoEvent, IOwnerEventSource<UpdatedCountInfoEvent>>())
                .AndSourceProperty(
                    s => ((IOwnerEventSource<UpdatedCountInfoEvent>)s).OwnerInstanceId,
                    p => p == _ownerId)
                .Build(),
            OnUpdatedCountInfo);
    }

    public bool Enabled => _view.gameObject.activeSelf;

    public void Initialize()
    {
        Hide();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _subscription?.Dispose();

        if (_view != null && _view.gameObject != null)
        {
            GameObject.Destroy(_view.gameObject);
            _view = null;
        }
    }

    public void Hide()
    {
        if (Enabled == false)
            return;

        _view.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (Enabled)
            return;

        _view.gameObject.SetActive(true);
    }

    private void OnUpdatedCountInfo(UpdatedCountInfoEvent countInfoEvent)
    {
        _view.ChangeTaskManagementInfo(countInfoEvent.TaskManagementInfo);
    }
}