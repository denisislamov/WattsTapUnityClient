using System.Collections.Generic;
using WattsTap.Core.React;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class AvatarScreenUIModel : UIBaseModel
    {
        public ReactiveProperty<string> CurrentAvatarId { get; private set; }
        public ReactiveProperty<string> SelectedAvatarId { get; private set; }
        public List<AvatarItemUIPresenter> AvatarPresenters { get; private set; }
        
        public override void Initialize()
        {
            base.Initialize();
            
            CurrentAvatarId = new ReactiveProperty<string>(string.Empty);
            SelectedAvatarId = new ReactiveProperty<string>(string.Empty);
            AvatarPresenters = new List<AvatarItemUIPresenter>();
        }
        
        public override void Dispose()
        {
            CurrentAvatarId?.Dispose();
            SelectedAvatarId?.Dispose();
            
            if (AvatarPresenters != null)
            {
                foreach (var presenter in AvatarPresenters)
                {
                    presenter?.Dispose();
                }
                AvatarPresenters.Clear();
            }
            
            base.Dispose();
        }
    }
}

