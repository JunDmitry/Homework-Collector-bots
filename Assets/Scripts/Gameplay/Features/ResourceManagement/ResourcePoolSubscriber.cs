using System;

public class ResourcePoolSubscriber
{
    private readonly Action<Resource> _onGetting;

    public ResourcePoolSubscriber(Action<Resource> onGetting = null)
    {
        _onGetting = onGetting;
    }

    public ISubscribeProvider CreateSubscribeProvider(ObjectPool<Resource> pool)
    {
        return MultipleSubscribeProvider.Combine(
            SubscribeProvider<ObjectPool<Resource>, Action<Resource>>.Create(
                pool,
                resource => resource.gameObject.SetActive(false),
                (source, handler) => source.Created += handler,
                (source, handler) => source.Created -= handler),
            SubscribeProvider<ObjectPool<Resource>, Action<Resource>>.Create(
                pool,
                resource =>
                {
                    resource.Collected += pool.Release;
                    _onGetting?.Invoke(resource);
                    resource.gameObject.SetActive(true);
                },
                (source, handler) => source.Getted += handler,
                (source, handler) => source.Getted -= handler),
            SubscribeProvider<ObjectPool<Resource>, Action<Resource>>.Create(
                pool,
                resource =>
                {
                    resource.Collected -= pool.Release;
                    resource.gameObject.SetActive(false);
                },
                (source, handler) => source.Released += handler,
                (source, handler) => source.Released -= handler)
            );
    }
}