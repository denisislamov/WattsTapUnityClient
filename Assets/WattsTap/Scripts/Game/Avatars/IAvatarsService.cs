using System;
using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Game.Avatars
{
    /// <summary>
    /// Результат покупки аватара
    /// </summary>
    public enum AvatarPurchaseResult
    {
        /// <summary>Покупка успешна</summary>
        Success,
        /// <summary>Аватар уже разблокирован</summary>
        AlreadyUnlocked,
        /// <summary>Недостаточно монет</summary>
        NotEnoughCoins,
        /// <summary>Недостаточно BTN токенов</summary>
        NotEnoughBTN,
        /// <summary>Уровень игрока недостаточен</summary>
        LevelTooLow,
        /// <summary>Аватар не существует</summary>
        AvatarNotFound,
        /// <summary>Покупка за BTN недоступна</summary>
        BTNPurchaseNotAvailable,
        /// <summary>Неизвестная ошибка</summary>
        Error
    }
    
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
        /// Event fired when an avatar is purchased/unlocked.
        /// Provides the avatar ID and result.
        /// </summary>
        event Action<string, AvatarPurchaseResult> OnAvatarPurchased;
        
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
        
        /// <summary>
        /// Проверяет, можно ли купить аватар за монеты при текущем уровне игрока
        /// </summary>
        bool CanPurchaseAvatarWithCoins(string avatarId, int playerLevel, long playerCoins);
        
        /// <summary>
        /// Проверяет, можно ли разблокировать аватар по уровню
        /// </summary>
        bool CanUnlockByLevel(string avatarId, int playerLevel);
        
        /// <summary>
        /// Покупает аватар за монеты (Watts)
        /// </summary>
        AvatarPurchaseResult PurchaseAvatarWithCoins(string avatarId);
        
        /// <summary>
        /// Покупает аватар за BTN токены (пока недоступно)
        /// </summary>
        AvatarPurchaseResult PurchaseAvatarWithBTN(string avatarId);
        
        /// <summary>
        /// Разблокирует аватар по достижении уровня (бесплатно)
        /// </summary>
        AvatarPurchaseResult UnlockAvatarByLevel(string avatarId);
        
        /// <summary>
        /// Получить список разблокированных аватаров
        /// </summary>
        IReadOnlyCollection<string> GetUnlockedAvatars();
        
        /// <summary>
        /// Принудительно разблокировать аватар (для тестов или наград)
        /// </summary>
        void UnlockAvatar(string avatarId);
    }
}

