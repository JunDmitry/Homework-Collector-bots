using UnityEngine;

public class World : MonoBehaviour
{
    private ResourceSpawner _resourceSpawner;

    private bool _isBeginLifetime = false;

    private void OnDestroy()
    {
        _resourceSpawner?.Disable();
        _resourceSpawner?.Clear();
    }

    public void RestartLifetime()
    {
        StopLifetime();
        BeginLifetime();
    }

    public void BeginLifetime()
    {
        ThrowIf.Invalid(_isBeginLifetime, $"{nameof(World)} was been already {nameof(BeginLifetime)}");

        _isBeginLifetime = true;

        _resourceSpawner.Enable();
    }

    public void StopLifetime()
    {
        _isBeginLifetime = false;
        _resourceSpawner.Disable();
    }

    public void Initialize(ResourceSpawner resourceSpawner)
    {
        ThrowIf.Null(resourceSpawner, nameof(resourceSpawner));

        _resourceSpawner = resourceSpawner;

        if (_isBeginLifetime)
            _resourceSpawner.Enable();
    }
}