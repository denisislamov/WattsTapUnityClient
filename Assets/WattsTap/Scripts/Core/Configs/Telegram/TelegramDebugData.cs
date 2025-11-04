using UnityEngine;

namespace WattsTap.Core.Configs.Telegram
{
    [CreateAssetMenu(fileName = "TelegramDebugData", menuName = "WattsTap/Configs/Telegram/TelegramDebugData")]
    public class TelegramDebugData : BaseConfig
    {
        [System.Serializable]
        public class Data
        {
            public string UserId;
            public string UserName;
            public string InitData;
        }

        public Data[] Entries;

        [Header("Index Settings")] [Tooltip("Index used in Editor mode")]
        public int EditorIndex = 0;

        [Tooltip("Index used in Build/Runtime mode")]
        public int UsageIndex = 0;

        private int CurrentIndex
        {
            get
            {
#if UNITY_EDITOR
                return EditorIndex;
#else
                return UsageIndex;
#endif
            }
        }

        public string UserId => Entries[CurrentIndex].UserId;
        public string UserName => Entries[CurrentIndex].UserName;
        public string InitData => Entries[CurrentIndex].InitData;
    }
}