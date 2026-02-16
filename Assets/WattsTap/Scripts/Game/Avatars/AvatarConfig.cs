using UnityEngine;

namespace WattsTap.Game.Avatars
{
    /// <summary>
    /// Тип разблокировки аватара
    /// </summary>
    public enum AvatarUnlockType
    {
        /// <summary>Доступен бесплатно с определенного уровня</summary>
        Level,
        /// <summary>Покупается за монеты (Watts)</summary>
        Coins,
        /// <summary>Покупается за BTN токены (пока недоступно)</summary>
        BTN,
        /// <summary>Доступен сразу (по умолчанию)</summary>
        Free
    }
    
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
        
        [Header("Unlock Requirements")]
        [Tooltip("Тип разблокировки аватара")]
        [SerializeField] private AvatarUnlockType _unlockType = AvatarUnlockType.Free;
        
        [Tooltip("Уровень, необходимый для разблокировки (если UnlockType = Level или для покупки)")]
        [SerializeField] private int _requiredLevel = 1;
        
        [Tooltip("Цена в монетах (Watts) для покупки")]
        [SerializeField] private long _coinPrice = 0;
        
        [Tooltip("Цена в BTN токенах (пока недоступно)")]
        [SerializeField] private long _btnPrice = 0;
        
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
        
        /// <summary>
        /// Тип разблокировки аватара
        /// </summary>
        public AvatarUnlockType UnlockType => _unlockType;
        
        /// <summary>
        /// Минимальный уровень игрока для разблокировки/покупки
        /// </summary>
        public int RequiredLevel => _requiredLevel;
        
        /// <summary>
        /// Цена в монетах (Watts)
        /// </summary>
        public long CoinPrice => _coinPrice;
        
        /// <summary>
        /// Цена в BTN токенах (пока недоступно)
        /// </summary>
        public long BtnPrice => _btnPrice;
        
        /// <summary>
        /// Проверяет, доступен ли аватар для покупки/разблокировки при данном уровне
        /// </summary>
        public bool IsAvailableAtLevel(int playerLevel)
        {
            return playerLevel >= _requiredLevel;
        }
        
        /// <summary>
        /// Можно ли купить аватар за монеты
        /// </summary>
        public bool CanBuyWithCoins => _unlockType == AvatarUnlockType.Coins && _coinPrice > 0;
        
        /// <summary>
        /// Можно ли купить аватар за BTN (пока недоступно)
        /// </summary>
        public bool CanBuyWithBTN => _unlockType == AvatarUnlockType.BTN && _btnPrice > 0;
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_avatarId))
            {
                _avatarId = name;
            }
        }
    }
}

