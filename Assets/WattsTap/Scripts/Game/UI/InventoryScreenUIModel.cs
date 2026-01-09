using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Game.Player;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Game.UI
{
    public class InventoryScreenUIModel : UIBaseModel
    {
        public ReactiveProperty<int> HitsCurrent { get; private set; }
        public ReactiveProperty<int> HitsMax { get; private set; }
        public ReactiveProperty<int> CoinsPerTap { get; private set; }
        
        private IPlayerService _playerService;
        private ITapControllerService _tapController;

        public override void Initialize()
        {
            base.Initialize();
            
            HitsCurrent = new ReactiveProperty<int>(0);
            HitsMax = new ReactiveProperty<int>(0);
            CoinsPerTap = new ReactiveProperty<int>(1);
            
            _playerService = ServiceLocator.Get<IPlayerService>();
            _tapController = ServiceLocator.Get<ITapControllerService>();
            
            if (_tapController != null)
            {
                _tapController.OnHitsChanged += OnHitsChanged;
                _tapController.OnIncomeMultiplierChanged += OnIncomeMultiplierChanged;
                
                HitsCurrent.Value = _tapController.CurrentHits;
                HitsMax.Value = _tapController.MaxHits;
                RecalculateCoinsPerTap();
            }
            
            if (_playerService != null)
            {
                _playerService.OnPlayerDataChanged += OnPlayerDataChanged;
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

        private void OnPlayerDataChanged(PlayerData playerData)
        {
            RecalculateCoinsPerTap();
        }

        private void RecalculateCoinsPerTap()
        {
            if (_playerService == null) return;
            var baseIncome = _playerService.IncomePerTap;
            var multiplier = _playerService.IncomeMultiplier;
            var effective = Mathf.Max(0, Mathf.RoundToInt(baseIncome * multiplier));
            CoinsPerTap.Value = effective;
        }

        public override void Dispose()
        {
            if (_tapController != null)
            {
                _tapController.OnHitsChanged -= OnHitsChanged;
                _tapController.OnIncomeMultiplierChanged -= OnIncomeMultiplierChanged;
            }
            
            if (_playerService != null)
            {
                _playerService.OnPlayerDataChanged -= OnPlayerDataChanged;
            }
            
            HitsCurrent?.Dispose();
            HitsMax?.Dispose();
            CoinsPerTap?.Dispose();
            
            base.Dispose();
        }
    }
}

