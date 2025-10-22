using System.Collections.Generic;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core.Configs;

namespace WattsTap.Core.UI
{
    public class UIService : IUIService
    {
        private readonly Dictionary<string, List<IUIView>> _viewInstances = new();
        private readonly Transform _uiRoot;
        private readonly IUIHost _overlayHost;

        private UIConfig _config;
        public Transform UIRoot => _uiRoot;
        public IUIHost OverlayRoot => _overlayHost;
        
        private List<IUIHost> _hosts = new();
        
        public UIService(Transform uiRoot, IUIHost overlayHost)
        {
            _uiRoot = uiRoot;
            _overlayHost = overlayHost;
        }
        public int InitializationOrder { get; } = 100;
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }
            
            var configService = ServiceLocator.Get<IConfigService>();
            _config = configService.GetConfig<UIConfig>(ConfigsConstants.UIConfig);
            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            _config = null;
        }
        
        public void RegisterHost(IUIHost host)
        {
            if (!_hosts.Contains(host))
            {
                _hosts.Add(host);
            }
        }

        public void UnregisterHost(IUIHost host)
        {
            _hosts.Remove(host);
        }

        public IUIView Open(string id)
        {
            throw new System.NotImplementedException();
        }

        public IUIView Open(string id, IUIHost host = null)
        {
            GameObject prefab = _config.GetPrefab(id);
            if (!prefab)
            {
                Debug.LogError($"Prefab \"{id}\" not found.");
                return null;
            }

            var parent = host?.Root ?? _uiRoot;
            
            GameObject instance = Object.Instantiate(prefab, parent);
            IUIView view = instance.GetComponent<IUIView>();

            if (!_viewInstances.ContainsKey(id))
            {
                _viewInstances[id] = new List<IUIView>();
            }

            _viewInstances[id].Add(view);

            var presenter = view.CreatePresenter();
            presenter.Initialize(view);

            return view;
        }

        public void Close(string id)
        {
            if (!_viewInstances.TryGetValue(id, out var instance))
            {
                Debug.LogWarning($"No views found for ID: {id}");
                return;
            }

            foreach (var view in instance)
            {
                if (view is MonoBehaviour mb)
                {
                    Object.Destroy(mb.gameObject);
                }
            }

            _viewInstances[id].Clear();
        }

        public void Close(string id, int index)
        {
            if (!_viewInstances.TryGetValue(id, out var list))
            {
                Debug.LogWarning($"No views found for ID: {id}");
                return;
            }

            if (index < 0 || index >= list.Count)
            {
                return;
            }

            var view = list[index];
            if (view is MonoBehaviour mb)
            {
                Object.Destroy(mb.gameObject);
            }

            list.RemoveAt(index);
        }

        public void Close(IUIHost view)
        {
            throw new System.NotImplementedException();
        }

        public void Close(IUIView view)
        {
            if (view == null)
            {
                Debug.LogWarning("View is null, cannot close.");
                return;
            }

            foreach (var kvp in _viewInstances)
            {
                if (kvp.Value.Remove(view))
                {
                    if (view is MonoBehaviour mb)
                    {
                        Object.Destroy(mb.gameObject);
                    }
                    return;
                }
            }

            Debug.LogWarning("View not found in any registered instances.");
        }

        public void Show(string id)
        {
            if (!_viewInstances.TryGetValue(id, out var list))
            {
                return;
            }

            foreach (var view in list)
            {
                if (view is MonoBehaviour mb)
                {
                    mb.gameObject.SetActive(true);
                }
            }
        }

        public void Show(string id, int index)
        {
            if (!_viewInstances.TryGetValue(id, out var list))
            {
                return;
            }

            if (index < 0 || index >= list.Count)
            {
                return;
            }

            if (list[index] is MonoBehaviour mb)
            {
                mb.gameObject.SetActive(true);
            }
        }

        public void Hide(string id)
        {
            if (!_viewInstances.TryGetValue(id, out var list))
            {
                return;
            }

            foreach (var view in list)
            {
                if (view is MonoBehaviour mb)
                {
                    mb.gameObject.SetActive(false);
                }
            }
        }

        public void Hide(string id, int index)
        {
            if (!_viewInstances.TryGetValue(id, out var list))
            {
                return;
            }

            if (index < 0 || index >= list.Count)
            {
                return;
            }

            if (list[index] is MonoBehaviour mb)
            {
                mb.gameObject.SetActive(false);
            }
        }

        public IReadOnlyList<IUIView> GetViews(string id)
        {
            if (_viewInstances.TryGetValue(id, out var list))
            {
                return list;
            }

            return new List<IUIView>();
        }
    }
}
