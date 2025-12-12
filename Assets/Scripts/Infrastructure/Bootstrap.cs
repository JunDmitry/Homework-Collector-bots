using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour, ICoroutineRunner
{
    [SerializeField] private BaseStructure _baseStructure;
    [SerializeField] private TransportationWorker _baseWorkerPrefab;
    [SerializeField] private World _world;
    [SerializeField] private ResourceSpawnerConfig _resourceSpawnerConfig;
    [SerializeField] private InputReader _inputReader;

    [SerializeField] private CanvasGroup _panelGroup;
    [SerializeField] private StorageView _storageViewTemplate;
    [SerializeField] private ResourceTaskManagementView _resourceTaskManagementViewTemplate;

    private readonly List<IPresenter> _presenters = new();

    private AssetProvider _assetProvider;
    private StaticDataProvider _staticDataProvider;

    private ISingleParameterFactory<ObjectPool<Resource>, Resource> _resourcePoolFactory;
    private ITwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3> _workerFactory;

    private ResourceSpawner _resourceSpawner;

    private IntelligentEventAggregator _eventAggregator;
    private SubscriptionInstaller _subscriptionInstaller;

    private void Awake()
    {
        InitializeInfrastructure();
        InitializeInput();
        InitializeFactories();
        InitializeGameplay();
        InitializeBase();
        InitializeWorld();
    }

    private void OnEnable()
    {
        _baseStructure.EnableLogics();
        _world.BeginLifetime();

        _presenters.ForEach(p => p.Show());
    }

    private void OnDisable()
    {
        _baseStructure.DisableLogics();
        _world.StopLifetime();
        _presenters.ForEach(p => p.Hide());
    }

    private void OnDestroy()
    {
        _presenters.ForEach(p => p.Dispose());
        _subscriptionInstaller?.Dispose();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow * new Color(1, 1, 1, .1f);
        Gizmos.DrawSphere(transform.position, _baseStructure.ScanInfo.ScanRadius);
    }

    private void InitializeInfrastructure()
    {
        _assetProvider = new AssetProvider();
        _staticDataProvider = new StaticDataProvider();
        _eventAggregator = new();

        _staticDataProvider.LoadAll();
        _subscriptionInstaller = new SubscriptionInstaller(_eventAggregator);

        ISingleParameterFactory<IPresenter, Structure> structurePresenterFactory = new BaseStructurePresenterFactory(
            _panelGroup,
            _storageViewTemplate,
            _resourceTaskManagementViewTemplate,
            _eventAggregator,
            new ObjectInstantiator());
        _presenters.Add(new SelectedStructurePanelPresenter(_eventAggregator, structurePresenterFactory.Create));
    }

    private void InitializeInput()
    {
        _inputReader.Initialize(_eventAggregator);
    }

    private void InitializeFactories()
    {
        _resourcePoolFactory = new SingleParameterFactory<ObjectPool<Resource>, Resource>(r => new(() => Instantiate(r), _eventAggregator));
        _workerFactory = new TwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3>(
            (prefab, position) => Instantiate(prefab, position, Quaternion.identity));
    }

    private void InitializeGameplay()
    {
        _resourceSpawner = new ResourceSpawner(_resourceSpawnerConfig, this);
        _resourceSpawner.Initialize(_assetProvider, _resourcePoolFactory, _eventAggregator);
        _resourceSpawner.Enable();
    }

    private void InitializeBase()
    {
        BaseStructureInitializer initializer = new(_eventAggregator, this, _workerFactory);

        initializer.Initialize(_baseStructure);

        using FakeSource<UnitCreatedEvent<BaseStructure>> fakeSource = new(_eventAggregator);
        fakeSource.Invoke(new(_baseStructure));
    }

    private void InitializeWorld()
    {
        _world.Initialize(_resourceSpawner, _eventAggregator);
    }
}