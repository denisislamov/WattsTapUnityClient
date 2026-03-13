using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopBoosterItemUIModelAnimation : UIBaseModel
    {
        public ShopBoosterItemConfig Config { get; private set; }

        public void SetConfig(ShopBoosterItemConfig config)
        {
            Config = config;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Dispose()
        {
            Config = null;
            base.Dispose();
        }
    }
}

