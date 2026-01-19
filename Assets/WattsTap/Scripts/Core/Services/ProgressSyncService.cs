using System;
using System.Collections;
using UnityEngine;
using WattsTap.Core.API;
using WattsTap.Core.Configs;

namespace WattsTap.Core.Services
{
    /// <summary>
    /// Interface for progress synchronization service
    /// </summary>
    public interface IProgressSyncService : IService
    {
        /// <summary>Whether sync is currently active</summary>
        bool IsSyncActive { get; }
        
        /// <summary>Last sync time</summary>
        DateTime LastSyncTime { get; }
        
        /// <summary>Interval between auto-syncs in seconds</summary>
        float SyncInterval { get; set; }
        
        /// <summary>Fired when progress is loaded from server</summary>
        event Action<LoadProgressResponse> OnProgressLoaded;
        
        /// <summary>Fired when progress is saved to server</summary>
        event Action<SaveProgressResponse> OnProgressSaved;
        
        /// <summary>Fired when progress is reset</summary>
        event Action<ResetProgressResponse> OnProgressReset;
        
        /// <summary>Fired on sync error</summary>
        event Action<string> OnSyncError;
        
        /// <summary>Start automatic sync</summary>
        void StartAutoSync();
        
        /// <summary>Stop automatic sync</summary>
        void StopAutoSync();
        
        /// <summary>Load progress from server once</summary>
        void LoadProgress();
        
        /// <summary>Save progress to server immediately</summary>
        void SaveProgressNow();
        
        /// <summary>Reset progress on server</summary>
        void ResetProgress();
        
        /// <summary>Set the current progress data for syncing</summary>
        void SetProgressData(int level, long watts, long currentXp, long totalXp);
    }
    
    /// <summary>
    /// Service for synchronizing player progress with server
    /// </summary>
    public class ProgressSyncService : IProgressSyncService
    {
        #region Private Fields
        
        private IReferralAPIService _apiService;
        private ProgressSyncConfig _config;
        private MonoBehaviour _coroutineRunner;
        private Coroutine _syncCoroutine;
        
        // Progress data to sync
        private int _level;
        private long _watts;
        private long _currentXp;
        private long _totalXp;
        
        // Dirty flag to track if data changed since last sync
        private bool _isDirty;
        
        // Sync settings
        private float _syncInterval = 5f; // Default 5 seconds
        private const float MinSyncInterval = 3f;
        private const float MaxSyncInterval = 30f;
        private bool _debugLogging = true;
        
        #endregion
        
        #region IService Implementation
        
        public int InitializationOrder => 60; // After ReferralAPIService
        public bool IsInitialized { get; private set; }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            if (!ServiceLocator.TryGet(out _apiService))
            {
                Debug.LogError("[ProgressSyncService] IReferralAPIService not found!");
                return;
            }
            
            // Load config
            if (ServiceLocator.TryGet<IConfigService>(out var configService))
            {
                _config = configService.GetConfig<ProgressSyncConfig>("ProgressSyncConfig");
                if (_config != null)
                {
                    _syncInterval = _config.SyncInterval;
                    _debugLogging = _config.DebugLogging;
                }
            }
            
            // Find a MonoBehaviour to run coroutines via IApplicationEntry
            if (ServiceLocator.TryGet<IApplicationEntry>(out var appEntry) && appEntry is MonoBehaviour mb)
            {
                _coroutineRunner = mb;
            }
            else
            {
                // Fallback: find any MonoBehaviour
                _coroutineRunner = UnityEngine.Object.FindObjectOfType<MonoBehaviour>();
            }
            
            if (_coroutineRunner == null)
            {
                Debug.LogError("[ProgressSyncService] No MonoBehaviour found to run coroutines!");
                return;
            }
            
            IsInitialized = true;
            if (_debugLogging)
                Debug.Log($"<color=#00FFFF>[ProgressSyncService] Initialized (interval: {_syncInterval}s)</color>");
        }
        
        public void Shutdown()
        {
            StopAutoSync();
            IsInitialized = false;
        }
        
        #endregion
        
        #region IProgressSyncService Properties
        
        public bool IsSyncActive => _syncCoroutine != null;
        public DateTime LastSyncTime { get; private set; }
        
        public float SyncInterval
        {
            get => _syncInterval;
            set => _syncInterval = Mathf.Clamp(value, MinSyncInterval, MaxSyncInterval);
        }
        
        #endregion
        
        #region Events
        
        public event Action<LoadProgressResponse> OnProgressLoaded;
        public event Action<SaveProgressResponse> OnProgressSaved;
        public event Action<ResetProgressResponse> OnProgressReset;
        public event Action<string> OnSyncError;
        
        #endregion
        
        #region Public Methods
        
        public void StartAutoSync()
        {
            if (!IsInitialized || _coroutineRunner == null)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot start auto sync - not initialized");
                return;
            }
            
            if (_syncCoroutine != null)
            {
                if (_debugLogging)
                    Debug.Log("[ProgressSyncService] Auto sync already running");
                return;
            }
            
