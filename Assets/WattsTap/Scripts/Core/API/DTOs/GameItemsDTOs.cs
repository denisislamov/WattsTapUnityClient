using System;
using System.Collections.Generic;

namespace WattsTap.Core.API
{
    // ──────────────────────────────────────────────
    //  Catalog DTOs  (GET /game/items/catalog)
    // ──────────────────────────────────────────────

    [Serializable]
    public class CatalogResponse
    {
        public List<CatalogItemDTO> items;
    }

    [Serializable]
    public class CatalogItemDTO
    {
        public string id;
        public string code;
        public string name;
        public string description;
        public string slot;
        public List<CatalogVariantDTO> variants;
    }

    [Serializable]
    public class CatalogVariantDTO
    {
        public string id;
        public string rarity;
        public int maxLevel;
        public string mainStatType;
        public List<CatalogLevelDTO> levels;
        public List<CatalogBonusDTO> bonuses;
    }

    [Serializable]
    public class CatalogLevelDTO
    {
        public int level;
        /// <summary>Flat value. Server sends null for some stats → defaults to 0.</summary>
        public float value;
        /// <summary>Percentage value. Server sends null for some stats → defaults to 0.</summary>
        public float valuePercent;
    }

    [Serializable]
    public class CatalogBonusDTO
    {
        public string statType;
        public float value;
        public float valuePercent;
        public string description;
    }

    // ──────────────────────────────────────────────
    //  Inventory DTOs  (GET /game/inventory)
    // ──────────────────────────────────────────────

    [Serializable]
    public class InventoryResponse
    {
        public List<PlayerInventoryItemDTO> inventory;
        public EquipmentSlotsDTO equipment;
        public EquippedStatsMapDTO equippedStats;
        public CurrenciesDTO currencies;
    }

    [Serializable]
    public class PlayerInventoryItemDTO
    {
        public string id;            // Player-item instance id
        public string itemId;        // Template item id
        public string variantId;     // Variant id (rarity-specific)
        public int level;
        public string slot;
        public string rarity;
        public string name;
        public string code;
        public string mainStatType;
        public bool isEquipped;
    }

    [Serializable]
    public class EquipmentSlotsDTO
    {
        public string WEAPON;
        public string ARMS;
        public string BODY;
        public string FEET;
    }

    [Serializable]
    public class EquippedStatsMapDTO
    {
        public StatValueDTO COINS_PER_TAP;
        public StatValueDTO XP_PER_TAP;
        public StatValueDTO CAPACITY_HITS;
        public StatValueDTO RECOVER_HITS_PER_SECOND;
        public StatValueDTO CRIT_CHANCE;
        public StatValueDTO CRIT_MULTIPLIER;
        public StatValueDTO OFFLINE_BONUS_PERCENT;
    }

    [Serializable]
    public class StatValueDTO
    {
        public float value;
        public float valuePercent;
    }

    [Serializable]
    public class CurrenciesDTO
    {
        public long coins;
        public long drawings;
    }

    // ──────────────────────────────────────────────
    //  Equip / Unequip / Upgrade DTOs
    // ──────────────────────────────────────────────

    [Serializable]
    public class EquipItemRequest
    {
        public string playerItemId;
    }

    [Serializable]
    public class UnequipItemRequest
    {
        public string slot;
    }

    [Serializable]
    public class UpgradeItemRequest
    {
        public string playerItemId;
    }

    [Serializable]
    public class EquipItemResponse
    {
        public bool success;
        public InventoryResponse inventory;
    }

    [Serializable]
    public class UnequipItemResponse
    {
        public bool success;
        public InventoryResponse inventory;
    }

    [Serializable]
    public class UpgradeItemResponse
    {
        public bool success;
        public PlayerInventoryItemDTO item;
        public CurrenciesDTO currencies;
    }
}

