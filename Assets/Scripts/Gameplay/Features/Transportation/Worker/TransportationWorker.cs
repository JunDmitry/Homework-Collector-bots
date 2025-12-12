using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TransportationWorker : MonoBehaviour, ITransportationWorker<Resource>, ICoroutineRunner
{
    [SerializeField] private GrabPoint _grabPoint;
    [SerializeField] private WorkerAnimator _workerAnimator;
    [SerializeField] private TransportationWorkerEventHandler _transportationEventHandler;

    private WorkerMover _mover;
    private Baggage<Resource> _resourceBaggage;
    private ISubscribeProvider _moverSubscribeProvider;

    private IEnumerator _moveCoroutine;

    private void Awake()
    {
        _mover = new(GetComponent<NavMeshAgent>(), _workerAnimator, this);
        _resourceBaggage = new(
            _transportationEventHandler,
            r =>
            {
                r.transform.SetParent(_grabPoint.transform);
                r.transform.localPosition = Vector3.zero;
            },
            r =>
            {
                r.transform.SetParent(null);
            });
        _moverSubscribeProvider = CreateMoverProvider();
    }

    private void OnEnable()
    {
        _moverSubscribeProvider?.Subscribe();
        _resourceBaggage.Enable();
    }

    private void OnDisable()
    {
        _moverSubscribeProvider?.Unsubscribe();
        _resourceBaggage.Disable();
    }

    private void OnDestroy()
    {
        _moverSubscribeProvider?.Dispose();
        _resourceBaggage?.Dispose();
    }

    public IEnumerator MoveTo(Vector3 targetPosition, float minDistance = .1f)
    {
        do
        {
            _moveCoroutine = _mover.MoveTo(targetPosition, minDistance);

            yield return _moveCoroutine;
        }
        while (_mover.IsSuccessfulMove == false);
    }

    public bool CanLoad()
    {
        return _resourceBaggage.IsEmpty;
    }

    public IEnumerator MakeLoad(Resource resource, Action<Resource> loadAction = null)
    {
        ThrowIf.Invalid(CanLoad() == false, $"{nameof(resource)} can't be loaded because the {nameof(_resourceBaggage)} is already full");

        _resourceBaggage.Put(resource);
        _workerAnimator.SetGrab();

        yield return _workerAnimator.WaitWhileGrabbing();

        loadAction?.Invoke(resource);
    }

    public bool CanShipment()
    {
        return _resourceBaggage.IsEmpty == false;
    }

    public IEnumerator MakeShipment(Action<Resource> shipmentAction = null)
    {
        ThrowIf.Invalid(CanShipment() == false, $"{nameof(TransportationWorker)} {nameof(_resourceBaggage)} should be not empty to {nameof(MakeShipment)}");

        _workerAnimator.SetBring();
        yield return _workerAnimator.WaitWhileBringing();

        Resource resource = _resourceBaggage.Take();
        shipmentAction?.Invoke(resource);
    }

    private ISubscribeProvider CreateMoverProvider()
    {
        return MultipleSubscribeProvider.Combine(
            SubscribeProvider<WorkerMover, Action>.Create(
                _mover,
                () =>
                {
                    if (_moveCoroutine == null)
                        return;

                    StopCoroutine(_moveCoroutine);
                    _moveCoroutine = null;
                },
                (s, h) => s.StoppedMovement += h,
                (s, h) => s.StoppedMovement -= h),
            SubscribeProvider<WorkerMover, Action>.Create(
                _mover,
                () =>
                {
                    _moveCoroutine = null;
                },
                (s, h) => s.CompletedMovement += h,
                (s, h) => s.CompletedMovement -= h)
            );
    }
}