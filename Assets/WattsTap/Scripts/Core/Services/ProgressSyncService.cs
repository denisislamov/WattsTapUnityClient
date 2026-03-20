using System;
using System.Collections;
using UnityEngine;
using WattsTap.Core.API;
using WattsTap.Core.Configs;
using WattsTap.Core.Configs.CoreServer;

namespace WattsTap.Core.Services
{
    /// <summary>
    /// Interface for progress synchronization service.
    /// Supports both legacy full-state sync and new tap-based sync.
    /// </summary>
    public interface IProgressSyncService : IService
    {
        /// <summary>Whether sync is currently active</summary>
        bool IsSyncActive { get; }
        
        /// <summary>Last sync time</summary>
        DateTime LastSyncTime { get; }
        
        /// <summary>Interval between auto-syncs in seconds</summary>
        float SyncInterval { get; set; }
        
        /// <summary>Current player energy from server</summary>
        int CurrentEnergy { get; }
        
        /// <summary>Whether the player has energy remaining</summary>
        bool HasEnergy { get; }
        
        /// <summary>Number of taps waiting to be sent</summary>
        int PendingTapCount { get; }
        
        /// <summary>Fired when progress is loaded from server</summary>
        event Action<LoadProgressResponse> OnProgressLoaded;
        
        /// <summary>Fired when progress is saved to server (legacy)</summary>
        event Action<SaveProgressResponse> OnProgressSaved;
        
        /// <summary>Fired when taps are processed by new server</summary>
        event Action<TapProgressResponse> OnTapsProcessed;
        
        /// <summary>Fired when progress is reset</summary>
        event Action<ResetProgressResponse> OnProgressReset;
        
        /// <summary>Fired on sync error</summary>
        event Action<string> OnSyncError;
        
        /// <summary>Fired when energy runs out (droppedTapCount > 0)</summary>
        event Action<int> OnEnergyDepleted;
        
        /// <summary>Fired when energy is updated from server response</summary>
        event Action<int> OnEnergyUpdated;
        
        /// <summary>Start automatic sync</summary>
        void StartAutoSync();
        
        /// <summary>Stop automatic sync</summary>
        void StopAutoSync();
        
        /// <summary>Load progress from server once</summary>
        void LoadProgress();
        
        /// <summary>Save progress to server immediately (legacy or flush taps)</summary>
        void SaveProgressNow();
        
        /// <summary>Reset progress on server</summary>
        void ResetProgress();
        
        /// <summary>Set the current progress data for syncing (legacy full-state)</summary>
        void SetProgressData(int level, long watts, long currentXp, long totalXp);
        
        /// <summary>Add taps to the pending batch (new tap-based model)</summary>
        void AddTaps(int tapCount);
        
        /// <summary>Flush all pending taps to server immediately</summary>
        void FlushTaps();
    }
    
    /// <summary>
    /// Service for synchronizing player progress with server.
    /// Supports both legacy full-state sync (via IReferralAPIService) and
    /// new tap-based sync (via ICoreServerService).
    /// </summary>
    public class ProgressSyncService : IProgressSyncService
    {
        #region Private Fields
        
        private ICoreServerService _coreService;
#if OLD_SERVER
        // OLD_SERVER: Legacy-сервис для обратной совместимости
        private IReferralAPIService _legacyService;
#endif
        private CoreServerConfig _coreConfig;
        private ProgressSyncConfig _config;
        private MonoBehaviour _coroutineRunner;
        private Coroutine _syncCoroutine;
        
        // Whether we use new tap-based API
        private bool _useTapMode;
        
        // Tap accumulator (new model)
        private int _pendingTapCount;
        private int _currentEnergy = int.MaxValue;
        
        // Legacy progress data
        private int _level;
        private long _watts;
        private long _currentXp;
        private long _totalXp;
        private bool _isDirty;
        
        // Sync settings
        private float _syncInterval = 5f;
        private const float MinSyncInterval = 1f;
        private const float MaxSyncInterval = 30f;
        private bool _debugLogging = true;
        
        #endregion
        
        #region IService
        
        public int InitializationOrder => 60;
        public bool IsInitialized { get; private set; }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            // Try new service first
            _useTapMode = ServiceLocator.TryGet(out _coreService) && _coreService.IsInitialized;
            
#if OLD_SERVER
            // OLD_SERVER: Фоллбек на legacy-сервис
            ServiceLocator.TryGet(out _legacyService);
            
