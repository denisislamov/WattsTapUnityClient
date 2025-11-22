using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Game.Player;

namespace WattsTap.Game.UI
{
    public class ProfileScreenUIModel : UIBaseModel
    {
        public ReactiveProperty<string> PlayerName { get; private set; }
        public ReactiveProperty<int> Level { get; private set; }
        public ReactiveProperty<long> CurrentXp { get; private set; }
        public ReactiveProperty<long> XpToNextLevel { get; private set; }

        private IPlayerService _playerService;

        public override void Initialize()
        {
            base.Initialize();

            PlayerName = new ReactiveProperty<string>("Player");
            Level = new ReactiveProperty<int>(1);
            CurrentXp = new ReactiveProperty<long>(0);
            XpToNextLevel = new ReactiveProperty<long>(100);

            ServiceLocator.TryGet(out _playerService);

            if (_playerService != null)
            {
                _playerService.OnPlayerDataChanged += OnPlayerDataChanged;
                _playerService.OnResourcesChanged += OnResourcesChanged;
                _playerService.OnLevelUp += OnLevelUp;
                UpdateFromPlayerData();
            }
        }

        private void OnLevelUp(int level)
        {
            Level.Value = level;
        }

        private void OnPlayerDataChanged(PlayerData _)
        {
            UpdateFromPlayerData();
        }

        private void OnResourcesChanged(PlayerResources resources)
        {
            CurrentXp.Value = resources.currentXP;
            XpToNextLevel.Value = resources.xpToNextLevel;
        }

        private void UpdateFromPlayerData()
        {
            if (_playerService == null)
            {
                return;
            }

            var playerData = _playerService.GetPlayerData();
            if (playerData == null)
            {
                return;
            }

            Level.Value = playerData.level;
            CurrentXp.Value = playerData.resources.currentXP;
            XpToNextLevel.Value = playerData.resources.xpToNextLevel;
        }

        public override void Dispose()
        {
            if (_playerService != null)
            {
                _playerService.OnPlayerDataChanged -= OnPlayerDataChanged;
                _playerService.OnResourcesChanged -= OnResourcesChanged;
                _playerService.OnLevelUp -= OnLevelUp;
            }

            PlayerName?.Dispose();
            Level?.Dispose();
            CurrentXp?.Dispose();
            XpToNextLevel?.Dispose();

            base.Dispose();
        }
    }
}

