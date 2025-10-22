using System;

namespace WattsTap.Core.UI
{
    public interface IUIViewPresenter : IDisposable
    {
        void Initialize(IUIView view);
    }
}