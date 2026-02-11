using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core.Configs;
using WattsTap.Game.UI;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Core.Services
{
    /// <summary>
    /// Service that aggregates upgrade data from UpgradesConfig for UI display
    /// Currently returns first level data only (no progression)
    /// </summary>
    public class UpgradesService : IUpgradesService
    {
        private UpgradesConfig _upgradesConfig;
        private List<UpgradeDisplayData> _cachedDisplayData;
        
        #region IService Implementation
        
        public int InitializationOrder => 60;
        public bool IsInitialized { get; private set; }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            // Get config from ConfigService
            if (ServiceLocator.TryGet<IConfigService>(out var configService))
            {
                try
                {
                    _upgradesConfig = configService.GetConfig<UpgradesConfig>();
                    Debug.Log($"<color=#00FFFF>[UpgradesService] Loaded UpgradesConfig with {_upgradesConfig?.skills?.Count ?? 0} skills</color>");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[UpgradesService] Failed to load UpgradesConfig: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("[UpgradesService] IConfigService not found!");
            }
            
            // Pre-cache the display data
            CacheDisplayData();
            
            IsInitialized = true;
            Debug.Log("<color=#00FFFF>[UpgradesService] Initialized</color>");
        }
        
        public void Shutdown()
        {
            _upgradesConfig = null;
            _cachedDisplayData = null;
            IsInitialized = false;
        }
        
        #endregion
        
        #region IUpgradesService Implementation
        
        public List<UpgradeDisplayData> GetAllUpgradesForDisplay()
        {
            if (_cachedDisplayData == null)
            {
                CacheDisplayData();
            }
            
            return _cachedDisplayData;
        }
        
        public UpgradeDisplayData GetUpgradeDisplayData(UpgradeType upgradeType)
        {
            if (_upgradesConfig == null)
            {
                Debug.LogWarning("[UpgradesService] UpgradesConfig is null");
                return null;
            }
            
            var skillData = _upgradesConfig.GetSkillData(upgradeType);
            if (skillData == null)
            {
                Debug.LogWarning($"[UpgradesService] Skill data not found for {upgradeType}");
                return null;
            }
            
            return CreateDisplayData(skillData);
        }
        
        public int GetUpgradesCount()
        {
            return _upgradesConfig?.skills?.Count ?? 0;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Cache all display data for quick access
        /// </summary>
        private void CacheDisplayData()
        {
            _cachedDisplayData = new List<UpgradeDisplayData>();
            
            if (_upgradesConfig?.skills == null)
            {
                Debug.LogWarning("[UpgradesService] No skills found in UpgradesConfig");
                return;
            }
            
            foreach (var skill in _upgradesConfig.skills)
            {
                if (skill == null) continue;
                
                var displayData = CreateDisplayData(skill);
                if (displayData != null)
                {
                    _cachedDisplayData.Add(displayData);
                }
            }
            
            Debug.Log($"[UpgradesService] Cached {_cachedDisplayData.Count} upgrade display items");
        }
        
        /// <summary>
        /// Create display data from skill data
        /// Currently uses first level only (no progression)
        /// </summary>
        private UpgradeDisplayData CreateDisplayData(UpgradeSkillData skillData)
        {
            if (skillData == null) return null;
            
            // Get first level data (level 1)
            const int currentLevel = 1;
            var levelData = skillData.GetLevel(currentLevel);
            
            // Get next level price (for level 2)
            long nextLevelPrice = 0;
            var nextLevelData = skillData.GetLevel(currentLevel + 1);
            if (nextLevelData != null)
            {
                nextLevelPrice = nextLevelData.price;
            }
            
            return new UpgradeDisplayData
            {
                UpgradeType = skillData.upgradeType,
                Title = skillData.title,
                Description = skillData.description,
                Icon = skillData.icon,
                CurrentLevel = currentLevel,
                MaxLevel = skillData.levels?.Count ?? 0,
                CurrentParameterValue = levelData?.parameter ?? skillData.startParameter,
                NextLevelPrice = nextLevelPrice
            };
        }
        
        #endregion
    }
}
