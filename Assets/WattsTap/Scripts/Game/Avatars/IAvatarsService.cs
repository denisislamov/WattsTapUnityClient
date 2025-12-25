using System;
using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Game.Avatars
{
    /// <summary>
    /// Interface for the Avatars Service.
    /// </summary>
    public interface IAvatarsService : WattsTap.Core.IService
    {
        /// <summary>
        /// Event fired when the current avatar is changed.
        /// Provides the new avatar ID.
        /// </summary>
        event Action<string> OnAvatarChanged;
        
        /// <summary>
        /// Event fired when the Telegram avatar is loaded.
        /// Provides the loaded sprite.
        /// </summary>
        event Action<Sprite> OnTelegramAvatarLoaded;
        
        /// <summary>
        /// Gets all available avatar configurations.
        /// </summary>
        IReadOnlyList<AvatarConfig> GetAllAvatarConfigs();
        
        /// <summary>
        /// Gets the configuration for a specific avatar.
        /// </summary>
        AvatarConfig GetAvatarConfig(string avatarId);
        
        /// <summary>
        /// Gets the currently selected avatar ID.
        /// </summary>
        string GetCurrentAvatarId();
        
        /// <summary>
        /// Gets the sprite for the currently selected avatar.
        /// </summary>
        Sprite GetCurrentAvatarSprite();
        
        /// <summary>
        /// Selects a new avatar.
        /// </summary>
        /// <param name="avatarId">The ID of the avatar to select.</param>
        /// <returns>True if selection was successful.</returns>
        bool SelectAvatar(string avatarId);
        
        /// <summary>
        /// Checks if an avatar is unlocked.
        /// </summary>
        bool IsAvatarUnlocked(string avatarId);
        
        /// <summary>
        /// Gets the Telegram avatar sprite (loaded from user's profile photo).
        /// Returns default sprite if not loaded yet.
        /// </summary>
        Sprite GetTelegramAvatarSprite();
        
        /// <summary>
        /// Gets the default avatar ID (Telegram avatar).
        /// </summary>
        string TelegramAvatarId { get; }
        
        /// <summary>
        /// Returns true if the Telegram avatar has been loaded.
        /// </summary>
        bool IsTelegramAvatarLoaded { get; }
    }
}

