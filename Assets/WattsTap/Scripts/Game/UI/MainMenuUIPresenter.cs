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

        protected override void OnDispose()
        {
            // Отписка от событий модели
            if (Model != null)
            {
                Model.PlayerName.OnValueChanged -= OnPlayerNameChanged;
                Model.PlayerLevel.OnValueChanged -= OnPlayerLevelChanged;
                Model.IsLoading.OnValueChanged -= OnLoadingStateChanged;
            }
        }
    }
}