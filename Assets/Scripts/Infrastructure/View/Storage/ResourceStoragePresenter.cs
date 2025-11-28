using System;
using UnityEngine;

public class ResourceStoragePresenter : IPresenter
{
    private readonly ISubscribeProvider _provider;

    private StorageView _view;
    private bool _disposed = false;

    public ResourceStoragePresenter(Storage<Resource> storage, StorageView view)
    {
        _view = view;

        _provider = SubscribeProvider<Storage<Resource>, Action<ItemDeliverableType, int>>.Create(
            storage,
            view.ChangeResourceText,
            (s, h) => s.ChangedResourceCount += h,
            (s, h) => s.ChangedResourceCount -= h);
    }

    public bool Enabled => _view.enabled;

    public void Initialize()
    {
        Hide();
    }

    public void Show()
    {
        if (Enabled)
            return;

        _provider.Subscribe();
        _view.enabled = true;
    }

    public void Hide()
    {
        if (Enabled == false)
            return;

        _provider.Unsubscribe();
        _view.enabled = false;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _provider.Dispose();

        if (_view != null && _view.gameObject != null)
        {
            GameObject.Destroy(_view.gameObject);
            _view = null;
        }
    }
}