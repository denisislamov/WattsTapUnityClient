using System;
using System.Collections.Generic;

namespace WattsTap.Core.API
{
    // ──────────────────────────────────────────────
    //  Dev DTOs  (POST /dev/add-resources, etc.)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Request for POST /dev/add-resources (non-production).
    /// </summary>
    [Serializable]
    public class DevAddResourcesRequest
    {
        public float watts;
        public float xp;
    }

    /// <summary>
    /// Response from POST /dev/add-resources.
    /// </summary>
    [Serializable]
    public class DevAddResourcesResponse
    {
        public bool success;
        public PlayerProgressDTO progress;
        public int addedWatts;
        public int addedXp;
        public string message;
    }

    /// <summary>
    /// Request for POST /game/dev/inventory/grant.
    /// </summary>
    [Serializable]
    public class DevInventoryGrantRequest
    {
        public string itemVariantId;
        public int level;
    }

    /// <summary>
    /// Response from POST /game/dev/inventory/grant.
    /// </summary>
    [Serializable]
    public class DevInventoryGrantResponse
    {
        public string playerItemId;
        public string itemVariantId;
        public int level;
    }

    /// <summary>
    /// Request for POST /game/dev/boosters/grant.
    /// </summary>
    [Serializable]
    public class DevBoosterGrantRequest
    {
        public string code;
    }

    /// <summary>
    /// Response from POST /game/dev/boosters/grant.
    /// </summary>
    [Serializable]
    public class DevBoosterGrantResponse
    {
        public string playerBoosterId;
        public string boosterCode;
        public string boosterType;
        public string acquiredAt;
        public string consumedAt;
    }

    // ──────────────────────────────────────────────
    //  Boosters DTOs  (GET /game/boosters, etc.)
    // ──────────────────────────────────────────────

    [Serializable]
    public class BoosterCatalogItemDTO
    {
        public string id;
        public string code;
        public string type;
        public string title;
        public string description;
        /// <summary>Price as string: "100". Use int.Parse() when needed.</summary>
        public string price;
        public string paymentAssetType;
        public bool activated;
        public int sortOrder;
        // config is a dynamic object — skipped for JsonUtility
    }

    [Serializable]
    public class BoosterInventoryItemDTO
    {
        public string playerBoosterId;
        public string boosterCode;
        public string boosterType;
        public string acquiredAt;
        public string consumedAt;
    }

    [Serializable]
    public class BoosterActiveItemDTO
    {
        public string id;
        public string boosterCode;
        public string boosterType;
        public string startedAt;
        public string expiresAt;
        public int secondsLeft;
    }

    [Serializable]
    public class BoosterListResponse
    {
        public List<BoosterCatalogItemDTO> catalog;
        public List<BoosterInventoryItemDTO> inventory;
        public List<BoosterActiveItemDTO> active;
    }

    [Serializable]
    public class BoosterUseRequest
    {
        public string playerBoosterId;
    }

    [Serializable]
    public class BoosterUseResponse
    {
        public bool success;
        public string boosterType;
        public string consumedAt;
        // effect is a dynamic object
    }

    [Serializable]
    public class BoosterPurchaseRequest
    {
        public string boosterCode;
    }

    [Serializable]
    public class BoosterPurchaseResponse
    {
        public bool success;
        public string boosterCode;
        public string boosterType;
        public string status;
        public string nextAction;
        public string message;
        public ShopPaymentPayloadDTO payment;
    }

    // ──────────────────────────────────────────────
    //  Shop Payment DTO (shared by avatars, boosters, chests)
    // ──────────────────────────────────────────────

    [Serializable]
    public class ShopPaymentPayloadDTO
    {
        public string orderId;
        public string price;
        public string paymentAssetType;
        public string walletAddress;
        public string expiresAt;
        public string comment;
        public string paymentLink;
        public TonConnectArgsDTO tonConnectArgs;
    }

    [Serializable]
    public class TonConnectArgsDTO
    {
        public string address;
        public string amount;
        public string payload;
    }

    // ──────────────────────────────────────────────
    //  Chests DTOs  (GET /game/chests/catalog, etc.)
    // ──────────────────────────────────────────────

    [Serializable]
    public class ChestRarityChancesDTO
    {
        public float common;
        public float uncommon;
        public float rare;
        public float legendary;
    }

    [Serializable]
    public class ChestItemsPerOpenStatsDTO
    {
        public float min;
        public float max;
        public float avg;
    }

    [Serializable]
    public class ChestPreviewDTO
    {
        public string imageUrl;
        public string backgroundStyle;
    }

    [Serializable]
    public class ChestCatalogItemDTO
    {
        public string chestType;
        public string displayName;
        public int containsItemsMin;
        public int containsItemsMax;
        public int nextOpenItemCount;
        public int ownedCount;
        public bool canOpen;
        public bool canPurchase;
        public string price;
        public string paymentAssetType;
        public ChestRarityChancesDTO rarityChances;
        public ChestItemsPerOpenStatsDTO itemsPerOpenStats;
        public ChestPreviewDTO preview;
    }

    [Serializable]
    public class ChestCatalogResponse
    {
        public List<ChestCatalogItemDTO> chests;
    }

    [Serializable]
    public class ChestStateItemDTO
    {
        public string chestType;
        public string bagCode;
        public int slotIndex;
        public int opensInCurrentCycle;
    }

    [Serializable]
    public class ChestStateResponse
    {
        public List<ChestStateItemDTO> state;
    }

    [Serializable]
    public class ChestOpenRequest
    {
        public string chestType;
        public string requestId;
    }

    [Serializable]
    public class ChestOpenRewardDTO
    {
        public string itemVariantId;
        public string rarity;
        public int quantity;
        public int revealOrder;
    }

    [Serializable]
    public class ChestOpenNewProgressDTO
    {
        public string bagCode;
        public int slotIndex;
    }

    [Serializable]
    public class ChestOpenResponse
    {
        public string openId;
        public string chestType;
        public int configVersion;
        public string bagCode;
        public int slotIndex;
        public int remainingOwnedCount;
        public List<ChestOpenRewardDTO> rewards;
        public ChestOpenNewProgressDTO newProgress;
    }

    [Serializable]
    public class ChestPurchaseRequest
    {
        public string chestType;
        public int quantity;
        public string requestId;
    }

    [Serializable]
    public class ChestPurchaseResponse
    {
        public bool success;
        public string orderId;
        public string chestType;
        public int quantity;
        public string status;
        public string nextAction;
        public ShopPaymentPayloadDTO payment;
    }
}

