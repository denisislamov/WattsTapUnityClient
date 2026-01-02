using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopScreenUIView : UIBaseView<ShopScreenUIPresenter>
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button _chestsButton;
        [SerializeField] private Button _boostersButton;

        [Header("Tab Content")]
        [SerializeField] private GameObject[] _chestsPanelElements;
        [SerializeField] private GameObject[] _boostersPanelElements;

        [Header("Controls")]
        [SerializeField] private Button[] _closeButtons;

        [Header("Chest Items")]
        [SerializeField] private Button[] _chestItemOpenButtons;
        [SerializeField] private ShopChestItemConfig[] _chestItemConfigs;

        [Header("Booster Items")]
        [SerializeField] private Button[] _boosterItemOpenButtons;
        [SerializeField] private ShopBoosterItemConfig[] _boosterItemConfigs;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;

        public Button ChestsButton => _chestsButton;
        public Button BoostersButton => _boostersButton;
        public Button[] CloseButtons => _closeButtons;
        public Button[] ChestItemOpenButtons => _chestItemOpenButtons;
        public ShopChestItemConfig[] ChestItemConfigs => _chestItemConfigs;
        public Button[] BoosterItemOpenButtons => _boosterItemOpenButtons;
        public ShopBoosterItemConfig[] BoosterItemConfigs => _boosterItemConfigs;

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

        /// <summary>
        /// Shows chests tab content and hides boosters content
        /// </summary>
        public void ShowChestsTab()
        {
            SetElementsActive(_chestsPanelElements, true);
            SetElementsActive(_boostersPanelElements, false);
        }

        /// <summary>
        /// Shows boosters tab content and hides chests content
        /// </summary>
        public void ShowBoostersTab()
        {
            SetElementsActive(_chestsPanelElements, false);
            SetElementsActive(_boostersPanelElements, true);
        }

        private static void SetElementsActive(GameObject[] elements, bool isActive)
        {
            if (elements == null)
            {
                return;
            }

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] != null)
                {
                    elements[i].SetActive(isActive);
                }
            }
        }
    }
}
