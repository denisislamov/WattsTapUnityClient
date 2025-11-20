using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.Configs;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Game.Player;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.UI
{
    public class LevelUpUIModel : UIBaseModel
    {
        private const string PlayerLevelConfigId = "PlayerLevelConfig";

        public ReactiveProperty<int> Level { get; private set; }
        public ReactiveProperty<long> WattReward { get; private set; }

        private IPlayerService _playerService;
        private PlayerLevelConfig _levelConfig;

        public override void Initialize()
        {
            base.Initialize();

            Level = new ReactiveProperty<int>(1);
            WattReward = new ReactiveProperty<long>(0);

            ServiceLocator.TryGet(out _playerService);

            if (ServiceLocator.TryGet(out IConfigService configService))
            {
                _levelConfig = configService.GetConfig<PlayerLevelConfig>(PlayerLevelConfigId);
            }
            else
            {
                Debug.LogWarning("[LevelUpUIModel] Config service is not available.");
            }

            UpdateValues();

            if (_playerService != null)
            {
                _playerService.OnPlayerDataChanged += OnPlayerDataChanged;
            }
        }

        private void OnPlayerDataChanged(PlayerData _)
        {
            UpdateValues();
        }

        private void UpdateValues()
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

            if (_levelConfig != null)
            {
                var rewards = _levelConfig.GetRewardsForLevel(playerData.level);
                WattReward.Value = rewards?.coins ?? 0;
            }
            else
            {
                WattReward.Value = 0;
            }
        }

        public override void Dispose()
        {
            if (_playerService != null)
            {
                _playerService.OnPlayerDataChanged -= OnPlayerDataChanged;
            }

            Level?.Dispose();
            WattReward?.Dispose();

            base.Dispose();
        }
    }
}

