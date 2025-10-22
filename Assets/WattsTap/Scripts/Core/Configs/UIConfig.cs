using UnityEngine;

namespace WattsTap.Core.Configs
{
    [CreateAssetMenu(menuName = "HNConfigs/UI Config")]
    public class UIConfig : BaseConfig
    {
        [System.Serializable]
        public class UIEntry
        {
            public string Id;
            public GameObject Prefab;
        }

        public UIEntry[] Entries;

        public GameObject GetPrefab(string id)
        {
            foreach (var entry in Entries)
                if (entry.Id == id)
                    return entry.Prefab;

            Debug.LogError($"UI Prefab not found for ID: {id}");
            return null;
        }
    }
}