            _syncCoroutine = _coroutineRunner.StartCoroutine(AutoSyncCoroutine());
            if (_debugLogging)
                Debug.Log($"<color=#00FF00>[ProgressSyncService] Auto sync started (interval: {_syncInterval}s)</color>");
        }
        
        public void StopAutoSync()
        {
            if (_syncCoroutine != null && _coroutineRunner != null)
            {
                _coroutineRunner.StopCoroutine(_syncCoroutine);
                _syncCoroutine = null;
                if (_debugLogging)
                    Debug.Log("<color=#FFFF00>[ProgressSyncService] Auto sync stopped</color>");
            }
        }
        
        public void LoadProgress()
        {
            if (!IsInitialized || _coroutineRunner == null)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot load progress - not initialized");
                return;
            }
            
            if (!_apiService.IsAuthenticated)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot load progress - not authenticated");
                return;
            }
            
            _coroutineRunner.StartCoroutine(LoadProgressCoroutine());
        }
        
        public void SaveProgressNow()
        {
            if (!IsInitialized || _coroutineRunner == null)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot save progress - not initialized");
                return;
            }
            
            if (!_apiService.IsAuthenticated)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot save progress - not authenticated");
                return;
            }
            
            _coroutineRunner.StartCoroutine(SaveProgressCoroutine());
        }
        
        public void ResetProgress()
        {
            if (!IsInitialized || _coroutineRunner == null)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot reset progress - not initialized");
                return;
            }
            
            if (!_apiService.IsAuthenticated)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot reset progress - not authenticated");
                return;
            }
            
            _coroutineRunner.StartCoroutine(ResetProgressCoroutine());
        }
        
        public void SetProgressData(int level, long watts, long currentXp, long totalXp)
        {
            // Check if data changed
            if (_level != level || _watts != watts || _currentXp != currentXp || _totalXp != totalXp)
            {
                _level = level;
                _watts = watts;
                _currentXp = currentXp;
                _totalXp = totalXp;
                _isDirty = true;
            }
        }
        
        #endregion
        
        #region Private Coroutines
        
        private IEnumerator AutoSyncCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_syncInterval);
                
                // Only sync if authenticated and data has changed
                if (_apiService.IsAuthenticated && _isDirty)
                {
                    yield return SaveProgressCoroutine();
                }
            }
        }
        
        private IEnumerator LoadProgressCoroutine()
        {
            bool completed = false;
            
            yield return _apiService.LoadProgress(
                onSuccess: (response) =>
                {
                    LastSyncTime = DateTime.Now;
                    
                    // Update local data from server
                    if (response.progress != null)
                    {
                        _level = response.progress.level;
                        _watts = response.progress.watts;
                        _currentXp = response.progress.currentXp;
                        _totalXp = response.progress.totalXp;
                        _isDirty = false;
                    }
                    
                    Debug.Log($"<color=#00FF00>[ProgressSyncService] Progress loaded: Level={_level}, Watts={_watts}, XP={_currentXp}/{_totalXp}</color>");
                    OnProgressLoaded?.Invoke(response);
                    completed = true;
                },
                onError: (error) =>
                {
                    Debug.LogError($"[ProgressSyncService] Failed to load progress: {error}");
                    OnSyncError?.Invoke(error);
                    completed = true;
                }
            );
            
            yield return new WaitUntil(() => completed);
        }
        
        private IEnumerator SaveProgressCoroutine()
        {
            var request = new SaveProgressRequest
            {
                level = _level,
                watts = _watts,
                currentXp = _currentXp,
                totalXp = _totalXp
            };
            
            bool completed = false;
            
            yield return _apiService.SaveProgress(
                request,
                onSuccess: (response) =>
                {
                    LastSyncTime = DateTime.Now;
                    _isDirty = false;
                    
                    Debug.Log($"<color=#00FF00>[ProgressSyncService] Progress saved: {response.message}</color>");
                    OnProgressSaved?.Invoke(response);
                    completed = true;
                },
                onError: (error) =>
                {
                    Debug.LogError($"[ProgressSyncService] Failed to save progress: {error}");
                    OnSyncError?.Invoke(error);
                    completed = true;
                }
            );
            
            yield return new WaitUntil(() => completed);
        }
        
        private IEnumerator ResetProgressCoroutine()
        {
            bool completed = false;
            
            yield return _apiService.ResetProgress(
                onSuccess: (response) =>
                {
                    LastSyncTime = DateTime.Now;
                    
                    // Update local data from reset response
                    if (response.progress != null)
                    {
                        _level = response.progress.level;
                        _watts = response.progress.watts;
                        _currentXp = response.progress.currentXp;
                        _totalXp = response.progress.totalXp;
                        _isDirty = false;
                    }
                    
                    Debug.Log($"<color=#FFFF00>[ProgressSyncService] Progress reset: {response.message}</color>");
                    OnProgressReset?.Invoke(response);
                    completed = true;
                },
                onError: (error) =>
                {
                    Debug.LogError($"[ProgressSyncService] Failed to reset progress: {error}");
                    OnSyncError?.Invoke(error);
                    completed = true;
                }
            );
            
            yield return new WaitUntil(() => completed);
        }
        
        #endregion
    }
}

