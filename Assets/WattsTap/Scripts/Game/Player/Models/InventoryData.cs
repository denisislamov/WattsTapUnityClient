using System;
using System.Collections.Generic;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Инвентарь игрока (предметы и их слоты)
    /// </summary>
    [Serializable]
    public class InventoryData
    {
        /// <summary>
        /// ID экипированного оружия
        /// </summary>
        public string equippedWeaponId;
        
        /// <summary>
        /// ID экипированного шлема
        /// </summary>
        public string equippedHelmetId;
        
        /// <summary>
        /// ID экипированной брони
        /// </summary>
        public string equippedArmorId;
        
        /// <summary>
        /// ID экипированной обуви
        /// </summary>
        public string equippedBootsId;
        
        /// <summary>
        /// Список всех предметов в инвентаре
        /// </summary>
        public List<InventoryItem> items;

        public InventoryData()
        {
            equippedWeaponId = string.Empty;
            equippedHelmetId = string.Empty;
            equippedArmorId = string.Empty;
            equippedBootsId = string.Empty;
            items = new List<InventoryItem>();
        }
    }

    /// <summary>
    /// Предмет в инвентаре
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        /// <summary>
        /// Уникальный ID предмета
        /// </summary>
        public string itemId;
        
        /// <summary>
        /// Тип предмета (Weapon, Helmet, Armor, Boots)
        /// </summary>
        public ItemType type;
        
        /// <summary>
        /// Редкость предмета
        /// </summary>
        public ItemRarity rarity;
        
        /// <summary>
        /// Уровень предмета
        /// </summary>
        public int level;
        
        /// <summary>
        /// Бонус к доходу за тап
        /// </summary>
        public long tapIncomeBonus;
        
        /// <summary>
        /// Бонус к пассивному доходу
        /// </summary>
        public long passiveIncomeBonus;
        
        /// <summary>
        /// Бонус к максимальной энергии
        /// </summary>
        public int energyBonus;

        public InventoryItem()
        {
            itemId = Guid.NewGuid().ToString();
            type = ItemType.Weapon;
            rarity = ItemRarity.Common;
            level = 1;
            tapIncomeBonus = 0;
            passiveIncomeBonus = 0;
            energyBonus = 0;
        }
    }

    /// <summary>
    /// Типы предметов
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Helmet,
        Armor,
        Boots
    }

    /// <summary>
    /// Редкость предметов
    /// </summary>
    public enum ItemRarity
    {
        Common,     // Обычный
        Uncommon,   // Необычный
        Rare,       // Редкий
        Epic,       // Эпический
        Legendary   // Легендарный
    }
}
