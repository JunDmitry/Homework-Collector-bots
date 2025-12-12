using UnityEngine;

public class BaseStructurePresenterFactory : ISingleParameterFactory<IPresenter, Structure>
{
    private readonly CanvasGroup _groupPrefab;
    private readonly StorageView _storageViewPrefab;
    private readonly ResourceTaskManagementView _resourceTaskManagementViewPrefab;
    private readonly IEventAggregator _eventAggregator;
    private readonly IInstantiator _instantiator;

    public BaseStructurePresenterFactory(
        CanvasGroup group,
        StorageView storageView,
        ResourceTaskManagementView resourceTaskManagementView,
        IEventAggregator eventAggregator,
        IInstantiator instantiator)
    {
        _groupPrefab = group;
        _storageViewPrefab = storageView;
        _resourceTaskManagementViewPrefab = resourceTaskManagementView;
        _eventAggregator = eventAggregator;
        _instantiator = instantiator;
    }

    public IPresenter Create(Structure structure)
    {
        CanvasGroup group = _instantiator.Instantiate(_groupPrefab, structure.transform);
        StorageView storageView = _instantiator.Instantiate(_storageViewPrefab, group.transform);
        ResourceTaskManagementView taskManagementView = _instantiator.Instantiate(_resourceTaskManagementViewPrefab, group.transform);

        int id = structure.GetInstanceID();
        ResourceStoragePresenter storagePresenter = new(storageView, _eventAggregator, id);
        ResourceTaskManagementPresenter taskManagementPresenter = new(taskManagementView, _eventAggregator, id);

        BaseStructurePresenter baseStructurePresenter = new(group, taskManagementPresenter, storagePresenter);
        baseStructurePresenter.Initialize();

        return baseStructurePresenter;
    }
}