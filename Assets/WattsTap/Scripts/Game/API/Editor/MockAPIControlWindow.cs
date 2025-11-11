#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using WattsTap.Game.API;
using WattsTap.Core;

namespace WattsTap.Scripts.Game.API.Editor
{
    /// <summary>
    /// Editor window для управления Mock API сервисом
    /// Позволяет тестировать API функционал без реального сервера
    /// </summary>
    public class MockAPIControlWindow : EditorWindow
    {
        private WattsTapAPIMockService _mockService;
        private Vector2 _scrollPosition;

        // Параметры для тестирования
        private long _wattsToAdd = 1000;
        private long _xpToAdd = 500;
        private int _levelToSet = 1;
        private float _errorChance;
        private bool _simulateNetworkDelay = true;
        private float _minDelay = 0.1f;
        private float _maxDelay = 0.5f;

        [MenuItem("WattsTap/Mock API Control Panel")]
        public static void ShowWindow()
        {
            var window = GetWindow<MockAPIControlWindow>("Mock API Control");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            RefreshMockService();
        }

        private void RefreshMockService()
        {
            if (Application.isPlaying)
            {
                _mockService = ServiceLocator.Get<IWattsTapAPIService>() as WattsTapAPIMockService;
            }
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawConnectionStatus();
            EditorGUILayout.Space(10);

            if (_mockService != null)
            {
                DrawPlayerDataSection();
                EditorGUILayout.Space(10);

                DrawResourcesSection();
                EditorGUILayout.Space(10);

                DrawSimulationSettings();
                EditorGUILayout.Space(10);

                DrawQuickActions();
            }
            else
            {
                DrawNotPlayingMessage();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUILayout.Label("Mock API Control Panel", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Эта панель позволяет управлять Mock API сервисом для тестирования игры в редакторе без реального бэкенда.",
                MessageType.Info
            );
        }

        private void DrawConnectionStatus()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Status", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                if (_mockService != null)
                {
                    EditorGUILayout.LabelField("Service Status:", "✓ Mock API Active");
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField("Mode:", "MOCK (Editor Testing)");
                    GUI.color = Color.white;
                }
                else
                {
                    EditorGUILayout.LabelField("Service Status:", "⚠ Real API Active");
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField("Mode:", "PRODUCTION");
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Mock service not found. Are you using WattsTapAPIMockService?", MessageType.Warning);
                }

                if (GUILayout.Button("Refresh Service Reference"))
                {
                    RefreshMockService();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Service Status:", "Not Running");
                GUI.color = Color.gray;
                EditorGUILayout.LabelField("Play mode required");
                GUI.color = Color.white;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPlayerDataSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Player Data", EditorStyles.boldLabel);

            var playerData = _mockService.GetCurrentMockData();
            if (playerData != null)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Player ID:", playerData.playerId);
                EditorGUILayout.LabelField("Nickname:", playerData.nickname);
                EditorGUILayout.LabelField("Level:", playerData.level.ToString());
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Resources:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Watts:", playerData.resources.watts.ToString("N0"));
                EditorGUILayout.LabelField("Current XP:", playerData.resources.currentXP.ToString("N0"));
                EditorGUILayout.LabelField("XP to Next Level:", playerData.resources.xpToNextLevel.ToString("N0"));
                EditorGUILayout.LabelField("Current Hits:", $"{playerData.resources.currentHits} / {playerData.resources.maxHits}");
                EditorGUILayout.LabelField("Energy:", $"{playerData.resources.currentEnergy} / {playerData.resources.maxEnergy}");
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Statistics:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Total Taps:", playerData.stats.totalTaps.ToString("N0"));
                EditorGUILayout.LabelField("Income per Tap:", playerData.stats.incomePerTap.ToString("N0"));
                EditorGUILayout.LabelField("Income per Hour:", playerData.stats.incomePerHour.ToString("N0"));
                EditorGUI.indentLevel--;
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawResourcesSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Modify Resources", EditorStyles.boldLabel);

            // Add Watts
            EditorGUILayout.BeginHorizontal();
            _wattsToAdd = EditorGUILayout.LongField("Watts to Add:", _wattsToAdd);
            if (GUILayout.Button("Add Watts", GUILayout.Width(100)))
            {
                _mockService.AddWatts(_wattsToAdd);
            }
            EditorGUILayout.EndHorizontal();

            // Add XP
            EditorGUILayout.BeginHorizontal();
            _xpToAdd = EditorGUILayout.LongField("XP to Add:", _xpToAdd);
            if (GUILayout.Button("Add XP", GUILayout.Width(100)))
            {
                _mockService.AddXp(_xpToAdd);
            }
            EditorGUILayout.EndHorizontal();

            // Set Level
            EditorGUILayout.BeginHorizontal();
            _levelToSet = EditorGUILayout.IntField("Set Level:", _levelToSet);
            if (GUILayout.Button("Set Level", GUILayout.Width(100)))
            {
                _mockService.SetLevel(_levelToSet);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Restore Hits
            if (GUILayout.Button("Restore Hits to Max"))
            {
                _mockService.RestoreHits();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSimulationSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Simulation Settings", EditorStyles.boldLabel);

            // Network Delay
            _simulateNetworkDelay = EditorGUILayout.Toggle("Simulate Network Delay:", _simulateNetworkDelay);
            if (_simulateNetworkDelay)
            {
                EditorGUI.indentLevel++;
                _minDelay = EditorGUILayout.Slider("Min Delay (s):", _minDelay, 0f, 2f);
                _maxDelay = EditorGUILayout.Slider("Max Delay (s):", _maxDelay, 0f, 2f);
                EditorGUI.indentLevel--;

                // Применяем значения
                _mockService.simulateNetworkDelay = _simulateNetworkDelay;
                _mockService.minDelay = _minDelay;
                _mockService.maxDelay = Mathf.Max(_maxDelay, _minDelay);
            }

            EditorGUILayout.Space(5);

            // Error Simulation
            _errorChance = EditorGUILayout.Slider("Error Chance (%):", _errorChance * 100f, 0f, 100f) / 100f;
            if (GUILayout.Button("Apply Error Chance"))
            {
                _mockService.SetErrorChance(_errorChance);
            }

            EditorGUILayout.HelpBox(
                "Error Chance симулирует случайные ошибки API (например, сетевые проблемы). " +
                "Полезно для тестирования error handling в клиенте.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("💰 Add 10K Watts"))
            {
                _mockService.AddWatts(10000);
            }

            if (GUILayout.Button("⚡ Add 1K XP"))
            {
                _mockService.AddXp(1000);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔄 Reset Player Data"))
            {
                if (EditorUtility.DisplayDialog("Reset Player Data", 
                    "Are you sure you want to reset all player data to default?", "Yes", "Cancel"))
                {
                    _mockService.ResetPlayerData();
                }
            }

            if (GUILayout.Button("❤️ Restore Hits"))
            {
                _mockService.RestoreHits();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("📈 Level Up"))
            {
                var currentData = _mockService.GetCurrentMockData();
                _mockService.SetLevel(currentData.level + 1);
            }

            if (GUILayout.Button("💎 Add Million Watts"))
            {
                _mockService.AddWatts(1000000);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawNotPlayingMessage()
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to use Mock API Control Panel.\n\n" +
                "Make sure you register WattsTapAPIMockService instead of WattsTapAPIService in your ApplicationEntry.",
                MessageType.Warning
            );

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Enter Play Mode"))
            {
                EditorApplication.isPlaying = true;
            }
        }

        private void OnInspectorUpdate()
        {
            // Обновляем окно каждые 0.5 секунды когда в play mode
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
#endif