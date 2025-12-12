using System;
using System.Collections.Generic;

public class BehaviourCancellationToken
{
    private readonly List<Action> _callbacks;
    private readonly object _lock = new object();

    private BehaviourCancellationTokenSource _childSource = null;
    private bool _isCancellationRequest;
    private int _nonInterraptableDepth;

    public BehaviourCancellationToken(BehaviourCancellationTokenSource source)
    {
        Source = source;
        _callbacks = new();
    }

    public bool IsCancellationRequested => _isCancellationRequest;
    public bool CanInterrupt => _nonInterraptableDepth == 0;
    public BehaviourCancellationTokenSource Source { get; }

    public void RegisterCallback(Action callback)
    {
        lock (_lock)
        {
            if (_isCancellationRequest)
                callback?.Invoke();
            else
                _callbacks.Add(callback);
        }
    }

    public void RequestCancellation()
    {
        lock (_lock)
        {
            if (_isCancellationRequest)
                return;

            _isCancellationRequest = true;
            RequestCancellationChild();

            for (int i = _callbacks.Count - 1; i >= 0; i--)
                _callbacks[i]?.Invoke();

            _callbacks.Clear();
        }
    }

    public void EnterNonInterruptableState()
    {
        lock (_lock)
        {
            _nonInterraptableDepth++;
        }
    }

    public BehaviourCancellationToken CreateLinkedToken()
    {
        lock (_lock)
        {
            BehaviourCancellationToken childToken;

            if (_childSource == null || _childSource.Cancelled)
            {
                _childSource = new();
                childToken = _childSource.Token;
            }
            else
            {
                childToken = new(_childSource);
                _childSource.LinkToken(childToken);
            }

            return childToken;
        }
    }

    public void RequestCancellationChild()
    {
        lock (_lock)
        {
            if (_isCancellationRequest || _childSource == null || _childSource.Cancelled)
                return;

            _childSource.Cancel();
            _childSource.Dispose();
        }
    }
}