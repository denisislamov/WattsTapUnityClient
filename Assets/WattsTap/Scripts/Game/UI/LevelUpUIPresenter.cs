using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class LevelUpUIPresenter : UIBasePresenter<LevelUpUIView, LevelUpUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            Model.Level.OnValueChanged += OnLevelChanged;
            Model.WattReward.OnValueChanged += OnRewardChanged;
            
            ServiceLocator.TryGet(out _hapticService);

            if (View.CollectButton != null)
            {
                View.CollectButton.onClick.AddListener(OnCollectClicked);
            }

            OnLevelChanged(Model.Level.Value);
            OnRewardChanged(Model.WattReward.Value);
        }

        private void OnCollectClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.LevelUpPopUp);
        }

        private void OnLevelChanged(int level)
        {
            View.SetLevelValue(level);
        }

        private void OnRewardChanged(long amount)
        {
            View.ShowWattReward(amount);
        }

        protected override void OnDispose()
        {
            if (Model != null)
            {
                Model.Level.OnValueChanged -= OnLevelChanged;
                Model.WattReward.OnValueChanged -= OnRewardChanged;
            }

            if (View?.CollectButton != null)
            {
                View.CollectButton.onClick.RemoveListener(OnCollectClicked);
            }
        }
    }
}
