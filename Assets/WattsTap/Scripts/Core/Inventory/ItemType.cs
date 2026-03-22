namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Types of items available in the game.
    /// Easily extendable by adding new enum values.
    /// </summary>
    public enum ItemType
    {
        None = 0,
        
        // Weapons
        Weapon = 100,
        
        // Armor / equipment types
        ArmorBody = 200,
        ArmorLegs = 201,
        ArmorArms = 202,
        ArmorHead = 203,

        // Server-aligned aliases (preferred for new code)
        Body = 200,
        Feet = 201,
        Arms = 202,
    }

    public static class ItemTypeExtensions
    {
        /// <summary>
        /// Convert server slot string (WEAPON, ARMS, BODY, FEET) to enum.
        /// </summary>
        public static ItemType FromServerSlot(string serverSlot)
        {
            if (string.IsNullOrEmpty(serverSlot)) return ItemType.None;
            switch (serverSlot.ToUpperInvariant())
            {
                case "WEAPON": return ItemType.Weapon;
                case "ARMS":   return ItemType.Arms;
                case "BODY":   return ItemType.Body;
                case "FEET":   return ItemType.Feet;
                default:       return ItemType.None;
            }
        }

        /// <summary>
        /// Convert enum to server slot string.
        /// </summary>
        public static string ToServerSlot(this ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon:    return "WEAPON";
                case ItemType.ArmorArms: return "ARMS";
                case ItemType.ArmorBody: return "BODY";
                case ItemType.ArmorLegs: return "FEET";
                default:                 return "";
            }
        }
    }
}
