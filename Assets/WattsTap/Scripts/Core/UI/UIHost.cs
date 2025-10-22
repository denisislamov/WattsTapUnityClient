using UnityEngine;

namespace WattsTap.Core.UI
{
    public class UIHost : MonoBehaviour, IUIHost
    {
        public Transform Root => transform;
    }
}