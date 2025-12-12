using System;
using System.Collections.Generic;

public class BehaviourCancellationTokenSource : IDisposable
{
    private readonly List<BehaviourCancellationToken> _linkedTokens;

    private bool _disposed;

    public BehaviourCancellationTokenSource()
    {
        Token = new(this);
        _linkedTokens = new() { Token };
    }

    public BehaviourCancellationToken Token { get; }
    public bool Cancelled => _disposed;

    public void LinkToken(BehaviourCancellationToken token)
    {
        if (_disposed == false)
            _linkedTokens.Add(token);
    }

    public void Cancel()
    {
        if (_disposed)
            return;

        _linkedTokens.ForEach(token => token.RequestCancellation());
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Cancel();
        _linkedTokens.Clear();
    }
}