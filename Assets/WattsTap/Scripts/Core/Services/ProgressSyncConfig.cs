using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Core.Services
{
    /// <summary>
    /// Configuration for progress synchronization with server
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressSyncConfig", menuName = "WattsTap/Configs/Progress Sync Config")]
    public class ProgressSyncConfig : BaseConfig
    {
        [Header("Sync Settings")]
        [Tooltip("Interval between auto-syncs in seconds (default: 5)")]
        [SerializeField] [Range(3f, 30f)] private float _syncInterval = 5f;
        
        [Tooltip("Whether to start auto-sync automatically after authentication")]
        [SerializeField] private bool _autoStartSync = true;
        
        [Tooltip("Enable debug logging for sync operations")]
        [SerializeField] private bool _debugLogging = true;

        public float SyncInterval => _syncInterval;
        public bool AutoStartSync => _autoStartSync;
        public bool DebugLogging => _debugLogging;
    }
}

