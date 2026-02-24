using System.Collections.Generic;
using WattsTap.Core;
using WattsTap.Core.Inventory;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class MergeScreenUIModel : UIBaseModel
    {
        private IInventoryService _inventoryService;

        public override void Initialize()
        {
            base.Initialize();
            ServiceLocator.TryGet(out _inventoryService);
        }

        /// <summary>
        /// Get all inventory items.
        /// </summary>
        public IReadOnlyList<InventoryItem> GetAllItems()
        {
            return _inventoryService?.Items ?? new List<InventoryItem>();
        }

        public override void Dispose()
        {
            _inventoryService = null;
            base.Dispose();
        }
    }
}

