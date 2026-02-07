using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class WelcomeBackUIView : UIBaseView<WelcomeBackUIPresenter>
    {
        [Header("Timer")]
        [SerializeField] private Slider _timerSlider;
        [SerializeField] private TMP_Text _timerText;

        [Header("Reward")]
        [SerializeField] private TMP_Text _valueText;

        [Header("Controls")]
        [SerializeField] private Button _closeButton;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;

        public Button CloseButton => _closeButton;

        public void SetTimerProgress(float progress)
        {
            if (_timerSlider != null)
            {
                _timerSlider.value = Mathf.Clamp01(progress);
            }
        }

        public void SetTimerText(TimeSpan remainingTime)
        {
            if (_timerText != null)
            {
                _timerText.text = FormatTime(remainingTime);
            }
        }

        public void SetRewardValue(long amount)
        {
            if (_valueText != null)
            {
                _valueText.text = amount.ToString("N0");
            }
        }

        private static string FormatTime(TimeSpan time)
        {
            if (time < TimeSpan.Zero)
            {
                time = TimeSpan.Zero;
            }

            int totalHours = Mathf.FloorToInt((float)time.TotalHours);
            return $"{totalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
        }
        
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
    }
}

