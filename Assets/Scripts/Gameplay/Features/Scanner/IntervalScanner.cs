using System;
using System.Collections;
using UnityEngine;

public class IntervalScanner<T>
    where T : Component
{
    private readonly IScanner _scanner;
    private readonly WaitForSeconds _wait;
    private readonly Func<Vector3> _getPosition;
    private readonly ICoroutineRunner _coroutineStarter;
    private readonly int _layerToScan;

    private Coroutine _scanRoutine = null;

    public IntervalScanner(IScanner scanner, float interval, Func<Vector3> getPosition, ICoroutineRunner coroutineStarter, int layerToScan = -1)
    {
        ThrowIf.Null(scanner, nameof(scanner));
        ThrowIf.Argument(interval <= 0, $"{nameof(IScanner)} {nameof(interval)} must be positive", nameof(interval));
        ThrowIf.Null(getPosition, nameof(getPosition));

        _scanner = scanner;
        _wait = new(interval);
        _getPosition = getPosition;
        _coroutineStarter = coroutineStarter;
        _layerToScan = layerToScan;
    }

    public event Action<T> FoundedItem;

    public bool IsScanning { get; private set; } = false;

    public void BeginScan()
    {
        ThrowIf.Invalid(IsScanning, $"{nameof(IntervalScanner<T>)} already work");

        _scanRoutine = _coroutineStarter?.StartCoroutine(Scan());
    }

    public void RestartScan()
    {
        StopScan();
        BeginScan();
    }

    public void StopScan()
    {
        IsScanning = false;

        if (_scanRoutine != null)
        {
            _coroutineStarter?.StopCoroutine(_scanRoutine);
            _scanRoutine = null;
        }
    }

    private IEnumerator Scan()
    {
        IsScanning = true;

        while (IsScanning)
        {
            yield return _wait;

            _scanner.Scan<T>(_getPosition.Invoke(), _layerToScan)
                .ForEach(item => FoundedItem?.Invoke(item));
        }
    }
}