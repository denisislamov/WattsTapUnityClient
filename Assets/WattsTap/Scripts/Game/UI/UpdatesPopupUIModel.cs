using System.Collections.Generic;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class UpdatesPopupUIModel : UIBaseModel
    {
        private List<UpgradeDisplayData> _upgradesData;
        
        /// <summary>
        /// Current upgrades data for display
        /// </summary>
        public List<UpgradeDisplayData> UpgradesData => _upgradesData;
        
        /// <summary>
        /// Number of upgrades
        /// </summary>
        public int UpgradesCount => _upgradesData?.Count ?? 0;
        
        public override void Initialize()
        {
            base.Initialize();
            _upgradesData = new List<UpgradeDisplayData>();
        }

        public override void Dispose()
        {
            _upgradesData?.Clear();
            _upgradesData = null;
            base.Dispose();
        }
        
        /// <summary>
        /// Set upgrades data from service
        /// </summary>
        public void SetUpgradesData(List<UpgradeDisplayData> data)
        {
            _upgradesData = data ?? new List<UpgradeDisplayData>();
        }
    }
}
