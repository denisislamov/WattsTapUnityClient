using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class LevelUpUIView : UIBaseView<LevelUpUIPresenter>
    {
        [Header("Level Info")]
        [SerializeField] private TMP_Text _levelValue;

        [Header("Rewards")]
        [SerializeField] private RewardSlot[] _rewardSlots;

        [Header("Controls")]
        [SerializeField] private Button _collectButton;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;

        public Button CollectButton => _collectButton;

        private void OnEnable()
        {
            _themeManager = ServiceLocator.Get<MainMenuThemeManager>();
            
            if (_themeManager != null)
            {
                _themeManager.SkinChanged += OnSkinChanged;

                if (_themeManager.CurrentSkin != null)
                {
                    _themeManager.ApplySkin(_themeManager.CurrentSkin, _skinBindings, this);
                    return;
                }
                
                _themeManager.ApplySkin(null, _skinBindings, this);
            }
        }

        private void OnDisable()
        {
            if (_themeManager != null)
            {
                _themeManager.SkinChanged -= OnSkinChanged;
            }
        }
        
        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
            {
                _themeManager.ApplySkin(skin, _skinBindings, this);
            }
        }

        public void SetLevelValue(int level)
        {
            if (_levelValue != null)
            {
                _levelValue.text = level.ToString();
            }
        }

        public void ShowWattReward(long rewardAmount)
        {
            if (_rewardSlots == null || _rewardSlots.Length == 0)
            {
                return;
            }

            for (int i = 0; i < _rewardSlots.Length; i++)
            {
                bool shouldShow = i == 0 && rewardAmount > 0;
                _rewardSlots[i]?.ApplyState(shouldShow, rewardAmount);
            }
        }

        [Serializable]
        private class RewardSlot
        {
            [SerializeField] private GameObject root;
            [SerializeField] private TMP_Text rewardValue;

            public void ApplyState(bool active, long amount)
            {
                if (root != null)
                {
                    root.SetActive(active);
                }

                if (!active)
                {
                    return;
                }

                if (rewardValue == null && root != null)
                {
                    rewardValue = root.GetComponentInChildren<TMP_Text>(true);
                }

                if (rewardValue != null)
                {
                    rewardValue.text = $"+{amount:N0}";
                }
            }
        }
    }
}

