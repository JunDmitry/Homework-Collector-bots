using UnityEngine;

public class World : MonoBehaviour
{
    private ResourceSpawner _resourceSpawner;
    private IEventAggregator _eventAggregator;

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

    public void Initialize(ResourceSpawner resourceSpawner, IEventAggregator eventAggregator)
    {
        ThrowIf.Null(resourceSpawner, nameof(resourceSpawner));
        ThrowIf.Null(eventAggregator, nameof(eventAggregator));

        _resourceSpawner = resourceSpawner;
        _eventAggregator = eventAggregator;

        if (_isBeginLifetime)
            _resourceSpawner.Enable();
    }

    private void OnDrawGizmosSelected()
    {
        if (_resourceSpawner == null
            || _resourceSpawner.SpawnArea == null)
            return;

        Vector3 spawnPosition = Vector3.zero;
        Gizmos.color = Color.red * new Color(1, 1, 1, .5f);

        foreach (Rectangle rectangle in _resourceSpawner.SpawnArea)
        {
            Bounds bounds = rectangle.Bounds;

            Gizmos.color = Color.blue * new Color(1, 1, 1, 0.5f);
            Gizmos.DrawCube(bounds.center + spawnPosition, bounds.size);

            Gizmos.color = Color.red;

            Vector3 topLeft3D = new(rectangle.TopLeft.x, 0, rectangle.TopLeft.y);
            Vector3 topRight3D = new(rectangle.TopRight.x, 0, rectangle.TopRight.y);
            Vector3 bottomLeft3D = new(rectangle.BottomLeft.x, 0, rectangle.BottomLeft.y);
            Vector3 bottomRight3D = new(rectangle.BottomRight.x, 0, rectangle.BottomRight.y);

            Gizmos.DrawLine(topLeft3D + spawnPosition, topRight3D + spawnPosition);
            Gizmos.DrawLine(topRight3D + spawnPosition, bottomRight3D + spawnPosition);
            Gizmos.DrawLine(bottomRight3D + spawnPosition, bottomLeft3D + spawnPosition);
            Gizmos.DrawLine(bottomLeft3D + spawnPosition, topLeft3D + spawnPosition);
        }
    }
}