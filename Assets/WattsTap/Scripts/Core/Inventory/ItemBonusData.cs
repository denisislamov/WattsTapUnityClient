using System;
using UnityEngine;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Bonus data attached to an item variant (e.g. "Crit Chance 1%").
    /// Matches server's CatalogBonusDTO structure.
    /// </summary>
    [Serializable]
    public class ItemBonusData
    {
        [Tooltip("Type of stat this bonus affects")]
        public StatType StatType;

        [Tooltip("Flat bonus value")]
        public float Value;

        [Tooltip("Percentage bonus value")]
        public float ValuePercent;

        [Tooltip("Human-readable description from server")]
        public string Description;
    }
}

