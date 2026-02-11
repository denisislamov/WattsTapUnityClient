using System.Collections.Generic;
using WattsTap.Game.UI;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Core
{
    /// <summary>
    /// Interface for upgrades data service
    /// Aggregates data from UpgradesConfig for UI display
    /// </summary>
    public interface IUpgradesService : IService
    {
        /// <summary>
        /// Get all available upgrades for display in UI
        /// Returns data for the first level of each upgrade (no progression yet)
        /// </summary>
        List<UpgradeDisplayData> GetAllUpgradesForDisplay();
        
        /// <summary>
        /// Get upgrade display data for a specific upgrade type
        /// </summary>
        UpgradeDisplayData GetUpgradeDisplayData(UpgradeType upgradeType);
        
        /// <summary>
        /// Get the total count of available upgrades
        /// </summary>
        int GetUpgradesCount();
    }
}