            if (!_useTapMode && _legacyService == null)
            {
                Debug.LogError("[ProgressSyncService] Neither ICoreServerService nor IReferralAPIService found!");
                return;
            }
#else
            if (!_useTapMode)
            {
                Debug.LogError("[ProgressSyncService] ICoreServerService not found or not initialized!");
                return;
            }
#endif
            
            // Load configs
            if (ServiceLocator.TryGet<IConfigService>(out var configService))
            {
                _config = configService.GetConfig<ProgressSyncConfig>("ProgressSyncConfig");
                if (_config != null)
                {
                    _syncInterval = _config.SyncInterval;
                    _debugLogging = _config.DebugLogging;
                }
                
                try
                {
                    _coreConfig = configService.GetConfig<CoreServerConfig>(
                        WattsTap.Constants.ConfigsConstants.CoreServerConfig);
                    if (_coreConfig != null && _useTapMode)
                    {
                        _syncInterval = _coreConfig.TapSyncInterval;
                    }
                }
                catch { /* CoreServerConfig not registered yet — OK */ }
            }
            
            // Find coroutine runner
            if (ServiceLocator.TryGet<IApplicationEntry>(out var appEntry) && appEntry is MonoBehaviour mb)
                _coroutineRunner = mb;
            else
                _coroutineRunner = UnityEngine.Object.FindAnyObjectByType<MonoBehaviour>();
            
            if (_coroutineRunner == null)
            {
                Debug.LogError("[ProgressSyncService] No MonoBehaviour found to run coroutines!");
                return;
            }
            
            IsInitialized = true;
            if (_debugLogging)
                Debug.Log($"<color=#00FFFF>[ProgressSyncService] Initialized (mode: {(_useTapMode ? "TAP" : "LEGACY")}, interval: {_syncInterval}s)</color>");
        }
        
        public void Shutdown()
        {
            StopAutoSync();
            IsInitialized = false;
        }
        
        #endregion
        
        #region Properties
        
        public bool IsSyncActive => _syncCoroutine != null;
        public DateTime LastSyncTime { get; private set; }
        public int CurrentEnergy => _useTapMode ? _currentEnergy : int.MaxValue;
        public bool HasEnergy => _currentEnergy > 0;
        public int PendingTapCount => _pendingTapCount;
        
        public float SyncInterval
        {
            get => _syncInterval;
            set => _syncInterval = Mathf.Clamp(value, MinSyncInterval, MaxSyncInterval);
        }
        
        #endregion
        
        #region Events
        
        public event Action<LoadProgressResponse> OnProgressLoaded;
        public event Action<SaveProgressResponse> OnProgressSaved;
        public event Action<TapProgressResponse> OnTapsProcessed;
        public event Action<ResetProgressResponse> OnProgressReset;
        public event Action<string> OnSyncError;
        public event Action<int> OnEnergyDepleted;
        public event Action<int> OnEnergyUpdated;
        
        #endregion
        
        #region Public Methods
        
        public void StartAutoSync()
        {
            if (!IsInitialized || _coroutineRunner == null)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot start auto sync — not initialized");
                return;
            }
            
            if (_syncCoroutine != null)
            {
                if (_debugLogging) Debug.Log("[ProgressSyncService] Auto sync already running");
                return;
            }
            
