using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class AvatarScreenUIView : UIBaseView<AvatarScreenUIPresenter>
    {
        [Header("Controls")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _equipButton;
        
        [Header("Avatar Display")]
        [SerializeField] private Transform _avatarsContainer;
        [SerializeField] private AvatarItemUIView _avatarItemPrefab;

        public Button BackButton => _backButton;
        public Button EquipButton => _equipButton;
        public Transform AvatarsContainer => _avatarsContainer;
        public AvatarItemUIView AvatarItemPrefab => _avatarItemPrefab;
        
        /// <summary>
        /// Shows or hides the equip button based on selection state.
        /// </summary>
        public void SetEquipButtonVisible(bool visible)
        {
            if (_equipButton != null)
            {
                _equipButton.gameObject.SetActive(visible);
            }
        }
        
        /// <summary>
        /// Sets the equip button interactable state.
        /// </summary>
        public void SetEquipButtonInteractable(bool interactable)
        {
            if (_equipButton != null)
            {
                _equipButton.interactable = interactable;
            }
        }
    }
}

