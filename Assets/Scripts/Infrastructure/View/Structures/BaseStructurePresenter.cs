using System.Collections.Generic;
using UnityEngine;

public class BaseStructurePresenter : IPresenter
{
    private readonly List<IPresenter> _presenters;
    private readonly CanvasGroup _group;

    public BaseStructurePresenter(
        CanvasGroup group,
        ResourceTaskManagementPresenter taskManagementPresenter,
        ResourceStoragePresenter resourceStoragePresenter)
    {
        _presenters = new()
        {
            taskManagementPresenter,
            resourceStoragePresenter
        };
        _group = group;
    }

    public bool Enabled => _group.gameObject.activeSelf;

    public void Initialize()
    {
        Hide();
    }

    public void Dispose()
    {
        _presenters.ForEach(p => p?.Dispose());
        _presenters.Clear();

        if (_group != null && _group.gameObject != null)
            Object.Destroy(_group.gameObject);
    }

    public void Hide()
    {
        if (Enabled == false)
            return;

        _group.gameObject.SetActive(false);
        _group.alpha = 0;
        _presenters.ForEach(p => p?.Hide());
    }

    public void Show()
    {
        if (Enabled)
            return;

        _group.gameObject.SetActive(true);
        _group.alpha = 1;
        _presenters.ForEach(p => p?.Show());
    }
}