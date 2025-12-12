using System;
using UnityEngine;

public class InputReader : MonoBehaviour, IEventSource<LeftMouseClickEvent>
{
    private const string Fire1 = nameof(Fire1);

    private IEventAggregator _eventAggregator;

    private event Action<LeftMouseClickEvent> _leftClickEvent;

    event Action<LeftMouseClickEvent> IEventSource<LeftMouseClickEvent>.EventRaised
    {
        add => _leftClickEvent += value;
        remove => _leftClickEvent -= value;
    }

    bool IEventSource<LeftMouseClickEvent>.IsActive => enabled;

    private void OnEnable()
    {
        _eventAggregator?.RegisterSource(this);
    }

    private void OnDisable()
    {
        _eventAggregator?.UnregisterSource(this);
    }

    private void Update()
    {
        if (Input.GetButtonDown(Fire1))
        {
            Vector3 mouseScreenPoint = Input.mousePosition;
            _leftClickEvent?.Invoke(new(mouseScreenPoint));
        }
    }

    public void Initialize(IEventAggregator eventAggregator)
    {
        ThrowIf.Null(eventAggregator, nameof(eventAggregator));

        _eventAggregator = eventAggregator;

        if (enabled)
            _eventAggregator.RegisterSource(this);
    }
}