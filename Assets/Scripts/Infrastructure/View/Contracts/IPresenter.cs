using System;

public interface IPresenter : IDisposable
{
    bool Enabled { get; }

    void Hide();

    void Show();
}