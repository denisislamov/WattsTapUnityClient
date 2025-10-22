namespace WattsTap.Core.UI
{
    public interface IUIView
    {
        IUIViewPresenter CreatePresenter();
        IUIViewPresenter GetPresenter();
    }
}