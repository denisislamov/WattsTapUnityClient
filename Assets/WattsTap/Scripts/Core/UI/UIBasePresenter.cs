namespace WattsTap.Core.UI
{
    public abstract class UIBasePresenter<TView, TModel> : IUIViewPresenter 
        where TView : IUIView
        where TModel : UIBaseModel, new()
    {
        protected TView View;
        protected TModel Model;

        public void Initialize(IUIView view)
        {
            View = (TView)view;
            Model = new TModel();
            Model.Initialize();
            OnInit();
        }

        protected abstract void OnInit();
        
        public virtual void Dispose()
        {
            Model?.Dispose();
            OnDispose();
        }

        protected abstract void OnDispose();
    }
}