using WattsTap.Core.React;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class MainMenuUIModel : UIBaseModel
    {
        public ReactiveProperty<string> PlayerName { get; private set; }
        public ReactiveProperty<int> PlayerLevel { get; private set; }
        public ReactiveProperty<bool> IsLoading { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            
            PlayerName = new ReactiveProperty<string>("Player");
            PlayerLevel = new ReactiveProperty<int>(1);
            IsLoading = new ReactiveProperty<bool>(false);
        }

        public override void Dispose()
        {
            PlayerName?.Dispose();
            PlayerLevel?.Dispose();
            IsLoading?.Dispose();
            
            base.Dispose();
        }
    }
}

