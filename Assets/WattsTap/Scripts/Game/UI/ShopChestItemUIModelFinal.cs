using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopChestItemUIModelFinal : UIBaseModel
    {
        public ShopChestItemConfig Config { get; private set; }

        public void SetConfig(ShopChestItemConfig config)
        {
            Config = config;
        }

        public override void Dispose()
        {
            Config = null;
            base.Dispose();
        }
    }
}
