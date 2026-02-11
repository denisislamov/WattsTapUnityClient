using UnityEngine;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// Data transfer object for displaying upgrade information in UI
    /// </summary>
    public class UpgradeDisplayData
    {
        public UpgradeType UpgradeType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public float CurrentParameterValue { get; set; }
        public long NextLevelPrice { get; set; }
        public bool IsMaxLevel => CurrentLevel >= MaxLevel;
    }
}