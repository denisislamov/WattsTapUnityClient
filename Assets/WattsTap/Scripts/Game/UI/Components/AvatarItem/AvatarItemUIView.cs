using System;
using UnityEngine;
using UnityEngine.UI;

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
            SetStateObjectsActive(AvatarItemState.Selected, false);
            SetStateObjectsActive(AvatarItemState.Unselected, false);
            SetStateObjectsActive(AvatarItemState.Locked, false);
            
            // Enable only the target state
            _currentState = state;
            SetStateObjectsActive(_currentState, true);
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
                AvatarItemState.Selected => _selectedStateObjects,
                AvatarItemState.Unselected => _unselectedStateObjects,
                AvatarItemState.Locked => _lockedStateObjects,
                _ => null
            };
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
    }
}

