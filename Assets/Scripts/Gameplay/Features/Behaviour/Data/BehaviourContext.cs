using System;
using System.Collections.Generic;

public class BehaviourContext : IDisposable
{
    private readonly Dictionary<Type, IBehaviourComponent> _components;
    private readonly List<IDisposable> _disposables;

    public BehaviourContext()
    {
        _components = new();
        _disposables = new();
    }

    public void Dispose()
    {
        Clear();
    }

    public TComponent GetComponent<TComponent>()
        where TComponent : class, IBehaviourComponent
    {
        if (_components.TryGetValue(typeof(TComponent), out IBehaviourComponent component))
            return component as TComponent;

        throw new InvalidOperationException(
            $"Component of type {typeof(TComponent).Name} not found in {nameof(BehaviourContext)}");
    }

    public bool TryGetComponent<TComponent>(out TComponent result)
        where TComponent : class, IBehaviourComponent
    {
        result = default;

        if (_components.TryGetValue(typeof(TComponent), out IBehaviourComponent component))
        {
            result = component as TComponent;
            return result != null;
        }

        return false;
    }

    public void SetData<TComponent>(TComponent component)
        where TComponent : class, IBehaviourComponent
    {
        Type type = typeof(TComponent);
        
        if (component == null)
        {
            RemoveComponent<TComponent>();
            return;
        }

        _components[type] = component;

        if (component is IDisposable disposable)
            _disposables.Add(disposable);
    }

    public bool RemoveComponent<TComponent>()
    {
        Type type = typeof(TComponent);

        if (_components.TryGetValue(type, out IBehaviourComponent component) == false)
            return false;

        if (component is IDisposable disposable)
        {
            disposable?.Dispose();
            _disposables.Remove(disposable);
        }

        _components.Remove(type);

        return true;
    }

    public bool HasComponent<TComponent>()
        where TComponent : class, IBehaviourComponent
    {
        return _components.ContainsKey(typeof(TComponent));
    }

    public void Clear()
    {
        foreach (IDisposable disposable in _disposables)
            disposable?.Dispose();

        _disposables.Clear();
        _components.Clear();
    }
}