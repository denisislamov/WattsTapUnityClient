#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace WattsTap.Core.Inventory.Editor
{
    /// <summary>
    /// Custom editor for InventoryStartConfig with convenient tools for managing start items.
    /// </summary>
    [CustomEditor(typeof(InventoryStartConfig))]
    public class InventoryStartConfigEditor : UnityEditor.Editor
    {
        private ItemData _itemToAdd;
        private int _countToAdd = 1;
        private bool _autoEquipToAdd;
        private CatalogConfig _catalogConfig;
        private Vector2 _scrollPosition;
        private bool _showQuickAdd = true;
        private bool _showItemList = true;
        private string _searchFilter = "";
        
        private void OnEnable()
        {
            FindCatalogConfig();
        }
        
        private void FindCatalogConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:CatalogConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _catalogConfig = AssetDatabase.LoadAssetAtPath<CatalogConfig>(path);
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            InventoryStartConfig config = (InventoryStartConfig)target;
            
            DrawHeaderSection();
            DrawQuickAddSection(config);
            DrawItemListSection(config);
            DrawCatalogQuickPick(config);
            DrawActionButtons(config);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawHeaderSection()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Inventory Start Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure items to add to the player's inventory on game start or for testing purposes.",
                MessageType.Info);
            EditorGUILayout.Space(10);
        }
        
        private void DrawQuickAddSection(InventoryStartConfig config)
        {
            _showQuickAdd = EditorGUILayout.BeginFoldoutHeaderGroup(_showQuickAdd, "Quick Add Item");
            
            if (_showQuickAdd)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Item:", GUILayout.Width(80));
                _itemToAdd = (ItemData)EditorGUILayout.ObjectField(_itemToAdd, typeof(ItemData), false);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Count:", GUILayout.Width(80));
                _countToAdd = EditorGUILayout.IntSlider(_countToAdd, 1, 10);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Auto Equip:", GUILayout.Width(80));
                _autoEquipToAdd = EditorGUILayout.Toggle(_autoEquipToAdd);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                GUI.enabled = _itemToAdd != null;
                if (GUILayout.Button("Add Item", GUILayout.Height(25)))
                {
                    Undo.RecordObject(config, "Add Start Item");
                    config.AddStartItem(_itemToAdd, _countToAdd, _autoEquipToAdd);
                    EditorUtility.SetDirty(config);
                    _itemToAdd = null;
                    _countToAdd = 1;
                    _autoEquipToAdd = false;
                }
                GUI.enabled = true;
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);
        }
        
        private void DrawItemListSection(InventoryStartConfig config)
        {
            var items = config.GetEditableItems();
            
            _showItemList = EditorGUILayout.BeginFoldoutHeaderGroup(_showItemList, $"Start Items ({items.Count})");
            
            if (_showItemList)
            {
                if (items.Count == 0)
                {
                    EditorGUILayout.HelpBox("No items configured. Add items using Quick Add or drag from Catalog.", MessageType.None);
                }
                else
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(300));
                    
                    int indexToRemove = -1;
                    
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        
                        // Item icon and name
                        if (item.ItemData != null)
                        {
                            if (item.ItemData.Icon != null)
                            {
                                GUILayout.Label(item.ItemData.Icon.texture, GUILayout.Width(32), GUILayout.Height(32));
                            }
                            else
                            {
                                GUILayout.Box("?", GUILayout.Width(32), GUILayout.Height(32));
                            }
                            
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.LabelField(item.ItemData.DisplayName, EditorStyles.boldLabel);
                            
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"Type: {item.ItemData.ItemType}", GUILayout.Width(120));
                            EditorGUILayout.LabelField($"Rarity: {item.ItemData.Rarity}", GUILayout.Width(100));
                            EditorGUILayout.LabelField($"+{item.ItemData.PerTapBonus:F1}/tap", GUILayout.Width(80));
                            EditorGUILayout.EndHorizontal();
                            
                            EditorGUILayout.EndVertical();
                        }
                        else
                        {
                            EditorGUILayout.LabelField("MISSING ITEM DATA", EditorStyles.boldLabel);
                        }
                        
                        GUILayout.FlexibleSpace();
                        
                        // Count field
                        EditorGUILayout.BeginVertical(GUILayout.Width(60));
                        EditorGUILayout.LabelField("Count", GUILayout.Width(60));
                        item.Count = EditorGUILayout.IntField(item.Count, GUILayout.Width(60));
                        if (item.Count < 1) item.Count = 1;
                        EditorGUILayout.EndVertical();
                        
                        // Auto equip toggle
                        EditorGUILayout.BeginVertical(GUILayout.Width(50));
                        EditorGUILayout.LabelField("Equip", GUILayout.Width(50));
                        item.AutoEquip = EditorGUILayout.Toggle(item.AutoEquip, GUILayout.Width(50));
                        EditorGUILayout.EndVertical();
                        
                        // Remove button
                        if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(32)))
                        {
                            indexToRemove = i;
                        }
                        
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        
                        EditorGUILayout.Space(2);
                    }
                    
                    if (indexToRemove >= 0)
                    {
                        Undo.RecordObject(config, "Remove Start Item");
                        config.RemoveStartItem(indexToRemove);
                        EditorUtility.SetDirty(config);
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);
        }
        
        private void DrawCatalogQuickPick(InventoryStartConfig config)
        {
            if (_catalogConfig == null)
            {
                EditorGUILayout.HelpBox("CatalogConfig not found. Create one to enable quick pick.", MessageType.Warning);
                if (GUILayout.Button("Find Catalog Config"))
                {
                    FindCatalogConfig();
                }
                return;
            }
            
            EditorGUILayout.BeginFoldoutHeaderGroup(true, "Quick Pick from Catalog");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Search filter
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            _searchFilter = EditorGUILayout.TextField(_searchFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                _searchFilter = "";
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Filter by type buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Weapons", EditorStyles.miniButton))
            {
                AddItemsByType(config, ItemType.Weapon);
            }
            if (GUILayout.Button("Body Armor", EditorStyles.miniButton))
            {
                AddItemsByType(config, ItemType.ArmorBody);
            }
            if (GUILayout.Button("Head Armor", EditorStyles.miniButton))
            {
                AddItemsByType(config, ItemType.ArmorHead);
            }
            if (GUILayout.Button("Arms Armor", EditorStyles.miniButton))
            {
                AddItemsByType(config, ItemType.ArmorArms);
            }
            if (GUILayout.Button("Legs Armor", EditorStyles.miniButton))
            {
                AddItemsByType(config, ItemType.ArmorLegs);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Show filtered items from catalog
            var catalogItems = _catalogConfig.Items
                .Where(i => i != null)
                .Where(i => string.IsNullOrEmpty(_searchFilter) || 
                            i.DisplayName.ToLower().Contains(_searchFilter.ToLower()) ||
                            i.Id.ToLower().Contains(_searchFilter.ToLower()))
                .Take(10)
                .ToList();
            
            if (catalogItems.Count > 0)
            {
                EditorGUILayout.LabelField($"Showing {catalogItems.Count} items:", EditorStyles.miniLabel);
                
                EditorGUILayout.BeginHorizontal();
                int count = 0;
                foreach (var item in catalogItems)
                {
                    if (count > 0 && count % 5 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                    
                    string buttonLabel = item.DisplayName.Length > 12 
                        ? item.DisplayName.Substring(0, 10) + ".." 
                        : item.DisplayName;
                    
                    if (GUILayout.Button(buttonLabel, GUILayout.Width(80)))
                    {
                        Undo.RecordObject(config, "Add Start Item");
                        config.AddStartItem(item, 1, false);
                        EditorUtility.SetDirty(config);
                    }
                    count++;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);
        }
        
        private void AddItemsByType(InventoryStartConfig config, ItemType type)
        {
            if (_catalogConfig == null) return;
            
            var items = _catalogConfig.GetItemsByType(type);
            if (items.Count == 0)
            {
                EditorUtility.DisplayDialog("No Items", $"No items of type {type} found in catalog.", "OK");
                return;
            }
            
            // Show selection dialog
            GenericMenu menu = new GenericMenu();
            foreach (var item in items)
            {
                var capturedItem = item;
                menu.AddItem(new GUIContent($"{item.DisplayName} (+{item.PerTapBonus:F1}/tap)"), false, () =>
                {
                    Undo.RecordObject(config, "Add Start Item");
                    config.AddStartItem(capturedItem, 1, false);
                    EditorUtility.SetDirty(config);
                });
            }
            menu.ShowAsContext();
        }
        
        private void DrawActionButtons(InventoryStartConfig config)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Clear All Items", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Clear All", "Remove all items from this configuration?", "Yes", "No"))
                {
                    Undo.RecordObject(config, "Clear All Items");
                    config.ClearAllItems();
                    EditorUtility.SetDirty(config);
                }
            }
            
            if (GUILayout.Button("Validate", GUILayout.Height(25)))
            {
                ValidateConfig(config);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Statistics
            var items = config.GetEditableItems();
            if (items.Count > 0)
            {
                EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
                
                int totalItems = items.Sum(i => i.Count);
                float totalBonus = items.Where(i => i.ItemData != null).Sum(i => i.ItemData.PerTapBonus * i.Count);
                int equipCount = items.Count(i => i.AutoEquip);
                
                EditorGUILayout.LabelField($"Total item instances: {totalItems}");
                EditorGUILayout.LabelField($"Total per-tap bonus: +{totalBonus:F2}");
                EditorGUILayout.LabelField($"Items set to auto-equip: {equipCount}");
            }
        }
        
        private void ValidateConfig(InventoryStartConfig config)
        {
            var items = config.GetEditableItems();
            int issues = 0;
            
            foreach (var item in items)
            {
                if (item.ItemData == null)
                {
                    Debug.LogWarning("[InventoryStartConfig] Found entry with null ItemData");
                    issues++;
                }
                
                if (item.Count < 1)
                {
                    Debug.LogWarning($"[InventoryStartConfig] Item '{item.ItemData?.DisplayName}' has invalid count: {item.Count}");
                    issues++;
                }
            }
            
            // Check for duplicate auto-equip on same type
            var equipByType = items
                .Where(i => i.AutoEquip && i.ItemData != null)
                .GroupBy(i => i.ItemData.ItemType)
                .Where(g => g.Count() > 1);
            
            foreach (var group in equipByType)
            {
                Debug.LogWarning($"[InventoryStartConfig] Multiple items of type {group.Key} set to auto-equip. Only first will be equipped.");
                issues++;
            }
            
            if (issues == 0)
            {
                EditorUtility.DisplayDialog("Validation", "Configuration is valid!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation", $"Found {issues} issue(s). Check console for details.", "OK");
            }
        }
    }
}
#endif

