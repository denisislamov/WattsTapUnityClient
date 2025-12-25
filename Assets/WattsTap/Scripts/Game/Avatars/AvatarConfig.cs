using UnityEngine;

namespace WattsTap.Game.Avatars
{
    /// <summary>
    /// ScriptableObject configuration for an avatar.
    /// </summary>
    [CreateAssetMenu(fileName = "AvatarConfig", menuName = "WattsTap/Configs/Avatars/AvatarConfig")]
    public class AvatarConfig : ScriptableObject
    {
        [Header("Avatar Info")]
        [SerializeField] private string _avatarId;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _avatarSprite;
        
        [Header("Availability")]
        [SerializeField] private bool _isUnlockedByDefault = false;
        [SerializeField] private string _unlockRequirement;
        
        /// <summary>
        /// Unique identifier for this avatar.
        /// </summary>
        public string AvatarId => _avatarId;
        
        /// <summary>
        /// Display name of the avatar.
        /// </summary>
        public string DisplayName => _displayName;
        
        /// <summary>
        /// Sprite to display for this avatar.
        /// </summary>
        public Sprite AvatarSprite => _avatarSprite;
        
        /// <summary>
        /// Whether this avatar is unlocked by default.
        /// </summary>
        public bool IsUnlockedByDefault => _isUnlockedByDefault;
        
        /// <summary>
        /// Description of what's required to unlock this avatar.
        /// </summary>
        public string UnlockRequirement => _unlockRequirement;
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_avatarId))
            {
                _avatarId = name;
            }
        }
    }
}

