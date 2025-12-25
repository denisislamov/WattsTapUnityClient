namespace WattsTap.Game.UI
{
    /// <summary>
    /// Represents the possible states of an avatar item.
    /// </summary>
    public enum AvatarItemState
    {
        /// <summary>
        /// The avatar is currently equipped/active.
        /// </summary>
        Current,
        
        /// <summary>
        /// The avatar is newly selected (but not yet confirmed).
        /// </summary>
        Selected,
        
        /// <summary>
        /// The avatar is available but not selected.
        /// </summary>
        Unselected,
        
        /// <summary>
        /// The avatar is locked and not available.
        /// </summary>
        Locked
    }
}

