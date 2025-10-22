namespace WattsTap.Core.UI
{
    public abstract class UIBasePresenter<TView> : IUIViewPresenter where TView : IUIView
    {
        protected TView View;

        public void Initialize(IUIView view)
        {
            View = (TView)view;
            OnInit();
        }

        protected abstract void OnInit();
        public abstract void Dispose();
    }
}