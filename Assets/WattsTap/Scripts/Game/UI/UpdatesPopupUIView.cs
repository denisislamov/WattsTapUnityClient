using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class UpdatesPopupUIView : UIBaseView<UpdatesPopupUIPresenter>
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Navigation")]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _friendsReferralButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _questsButton;
        
        [Header("Upgrades List")]
        [SerializeField] private Transform _upgradesListContainer;
        [SerializeField] private GameObject _upgradeItemPrefab;
        [SerializeField] private GameObject _noUpgradesPlaceholder;
        
        private List<UpdatesPopupListElementView> _upgradeItemViews = new List<UpdatesPopupListElementView>();
        
        public Button MainMenuButton => _mainMenuButton;
        public Button FriendsReferralButton => _friendsReferralButton;
        public Button InventoryButton => _inventoryButton;
        public Button QuestsButton => _questsButton;
        
        #region Skinning
        
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
        
        #endregion
        
        #region Upgrades List
        
        /// <summary>
        /// Populate the upgrades list with data
        /// </summary>
        public void PopulateUpgradesList(List<UpgradeDisplayData> upgrades)
        {
            Debug.Log($"[UpdatesPopupUIView] PopulateUpgradesList called with {upgrades?.Count ?? 0} items");
            Debug.Log($"[UpdatesPopupUIView] _upgradesListContainer is {(_upgradesListContainer != null ? "NOT null" : "null")}");
            Debug.Log($"[UpdatesPopupUIView] _upgradeItemPrefab is {(_upgradeItemPrefab != null ? "NOT null" : "null")}");
            
            // Clear existing items
            ClearUpgradesList();
            
            bool hasUpgrades = upgrades != null && upgrades.Count > 0;
            
            // Show/hide placeholder
            if (_noUpgradesPlaceholder != null)
            {
                _noUpgradesPlaceholder.SetActive(!hasUpgrades);
            }
            
            if (!hasUpgrades || _upgradeItemPrefab == null || _upgradesListContainer == null)
            {
                Debug.LogWarning($"[UpdatesPopupUIView] Early return: hasUpgrades={hasUpgrades}, prefab={_upgradeItemPrefab != null}, container={_upgradesListContainer != null}");
                return;
            }
            
            // Create upgrade items
            foreach (var upgradeData in upgrades)
            {
                var item = Instantiate(_upgradeItemPrefab, _upgradesListContainer);
                
                var upgradeItemView = item.GetComponent<UpdatesPopupListElementView>();
                if (upgradeItemView != null)
                {
                    upgradeItemView.Setup(upgradeData);
                    _upgradeItemViews.Add(upgradeItemView);
                }
            }
            
            Debug.Log($"[UpdatesPopupUIView] Created {_upgradeItemViews.Count} upgrade items");
        }
        
        /// <summary>
        /// Clear all upgrade items from the list
        /// </summary>
        public void ClearUpgradesList()
        {
            if (_upgradesListContainer != null)
            {
                // Collect children to destroy (iterate in reverse to avoid issues)
                var childCount = _upgradesListContainer.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    var child = _upgradesListContainer.GetChild(i);
                    if (Application.isPlaying)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
            
            _upgradeItemViews.Clear();
        }
        
        /// <summary>
        /// Get all upgrade item views
        /// </summary>
        public List<UpdatesPopupListElementView> GetUpgradeItemViews() => _upgradeItemViews;
        
        #endregion
    }
}

