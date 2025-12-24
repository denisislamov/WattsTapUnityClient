using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class QuestsScreenUIView : UIBaseView<QuestsScreenUIPresenter>
    {
        [Header("Navigation")]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _friendsReferralButton;
        
        [Header("Quest List")]
        [SerializeField] private Transform _questListContainer;
        [SerializeField] private GameObject _questItemPrefab;
        [SerializeField] private GameObject _noQuestsPlaceholder;
        
        #region Properties
        
        public Button MainMenuButton => _mainMenuButton;
        public Button FriendsReferralButton => _friendsReferralButton;
        public Transform QuestListContainer => _questListContainer;
        public GameObject QuestItemPrefab => _questItemPrefab;
        
        #endregion
        
        #region Public Methods
        
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
        
        #endregion
    }
}

