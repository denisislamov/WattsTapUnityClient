using UnityEngine;

namespace WattsTap.Core.UI
{
    public abstract class UIBaseView<TPresenter> : MonoBehaviour, IUIView
        where TPresenter : IUIViewPresenter, new()
    {
        private TPresenter _presenter;
        public IUIViewPresenter CreatePresenter()
        {
            _presenter = new TPresenter();
            return _presenter;
        }

        public IUIViewPresenter GetPresenter()
        {
            return _presenter;
        }

        protected virtual void OnDestroy()
        {
            _presenter?.Dispose();
        }
    }
}