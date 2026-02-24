using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.UI.Components.Inventory;

namespace WattsTap.Game.UI
{
    public class MergeScreenUIPresenter : UIBasePresenter<MergeScreenUIView, MergeScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            // Populate inventory
            PopulateInventory();

            // Subscribe to view events
            View.OnItemClicked += OnItemClicked;

            if (View.BackButton != null)
            {
                View.BackButton.onClick.AddListener(OnBackButtonClicked);
            }

            if (View.InventoryButton != null)
            {
                View.InventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            }
        }

        private void PopulateInventory()
        {
            var items = Model.GetAllItems();
            View.PopulateInventory(items);
        }

        private void OnItemClicked(InventoryItemElementView itemView)
        {
            _hapticService?.ButtonPressed();
        }

        private void OnBackButtonClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.MergeScreen);
            _uiService?.Open(UIConstants.InventoryScreen);
        }

        private void OnInventoryButtonClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.MergeScreen);
            _uiService?.Open(UIConstants.InventoryScreen);
        }

        protected override void OnDispose()
        {
            if (View != null)
            {
                View.OnItemClicked -= OnItemClicked;
            }

            if (View?.BackButton != null)
            {
                View.BackButton.onClick.RemoveListener(OnBackButtonClicked);
            }

            if (View?.InventoryButton != null)
            {
                View.InventoryButton.onClick.RemoveListener(OnInventoryButtonClicked);
            }
        }
    }
}

