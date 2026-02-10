#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace WattsTap.Scripts.Game.GlobalConfigs.Editor
{
    /// <summary>
    /// Кастомный инспектор для UpgradesConfig с функцией импорта из CSV
    /// </summary>
    [CustomEditor(typeof(UpgradesConfig))]
    public class UpgradesConfigEditor : UnityEditor.Editor
    {
        private string _csvPath = "Assets/CvsConfigs/WattsBalanceUpgrades.csv";
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            UpgradesConfig config = (UpgradesConfig)target;
            
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("CSV Import Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Импорт данных из WattsBalanceUpgrades.csv. " +
                "Это перезапишет все текущие значения в конфигурации.", 
                MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CSV File Path:", GUILayout.Width(100));
            _csvPath = EditorGUILayout.TextField(_csvPath);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Import from CSV", GUILayout.Height(30)))
            {
                ImportFromCsv(config);
            }
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Validate Configuration", GUILayout.Height(25)))
            {
                ValidateConfig(config);
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            
            if (config.skills != null && config.skills.Count > 0)
            {
                EditorGUILayout.LabelField($"Skills configured: {config.skills.Count}");
                
                // Показываем статистику по каждому скиллу
                EditorGUILayout.Space(5);
                foreach (var skill in config.skills)
                {
                    if (skill == null) continue;
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"{skill.title} ({skill.upgradeType})", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"  Levels: {skill.levels?.Count ?? 0}");
                    EditorGUILayout.LabelField($"  Start Cost: {skill.startCost:N0}");
                    EditorGUILayout.LabelField($"  Start Parameter: {skill.startParameter}");
                    
                    if (skill.levels != null && skill.levels.Count > 0)
                    {
                        var maxLevel = skill.levels[skill.levels.Count - 1];
                        EditorGUILayout.LabelField($"  Max Level Cost: {maxLevel.price:N0}");
                        EditorGUILayout.LabelField($"  Max Level Parameter: {maxLevel.parameter}");
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }
                
                // Подсчитываем общую стоимость всех апгрейдов
                long totalCost = 0;
                foreach (var skill in config.skills)
                {
                    if (skill?.levels == null) continue;
                    foreach (var level in skill.levels)
                    {
                        if (level != null && level.price > 0)
                            totalCost += level.price;
                    }
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Total Cost (all upgrades): {totalCost:N0}", EditorStyles.boldLabel);
            }
        }
        
        private void ImportFromCsv(UpgradesConfig config)
        {
            if (!File.Exists(_csvPath))
            {
                EditorUtility.DisplayDialog(
                    "Error", 
                    $"CSV file not found at path:\n{_csvPath}\n\nPlease check the path and try again.", 
                    "OK");
                return;
            }
            
            if (EditorUtility.DisplayDialog(
                "Import CSV", 
                "This will overwrite all current configuration values.\n\nAre you sure you want to continue?", 
                "Yes, Import", 
                "Cancel"))
            {
                try
                {
                    Undo.RecordObject(config, "Import CSV Data");
                    
                    string csvText = File.ReadAllText(_csvPath);
                    UpgradesCsvConverter.ParseCsvToConfig(csvText, config);
                    
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    
                    EditorUtility.DisplayDialog(
                        "Success", 
                        $"CSV data imported successfully!\n\n{config.skills.Count} skills loaded.\n\nCheck the console for details.", 
                        "OK");
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog(
                        "Error", 
                        $"Failed to import CSV:\n\n{e.Message}", 
                        "OK");
                    Debug.LogError($"[UpgradesConfigEditor] Import failed: {e}");
                }
            }
        }
        
        private void ValidateConfig(UpgradesConfig config)
        {
            bool hasErrors = false;
            int errorCount = 0;
            int warningCount = 0;
            
            // Проверка наличия скиллов
            if (config.skills == null || config.skills.Count == 0)
            {
                Debug.LogError("[UpgradesConfig] No skills configured");
                hasErrors = true;
                errorCount++;
            }
            else
            {
                // Проверка каждого скилла
                foreach (var skill in config.skills)
                {
                    if (skill == null)
                    {
                        Debug.LogError("[UpgradesConfig] Found null skill in configuration");
                        hasErrors = true;
                        errorCount++;
                        continue;
                    }
                    
                    // Проверка названия
                    if (string.IsNullOrEmpty(skill.title))
                    {
                        Debug.LogWarning($"[UpgradesConfig] Skill {skill.upgradeType} has no title");
                        warningCount++;
                    }
                    
                    // Проверка уровней
                    if (skill.levels == null || skill.levels.Count == 0)
                    {
                        Debug.LogError($"[UpgradesConfig] Skill {skill.title} has no levels");
                        hasErrors = true;
                        errorCount++;
                        continue;
                    }
                    
                    // Проверка последовательности уровней
                    for (int i = 0; i < skill.levels.Count; i++)
                    {
                        var level = skill.levels[i];
                        
                        if (level == null)
                        {
                            Debug.LogError($"[UpgradesConfig] Skill {skill.title}: Level at index {i} is null");
                            hasErrors = true;
                            errorCount++;
                            continue;
                        }
                        
                        if (level.level != i + 1)
                        {
                            Debug.LogWarning($"[UpgradesConfig] Skill {skill.title}: Level mismatch at index {i}, expected {i + 1}, got {level.level}");
                            warningCount++;
                        }
                        
                        // Проверка прогрессии стоимости
                        if (i > 0)
                        {
                            var prevLevel = skill.levels[i - 1];
                            if (level.price > 0 && prevLevel.price > 0 && level.price <= prevLevel.price)
                            {
                                Debug.LogWarning($"[UpgradesConfig] Skill {skill.title}, Level {level.level}: Price ({level.price}) should be greater than previous level ({prevLevel.price})");
                                warningCount++;
                            }
                        }
                    }
                }
            }
            
            if (!hasErrors && warningCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "Validation Success", 
                    "Configuration is valid! No errors or warnings found.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Validation Complete", 
                    $"Validation completed with:\n\nErrors: {errorCount}\nWarnings: {warningCount}\n\nCheck the console for details.", 
                    "OK");
            }
        }
    }
}
#endif
