using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Game.Player;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Game.UI
{
    public class MainMenuUIModel : UIBaseModel
    {
        public ReactiveProperty<string> PlayerName { get; private set; }
        public ReactiveProperty<int> PlayerLevel { get; private set; }
        public ReactiveProperty<bool> IsLoading { get; private set; }
        public ReactiveProperty<long> TotalCoins { get; private set; }
        public ReactiveProperty<int> CoinsPerTap { get; private set; }
        public ReactiveProperty<int> HitsCurrent { get; private set; }
        public ReactiveProperty<int> HitsMax { get; private set; }
        
        public ReactiveProperty<int> Level { get; private set; }
        public ReactiveProperty<long> CurrentXp { get; private set; }
        public ReactiveProperty<long> XpToNextLevel { get; private set; }
        
        private IPlayerService _playerService;
        private ITapControllerService _tapController;

        public override void Initialize()
        {
            base.Initialize();
            
            PlayerName = new ReactiveProperty<string>("Player");
            PlayerLevel = new ReactiveProperty<int>(1);
            IsLoading = new ReactiveProperty<bool>(false);
            TotalCoins = new ReactiveProperty<long>(0);
            CoinsPerTap = new ReactiveProperty<int>(1);
            HitsCurrent = new ReactiveProperty<int>(0);
            HitsMax = new ReactiveProperty<int>(0);
            Level = new ReactiveProperty<int>(1);
            CurrentXp = new ReactiveProperty<long>(0);
            XpToNextLevel = new ReactiveProperty<long>(100);
            
            // Получаем сервис игрока
            _playerService = ServiceLocator.Get<IPlayerService>();
            _tapController = ServiceLocator.Get<ITapControllerService>();
            
            if (_playerService != null)
            {
                _playerService.OnResourcesChanged += OnPlayerResourcesChanged;
                _playerService.OnPlayerDataChanged += OnPlayerDataChanged;
                _playerService.OnLevelUp += OnLevelUp;
                
                UpdateFromPlayerData();
            }

            if (_tapController != null)
            {
                _tapController.OnHitsChanged += OnHitsChanged;
                _tapController.OnIncomeMultiplierChanged += OnIncomeMultiplierChanged;
                // Инициализируем текущие значения хитов
                HitsCurrent.Value = _tapController.CurrentHits;
                HitsMax.Value = _tapController.MaxHits;
                // Инициализируем CoinsPerTap с учётом множителя
                RecalculateCoinsPerTap();
            }
        }

        private void OnLevelUp(int value)
        {
            Level.Value = value;
        }

        private void OnPlayerResourcesChanged(PlayerResources resources)
        {
            TotalCoins.Value = resources.watts;
            CurrentXp.Value = resources.currentXP;
            XpToNextLevel.Value = resources.xpToNextLevel;
        }

        private void OnPlayerDataChanged(PlayerData playerData)
        {
            UpdateFromPlayerData();
            // База дохода за тап могла измениться — пересчитаем отображение
            RecalculateCoinsPerTap();
        }

        private void UpdateFromPlayerData()
        {
            if (_playerService == null) return;
            
            var playerData = _playerService.GetPlayerData();
            if (playerData != null)
            {
                TotalCoins.Value = playerData.resources.watts;
                // CoinsPerTap будет рассчитываться отдельно с учётом множителя
                // PlayerName.Value = playerData.nickname;
                PlayerLevel.Value = playerData.level;
                CurrentXp.Value = playerData.resources.currentXP;
                XpToNextLevel.Value = playerData.resources.xpToNextLevel;
            }
        }

        private void OnHitsChanged(int current, int max)
        {
            HitsCurrent.Value = current;
            HitsMax.Value = max;
        }

        private void OnIncomeMultiplierChanged(float multiplier)
        {
            RecalculateCoinsPerTap();
        }

        private void RecalculateCoinsPerTap()
        {
            if (_playerService == null) return;
            var baseIncome = (int)_playerService.GetPlayerData().stats.incomePerTap;
            var multiplier = _tapController != null ? _tapController.IncomeMultiplier : 1f;
            var effective = Mathf.Max(0, Mathf.RoundToInt(baseIncome * multiplier));
            CoinsPerTap.Value = effective;
        }

        public override void Dispose()
        {
            // Отписываемся от событий
            if (_playerService != null)
            {
                _playerService.OnResourcesChanged -= OnPlayerResourcesChanged;
                _playerService.OnPlayerDataChanged -= OnPlayerDataChanged;
                _playerService.OnLevelUp -= OnLevelUp;
            }

            if (_tapController != null)
            {
                _tapController.OnHitsChanged -= OnHitsChanged;
                _tapController.OnIncomeMultiplierChanged -= OnIncomeMultiplierChanged;
            }

            PlayerName?.Dispose();
            PlayerLevel?.Dispose();
            IsLoading?.Dispose();
            TotalCoins?.Dispose();
            CoinsPerTap?.Dispose();
            HitsCurrent?.Dispose();
            HitsMax?.Dispose();
            
            base.Dispose();
        }
    }
}