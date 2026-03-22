using System;
using UnityEngine;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Data for a single level in an item's progression.
    /// Matches server's CatalogLevelDTO structure.
    /// </summary>
    [Serializable]
    public class ItemLevelData
    {
        [Tooltip("Level number (1-based)")]
        public int Level;

        [Tooltip("Flat stat value (null on server → 0 here)")]
        public float Value;

        [Tooltip("Percentage stat value (null on server → 0 here)")]
        public float ValuePercent;
    }
}