            _syncCoroutine = _coroutineRunner.StartCoroutine(AutoSyncCoroutine());
            if (_debugLogging)
                Debug.Log($"<color=#00FF00>[ProgressSyncService] Auto sync started ({(_useTapMode ? "TAP" : "LEGACY")}, interval: {_syncInterval}s)</color>");
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
                Debug.LogWarning("[ProgressSyncService] Cannot load progress — not initialized");
                return;
            }
            
            _coroutineRunner.StartCoroutine(LoadProgressCoroutine());
        }
        
        public void SaveProgressNow()
        {
            if (!IsInitialized || _coroutineRunner == null) return;
            
            if (_useTapMode)
                FlushTaps();
#if OLD_SERVER
            // OLD_SERVER: Legacy save
            else
                _coroutineRunner.StartCoroutine(SaveProgressLegacyCoroutine());
#endif
        }
        
        public void ResetProgress()
        {
            if (!IsInitialized || _coroutineRunner == null) return;
            _coroutineRunner.StartCoroutine(ResetProgressCoroutine());
        }
        
        /// <summary>Legacy: Set full progress state for next sync.</summary>
        public void SetProgressData(int level, long watts, long currentXp, long totalXp)
        {
            if (_level != level || _watts != watts || _currentXp != currentXp || _totalXp != totalXp)
            {
                _level = level;
                _watts = watts;
                _currentXp = currentXp;
                _totalXp = totalXp;
                _isDirty = true;
            }
        }
        
        /// <summary>New: Accumulate taps for next batch send.</summary>
        public void AddTaps(int tapCount)
        {
            _pendingTapCount += tapCount;
        }
        
        /// <summary>New: Send all pending taps immediately.</summary>
        public void FlushTaps()
        {
            if (_pendingTapCount <= 0) return;
            if (!IsInitialized || _coroutineRunner == null) return;
            _coroutineRunner.StartCoroutine(SendTapsCoroutine());
        }
        
        #endregion
        
        #region Private Coroutines
        
        private IEnumerator AutoSyncCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_syncInterval);
                
                if (_useTapMode)
                {
                    // Tap mode: send pending taps
                    if (_coreService.IsAuthenticated && _pendingTapCount > 0)
                    {
                        yield return SendTapsCoroutine();
                    }
                }
#if OLD_SERVER
                else
                {
                    // Legacy mode: send full state if dirty
                    if (_legacyService != null && _legacyService.IsAuthenticated && _isDirty)
                    {
                        yield return SaveProgressLegacyCoroutine();
                    }
                }
