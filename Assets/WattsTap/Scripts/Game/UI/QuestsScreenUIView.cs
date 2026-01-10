using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class QuestsScreenUIView : UIBaseView<QuestsScreenUIPresenter>
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Navigation")]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _friendsReferralButton;
        [SerializeField] private Button _inventoryButton;
        
        [Header("Quest List")]
        [SerializeField] private Transform _questListContainer;
        [SerializeField] private GameObject _questItemPrefab;
        [SerializeField] private GameObject _noQuestsPlaceholder;
        
        public Button MainMenuButton => _mainMenuButton;
        public Button FriendsReferralButton => _friendsReferralButton;
        public Button InventoryButton => _inventoryButton;
        public Transform QuestListContainer => _questListContainer;
        public GameObject QuestItemPrefab => _questItemPrefab;
        
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
        
        /// <summary>
        /// Shows or hides the no quests placeholder
        /// </summary>
        public void SetNoQuestsPlaceholderActive(bool active)
        {
            if (_noQuestsPlaceholder != null)
            {
                _noQuestsPlaceholder.SetActive(active);
            }
        }
        
        /// <summary>
        /// Clears the quest list container
        /// </summary>
        public void ClearQuestList()
        {
            if (_questListContainer != null)
            {
                foreach (Transform child in _questListContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}

