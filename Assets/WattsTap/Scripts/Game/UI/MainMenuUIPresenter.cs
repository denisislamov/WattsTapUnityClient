using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class MainMenuUIPresenter : UIBasePresenter<MainMenuUIView, MainMenuUIModel>
    {
        protected override void OnInit()
        {
            // Подписка на изменения в модели
            Model.PlayerName.OnValueChanged += OnPlayerNameChanged;
            Model.PlayerLevel.OnValueChanged += OnPlayerLevelChanged;
            Model.IsLoading.OnValueChanged += OnLoadingStateChanged;
            Model.TotalCoins.OnValueChanged += OnTotalCoinsChanged;
            Model.CoinsPerTap.OnValueChanged += OnCoinsPerTapChanged;
            Model.HitsCurrent.OnValueChanged += OnHitsChanged;
            Model.HitsMax.OnValueChanged += OnHitsChanged;
            
            // Инициализация начальных значений
            OnTotalCoinsChanged(Model.TotalCoins.Value);
            OnCoinsPerTapChanged(Model.CoinsPerTap.Value);
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
        }

        private void OnPlayerNameChanged(string newName)
        {
            // Здесь можно обновить View или выполнить другую логику
        }

        private void OnPlayerLevelChanged(int newLevel)
        {
            // Здесь можно обновить View или выполнить другую логику
        }

        private void OnLoadingStateChanged(bool isLoading)
        {
            // Здесь можно обновить View или выполнить другую логику
        }

        private void OnTotalCoinsChanged(long totalCoins)
        {
            View.UpdateTotalCoins(totalCoins);
        }

        private void OnCoinsPerTapChanged(int coinsPerTap)
        {
            View.UpdateCoinsPerTap(coinsPerTap);
        }

        private void OnHitsChanged(int _)
        {
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
        }

        protected override void OnDispose()
        {
            // Отписка от событий модели
            if (Model != null)
            {
                Model.PlayerName.OnValueChanged -= OnPlayerNameChanged;
                Model.PlayerLevel.OnValueChanged -= OnPlayerLevelChanged;
                Model.IsLoading.OnValueChanged -= OnLoadingStateChanged;
                Model.TotalCoins.OnValueChanged -= OnTotalCoinsChanged;
                Model.CoinsPerTap.OnValueChanged -= OnCoinsPerTapChanged;
                Model.HitsCurrent.OnValueChanged -= OnHitsChanged;
                Model.HitsMax.OnValueChanged -= OnHitsChanged;
            }
        }
    }
}