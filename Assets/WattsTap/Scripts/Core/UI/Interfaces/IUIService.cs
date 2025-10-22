using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Core.UI
{
    public interface IUIService :  IService
    {
        public void RegisterHost(IUIHost host);
        public void UnregisterHost(IUIHost host);
        IUIView Open(string id, IUIHost host = null);
        void Close(string id);
        void Close(string id, int index);
        void Close(IUIView view);

        void Show(string id);
        void Show(string id, int index);

        void Hide(string id);
        void Hide(string id, int index);

        public Transform UIRoot { get; }
        public IUIHost OverlayRoot { get; }

        IReadOnlyList<IUIView> GetViews(string id);
    }
}