#endif
            }
        }
        
        private IEnumerator LoadProgressCoroutine()
        {
            bool completed = false;
            
            if (_useTapMode && _coreService.IsAuthenticated)
            {
                yield return _coreService.LoadProgress(
                    onSuccess: (response) =>
                    {
                        LastSyncTime = DateTime.Now;
                        if (response.progress != null)
                        {
                            _level = response.progress.level;
                            _watts = response.progress.watts;
                            _currentXp = response.progress.currentXp;
                            _totalXp = response.progress.totalXp;
                            _isDirty = false;
                        }
                        if (response.playerState != null)
                        {
                            _currentEnergy = response.playerState.energy;
                            OnEnergyUpdated?.Invoke(_currentEnergy);
                        }
                        
                        if (_debugLogging)
                            Debug.Log($"<color=#00FF00>[ProgressSyncService] Progress loaded: Level={_level}, Watts={_watts}, XP={_currentXp}/{_totalXp}, Energy={_currentEnergy}</color>");
                        
                        // Fire legacy event with compatible response
                        var legacyResp = new LoadProgressResponse
                        {
                            success = response.success,
                            progress = response.progress,
                            isNewPlayer = response.isNewPlayer
                        };
                        OnProgressLoaded?.Invoke(legacyResp);
                        completed = true;
                    },
                    onError: (error) =>
                    {
                        Debug.LogError($"[ProgressSyncService] Failed to load progress: {error}");
                        OnSyncError?.Invoke(error);
                        completed = true;
                    }
                );
            }
#if OLD_SERVER
            else if (_legacyService != null && _legacyService.IsAuthenticated)
            {
                yield return _legacyService.LoadProgress(
                    onSuccess: (response) =>
                    {
                        LastSyncTime = DateTime.Now;
                        if (response.progress != null)
                        {
                            _level = response.progress.level;
                            _watts = response.progress.watts;
                            _currentXp = response.progress.currentXp;
                            _totalXp = response.progress.totalXp;
                            _isDirty = false;
                        }
                        if (_debugLogging)
                            Debug.Log($"<color=#00FF00>[ProgressSyncService] Progress loaded (legacy): Level={_level}, Watts={_watts}</color>");
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
            }
#endif
            else
            {
                Debug.LogWarning("[ProgressSyncService] Cannot load progress — no authenticated service");
                completed = true;
            }
            
            yield return new WaitUntil(() => completed);
        }
        
        private IEnumerator SendTapsCoroutine()
        {
            if (_coreService == null || !_coreService.IsAuthenticated)
            {
                Debug.LogWarning("[ProgressSyncService] Cannot send taps — not authenticated");
                yield break;
            }
            
            int tapsToSend = _pendingTapCount;
            if (tapsToSend <= 0) yield break;
            
            _pendingTapCount = 0; // optimistic: clear immediately
            
            bool completed = false;
            
            yield return _coreService.SendTaps(tapsToSend,
                onSuccess: (response) =>
                {
                    LastSyncTime = DateTime.Now;
                    
                    // Update local state from server response
                    if (response.progress != null)
                    {
                        _level = response.progress.level;
                        _watts = response.progress.watts;
                        _currentXp = response.progress.currentXp;
                        _totalXp = response.progress.totalXp;
                        _isDirty = false;
                    }
                    
                    // Update energy
                    if (response.playerState != null)
                    {
                        _currentEnergy = response.playerState.energy;
                        OnEnergyUpdated?.Invoke(_currentEnergy);
                    }
                    else if (response.tap != null)
                    {
                        _currentEnergy = response.tap.energyAfter;
                        OnEnergyUpdated?.Invoke(_currentEnergy);
                    }
                    
                    // Check for dropped taps (energy depleted)
                    if (response.tap?.tapMeta != null && response.tap.tapMeta.droppedTapCount > 0)
                    {
                        Debug.LogWarning($"[ProgressSyncService] {response.tap.tapMeta.droppedTapCount} taps dropped (no energy!)");
                        OnEnergyDepleted?.Invoke(response.tap.tapMeta.droppedTapCount);
                    }
                    
                    if (_debugLogging)
                        Debug.Log($"<color=#00FF00>[ProgressSyncService] Taps sent: {tapsToSend}, applied: {response.tap?.tapMeta?.appliedTapCount ?? 0}</color>");
                    
                    OnTapsProcessed?.Invoke(response);
                    completed = true;
                },
                onError: (error) =>
                {
                    // Put taps back on failure
                    _pendingTapCount += tapsToSend;
                    Debug.LogError($"[ProgressSyncService] Failed to send taps: {error}");
                    OnSyncError?.Invoke(error);
                    completed = true;
                }
            );
            
            yield return new WaitUntil(() => completed);
        }
        
#if OLD_SERVER
        private IEnumerator SaveProgressLegacyCoroutine()
        {
            if (_legacyService == null || !_legacyService.IsAuthenticated) yield break;
            
            var request = new SaveProgressRequest
            {
                level = _level,
                watts = _watts,
                currentXp = _currentXp,
                totalXp = _totalXp
            };
            
            bool completed = false;
            
            yield return _legacyService.SaveProgress(
                request,
                onSuccess: (response) =>
                {
                    LastSyncTime = DateTime.Now;
                    _isDirty = false;
                    if (_debugLogging)
                        Debug.Log($"<color=#00FF00>[ProgressSyncService] Progress saved (legacy): {response.message}</color>");
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
#endif
        
        private IEnumerator ResetProgressCoroutine()
        {
            bool completed = false;
            
            if (_useTapMode && _coreService.IsAuthenticated)
            {
                yield return _coreService.ResetProgress(
                    onSuccess: (response) =>
                    {
                        LastSyncTime = DateTime.Now;
                        if (response.progress != null)
                        {
                            _level = response.progress.level;
                            _watts = response.progress.watts;
                            _currentXp = response.progress.currentXp;
                            _totalXp = response.progress.totalXp;
                            _isDirty = false;
                        }
                        _pendingTapCount = 0;
                        if (_debugLogging)
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
            }
#if OLD_SERVER
            else if (_legacyService != null && _legacyService.IsAuthenticated)
            {
                yield return _legacyService.ResetProgress(
                    onSuccess: (response) =>
                    {
                        LastSyncTime = DateTime.Now;
                        if (response.progress != null)
                        {
                            _level = response.progress.level;
                            _watts = response.progress.watts;
                            _currentXp = response.progress.currentXp;
                            _totalXp = response.progress.totalXp;
                            _isDirty = false;
                        }
                        if (_debugLogging)
                            Debug.Log($"<color=#FFFF00>[ProgressSyncService] Progress reset (legacy): {response.message}</color>");
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
            }
#endif
            
            yield return new WaitUntil(() => completed);
        }
        
        #endregion
    }
}

