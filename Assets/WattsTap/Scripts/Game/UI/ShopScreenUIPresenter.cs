using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class ShopScreenUIPresenter : UIBasePresenter<ShopScreenUIView, ShopScreenUIModel>
    {
        private IUIService _uiService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);

            if (View.ChestsButton != null)
            {
                View.ChestsButton.onClick.AddListener(OnChestsButtonClicked);
            }

            if (View.BoostersButton != null)
            {
                View.BoostersButton.onClick.AddListener(OnBoostersButtonClicked);
            }

            foreach (var closeButton in View.CloseButtons)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
            
            // Show chests tab by default
            View.ShowChestsTab();
        }

        private void OnChestsButtonClicked()
        {
            View.ShowChestsTab();
        }

        private void OnBoostersButtonClicked()
        {
            View.ShowBoostersTab();
        }

        private void OnCloseButtonClicked()
        {
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.ShopScreen);
        }

        protected override void OnDispose()
        {
            if (View?.ChestsButton != null)
            {
                View.ChestsButton.onClick.RemoveListener(OnChestsButtonClicked);
            }

            if (View?.BoostersButton != null)
            {
                View.BoostersButton.onClick.RemoveListener(OnBoostersButtonClicked);
            }

            foreach (var closeButton in View.CloseButtons)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }
    }
}








