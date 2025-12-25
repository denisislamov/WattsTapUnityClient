using UnityEngine;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// Model for AvatarItem, holds the data for a single avatar.
    /// </summary>
    public class AvatarItemUIModel
    {
        /// <summary>
        /// Unique identifier for this avatar.
        /// </summary>
        public string AvatarId { get; private set; }
        
        /// <summary>
        /// The sprite to display for this avatar.
        /// </summary>
        public Sprite AvatarSprite { get; private set; }
        
        /// <summary>
        /// Current state of the avatar.
        /// </summary>
        public AvatarItemState State { get; private set; }
        
        /// <summary>
        /// Whether this avatar is available for selection.
        /// </summary>
        public bool IsUnlocked => State != AvatarItemState.Locked;
        
        /// <summary>
        /// Creates a new AvatarItemUIModel with the specified parameters.
        /// </summary>
        public AvatarItemUIModel(string avatarId, Sprite sprite, AvatarItemState initialState = AvatarItemState.Unselected)
        {
            AvatarId = avatarId;
            AvatarSprite = sprite;
            State = initialState;
        }
        
        /// <summary>
        /// Updates the avatar's state.
        /// </summary>
        public void SetState(AvatarItemState newState)
        {
            State = newState;
        }
        
        /// <summary>
        /// Updates the avatar's sprite.
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            AvatarSprite = sprite;
        }
    }
}

