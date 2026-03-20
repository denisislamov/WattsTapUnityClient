#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using WattsTap.Core.API;

namespace WattsTap.Core.Services.Editor
{
    /// <summary>
    /// Editor window for debugging and managing player progress.
    /// Allows reset progress and view current state.
    /// </summary>
    public class ProgressDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showSyncStatus = true;
        private bool _showDangerZone;
        
        [MenuItem("WattsTap/Progress Debug Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProgressDebugWindow>("Progress Debug");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawHeader();
            
            EditorGUILayout.Space(10);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use progress debugging features.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }
            
            DrawSyncStatusSection();
            EditorGUILayout.Space(10);
            DrawActionsSection();
            EditorGUILayout.Space(20);
            DrawDangerZoneSection();
            
            EditorGUILayout.EndScrollView();
            
            // Auto-refresh in play mode
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Progress Debug Window", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Debug tool for managing player progress. Use with caution in production builds.",
                MessageType.Info);
        }
        
        private void DrawSyncStatusSection()
        {
            _showSyncStatus = EditorGUILayout.BeginFoldoutHeaderGroup(_showSyncStatus, "Sync Status");
            
            if (_showSyncStatus)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                if (ServiceLocator.TryGet<IProgressSyncService>(out var syncService))
                {
                    // Status indicators
                    DrawLabelPair("Initialized:", syncService.IsInitialized ? "✓ Yes" : "✗ No");
                    DrawLabelPair("Auto Sync Active:", syncService.IsSyncActive ? "✓ Yes" : "✗ No");
                    DrawLabelPair("Sync Interval:", $"{syncService.SyncInterval:F1}s");
                    DrawLabelPair("Last Sync:", syncService.LastSyncTime != default 
                        ? syncService.LastSyncTime.ToString("HH:mm:ss") 
                        : "Never");
                    
                    // API Auth status
                    EditorGUILayout.Space(5);
                    if (ServiceLocator.TryGet<ICoreServerService>(out var coreService))
                    {
                        DrawLabelPair("Core API Auth:", coreService.IsAuthenticated ? "✓ Yes" : "✗ No");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("ProgressSyncService not available", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawLabelPair(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));
            EditorGUILayout.LabelField(value);
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawActionsSection()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (ServiceLocator.TryGet<IProgressSyncService>(out var syncService))
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Load Progress", GUILayout.Height(30)))
                {
                    syncService.LoadProgress();
                    Debug.Log("[ProgressDebugWindow] Loading progress from server...");
                }
                
                if (GUILayout.Button("Save Progress Now", GUILayout.Height(30)))
                {
                    syncService.SaveProgressNow();
                    Debug.Log("[ProgressDebugWindow] Saving progress to server...");
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                
                GUI.enabled = !syncService.IsSyncActive;
                if (GUILayout.Button("Start Auto Sync", GUILayout.Height(25)))
                {
                    syncService.StartAutoSync();
                }
                GUI.enabled = true;
                
                GUI.enabled = syncService.IsSyncActive;
                if (GUILayout.Button("Stop Auto Sync", GUILayout.Height(25)))
                {
                    syncService.StopAutoSync();
                }
                GUI.enabled = true;
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("ProgressSyncService not available", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDangerZoneSection()
        {
            _showDangerZone = EditorGUILayout.BeginFoldoutHeaderGroup(_showDangerZone, "⚠ Danger Zone");
            
            if (_showDangerZone)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Warning
                EditorGUILayout.HelpBox(
                    "WARNING: Resetting progress will delete ALL player data including level, watts, and experience. This action cannot be undone!",
                    MessageType.Warning);
                
                EditorGUILayout.Space(10);
                
                // Reset Progress Button
                GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
                
                if (GUILayout.Button("🗑 RESET PROGRESS", GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Reset Progress",
                        "Are you sure you want to reset all player progress?\n\nThis will:\n- Reset level to 1\n- Reset Watts to 0\n- Reset all experience\n\nThis action CANNOT be undone!",
                        "Yes, Reset Everything",
                        "Cancel"))
                    {
                        ResetProgress();
                    }
                }
                
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void ResetProgress()
        {
            // Use ProgressSyncService to reset progress
            if (ServiceLocator.TryGet<IProgressSyncService>(out var syncService))
            {
                syncService.ResetProgress();
                Debug.Log("<color=#FF0000>[ProgressDebugWindow] Progress reset initiated via ProgressSyncService</color>");
                return;
            }
            
            Debug.LogError("[ProgressDebugWindow] Cannot reset progress - ProgressSyncService not available!");
        }
    }
}
#endif

