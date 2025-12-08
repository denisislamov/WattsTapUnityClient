using UnityEngine;

namespace WattsTap.Core
{
    /// <summary>
    /// Singleton MonoBehaviour for running coroutines from non-MonoBehaviour classes.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;
        
        /// <summary>
        /// Singleton instance of CoroutineRunner
        /// </summary>
        public static CoroutineRunner Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[CoroutineRunner] Instance requested after application quit. Returning null.");
                    return null;
                }
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Try to find existing instance
                        _instance = FindObjectOfType<CoroutineRunner>();
                        
                        if (_instance == null)
                        {
                            // Create new GameObject with CoroutineRunner
                            var go = new GameObject("[CoroutineRunner]");
                            _instance = go.AddComponent<CoroutineRunner>();
                            DontDestroyOnLoad(go);
                            
                            Debug.Log("<color=#00FFFF>[CoroutineRunner] Instance created</color>");
                        }
                    }
                    
                    return _instance;
                }
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}





