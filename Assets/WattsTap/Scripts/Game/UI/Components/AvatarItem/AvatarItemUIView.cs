using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Game.Avatars;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// View component for AvatarItem UI element.
    /// Manages visual representation and state-based GameObject visibility.
    /// </summary>
    public class AvatarItemUIView : MonoBehaviour
    {
        [Header("Avatar Settings")]
        [SerializeField] private string _avatarId;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Button _avatarButton;
        
        [Header("Cost Display")]
        [SerializeField] private TMP_Text _costText;
        
        [Header("State GameObjects")]
        [Tooltip("GameObjects to show when this is the current (equipped) avatar")]
        [SerializeField] private GameObject[] _currentStateObjects;
        
        [Tooltip("GameObjects to show when this avatar is newly selected")]
        [SerializeField] private GameObject[] _selectedStateObjects;
        
        [Tooltip("GameObjects to show when this avatar is available but not selected")]
        [SerializeField] private GameObject[] _unselectedStateObjects;
        
        [Tooltip("GameObjects to show when this avatar is locked")]
        [SerializeField] private GameObject[] _lockedStateObjects;
        
        private AvatarItemState _currentState = AvatarItemState.Unselected;
        
        /// <summary>
        /// Unique identifier for this avatar.
        /// </summary>
        public string AvatarId => _avatarId;
        
        /// <summary>
        /// The button component for handling clicks.
        /// </summary>
        public Button AvatarButton => _avatarButton;
        
        /// <summary>
        /// Current state of the avatar item.
        /// </summary>
        public AvatarItemState CurrentState => _currentState;
        
        /// <summary>
        /// Event fired when avatar is clicked.
        /// </summary>
        public event Action<AvatarItemUIView> OnAvatarClicked;
        
        private void Awake()
        {
            if (_avatarButton != null)
            {
                _avatarButton.onClick.AddListener(HandleButtonClick);
            }
        }
        
        private void OnDestroy()
        {
            if (_avatarButton != null)
            {
                _avatarButton.onClick.RemoveListener(HandleButtonClick);
            }
        }
        
        private void HandleButtonClick()
        {
            OnAvatarClicked?.Invoke(this);
        }
        
        /// <summary>
        /// Sets the avatar's unique identifier.
        /// </summary>
        public void SetAvatarId(string id)
        {
            _avatarId = id;
        }
        
        /// <summary>
        /// Sets the avatar's sprite image.
        /// </summary>
        public void SetAvatarSprite(Sprite sprite)
        {
            if (_avatarImage != null && sprite != null)
            {
                _avatarImage.sprite = sprite;
            }
        }
        
        /// <summary>
        /// Sets the state of the avatar item and updates GameObject visibility.
        /// </summary>
        public void SetState(AvatarItemState newState)
        {
            if (_currentState == newState)
            {
                return;
            }
            
            // Exit current state - disable current state objects
            SetStateObjectsActive(_currentState, false);
            
            // Enter new state - enable new state objects
            _currentState = newState;
            SetStateObjectsActive(_currentState, true);
        }
        
        /// <summary>
        /// Forces state update regardless of current state.
        /// Useful for initialization.
        /// </summary>
        public void ForceSetState(AvatarItemState state)
        {
            // Disable all state objects first
            SetStateObjectsActive(AvatarItemState.Current, false);
            SetStateObjectsActive(AvatarItemState.Unselected, false);
            SetStateObjectsActive(AvatarItemState.Locked, false);
            SetSelectedObjectsActive(false);
            
            // Enable only the target state
            _currentState = state;
            SetStateObjectsActive(_currentState, true);
            
            // Show selected objects if state is Selected
            if (state == AvatarItemState.Selected)
            {
                SetSelectedObjectsActive(true);
            }
        }
        
        private void SetStateObjectsActive(AvatarItemState state, bool active)
        {
            GameObject[] objects = GetStateObjects(state);
            
            if (objects == null)
            {
                return;
            }
            
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
        
        private GameObject[] GetStateObjects(AvatarItemState state)
        {
            return state switch
            {
                AvatarItemState.Current => _currentStateObjects,
                AvatarItemState.Unselected => _unselectedStateObjects,
                AvatarItemState.Locked => _lockedStateObjects,
                _ => null
            };
        }
        
        /// <summary>
        /// Shows or hides the selected visual overlay independently of avatar state.
        /// This allows showing selection highlight on any avatar, including locked ones.
        /// </summary>
        public void SetSelected(bool selected)
        {
            SetSelectedObjectsActive(selected);
        }
        
        private void SetSelectedObjectsActive(bool active)
        {
            if (_selectedStateObjects == null) return;
            
            foreach (var obj in _selectedStateObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
        
        /// <summary>
        /// Sets whether the button is interactable.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (_avatarButton != null)
            {
                _avatarButton.interactable = interactable;
            }
        }
        
        /// <summary>
        /// Sets the cost display based on unlock type and price.
        /// </summary>
        public void SetCost(AvatarUnlockType unlockType, long price, int requiredLevel)
        {
            if (_costText == null)
            {
                return;
            }
            
            switch (unlockType)
            {
                case AvatarUnlockType.Level:
                    _costText.text = $"{requiredLevel} lvl";
                    break;
                    
                case AvatarUnlockType.Coins:
                    _costText.text = FormatNumber(price);
                    break;
                    
                case AvatarUnlockType.BTN:
                    _costText.text = $"{price} BTN";
                    break;
                    
                case AvatarUnlockType.Free:
                default:
                    _costText.text = string.Empty;
                    break;
            }
        }
        
        /// <summary>
        /// Sets raw cost text directly.
        /// </summary>
        public void SetCostText(string text)
        {
            if (_costText != null)
            {
                _costText.text = text;
            }
        }
        
        /// <summary>
        /// Shows or hides the cost text.
        /// </summary>
        public void SetCostVisible(bool visible)
        {
            if (_costText != null)
            {
                _costText.gameObject.SetActive(visible);
            }
        }
        
        /// <summary>
        /// Sets the cost text opacity based on affordability.
        /// Full opacity (1.0) if affordable, 50% opacity (0.5) if not.
        /// </summary>
        public void SetCostAffordable(bool canAfford)
        {
            if (_costText == null)
            {
                return;
            }
            
            var color = _costText.color;
            color.a = canAfford ? 1f : 0.5f;
            _costText.color = color;
        }
        
        /// <summary>
        /// Formats a number with K/M abbreviations.
        /// Examples: 999 -> "999", 1500 -> "1.5K", 1000000 -> "1M", 1500000 -> "1.5M"
        /// </summary>
        private static string FormatNumber(long number)
        {
            if (number >= 1_000_000)
            {
                double millions = number / 1_000_000.0;
                return millions % 1 == 0 
                    ? $"{millions:0}M" 
                    : $"{millions:0.##}M";
            }
            
            if (number >= 1_000)
            {
                double thousands = number / 1_000.0;
                return thousands % 1 == 0 
                    ? $"{thousands:0}K" 
                    : $"{thousands:0.##}K";
            }
            
            return number.ToString();
        }
    }
}

