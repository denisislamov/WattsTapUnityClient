using WattsTap.Core;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Game.Player;

namespace WattsTap.Game.UI
{
    public class MainMenuUIModel : UIBaseModel
    {
        public ReactiveProperty<string> PlayerName { get; private set; }
        public ReactiveProperty<int> PlayerLevel { get; private set; }
        public ReactiveProperty<bool> IsLoading { get; private set; }
        public ReactiveProperty<long> TotalCoins { get; private set; }
        public ReactiveProperty<int> CoinsPerTap { get; private set; }

        private IPlayerService _playerService;

        public override void Initialize()
        {
            base.Initialize();
            
            PlayerName = new ReactiveProperty<string>("Player");
            PlayerLevel = new ReactiveProperty<int>(1);
            IsLoading = new ReactiveProperty<bool>(false);
            TotalCoins = new ReactiveProperty<long>(0);
            CoinsPerTap = new ReactiveProperty<int>(1);

            // Получаем сервис игрока
            _playerService = ServiceLocator.Get<IPlayerService>();
            
            if (_playerService != null)
            {
                // Подписываемся на изменения ресурсов
                _playerService.OnResourcesChanged += OnPlayerResourcesChanged;
                _playerService.OnPlayerDataChanged += OnPlayerDataChanged;
                
                // Инициализируем начальные значения
                UpdateFromPlayerData();
            }
        }

        private void OnPlayerResourcesChanged(PlayerResources resources)
        {
            TotalCoins.Value = resources.watts;
        }

        private void OnPlayerDataChanged(PlayerData playerData)
        {
            UpdateFromPlayerData();
        }

        private void UpdateFromPlayerData()
        {
            if (_playerService == null) return;
            
            var playerData = _playerService.GetPlayerData();
            if (playerData != null)
            {
                TotalCoins.Value = playerData.resources.watts;
                CoinsPerTap.Value = (int)playerData.stats.incomePerTap;
                PlayerName.Value = playerData.nickname;
                PlayerLevel.Value = playerData.level;
            }
        }

        public override void Dispose()
        {
            // Отписываемся от событий
            if (_playerService != null)
            {
                _playerService.OnResourcesChanged -= OnPlayerResourcesChanged;
                _playerService.OnPlayerDataChanged -= OnPlayerDataChanged;
            }

            PlayerName?.Dispose();
            PlayerLevel?.Dispose();
            IsLoading?.Dispose();
            TotalCoins?.Dispose();
            CoinsPerTap?.Dispose();
            
            base.Dispose();
        }
    }
}