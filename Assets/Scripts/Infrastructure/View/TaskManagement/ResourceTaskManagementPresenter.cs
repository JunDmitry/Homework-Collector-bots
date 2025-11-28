using System;
using UnityEngine;

public class ResourceTaskManagementPresenter : IPresenter
{
    private readonly ISubscribeProvider _provider;

    private ResourceTaskManagementView _view;
    private bool _disposed;

    public ResourceTaskManagementPresenter(ResourceTaskManagementView view, TaskManagement<Resource> taskManagement)
    {
        _view = view;

        _provider = SubscribeProvider<TaskManagement<Resource>, Action<IReadonlyTaskManagementInfo>>.Create(
            taskManagement,
            _view.ChangeTaskManagementInfo,
            (s, h) => s.UpdatedCountInfo += h,
            (s, h) => s.UpdatedCountInfo -= h);
    }

    public bool Enabled => _view.enabled;

    public void Initialize()
    {
        Hide();
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

    public void Hide()
    {
        if (Enabled == false)
            return;

        _provider.Unsubscribe();
        _view.enabled = false;
    }

    public void Show()
    {
        if (Enabled)
            return;

        _provider.Subscribe();
        _view.enabled = true;
    }
}