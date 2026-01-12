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
        
        // Armor types
        ArmorBody = 200,
        ArmorLegs = 201,
        ArmorArms = 202,
        ArmorHead = 203,
    }
}

