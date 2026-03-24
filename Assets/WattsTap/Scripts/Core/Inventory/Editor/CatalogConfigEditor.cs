#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WattsTap.Core.Inventory.Editor
{
    /// <summary>
    /// Custom editor for CatalogConfig — flat table list matching the design doc screenshots.
    /// Each row: [#] [Icon] [Name] [Type] [Rarity] [Stat] [Lv1 Value] [MaxLv] [Bonuses]
    /// Sprites are editable directly via ObjectField in each row.
    /// Items grouped by rarity, then by type within each rarity section.
    /// </summary>
    [CustomEditor(typeof(CatalogConfig))]
    public class CatalogConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _itemsProp;
        private Vector2 _scrollPos;

        // Foldout states per rarity
        private readonly Dictionary<ItemRarity, bool> _foldouts = new Dictionary<ItemRarity, bool>();
        private bool _showRawList;

        // Column widths
        private const float COL_NUM     = 28f;
        private const float COL_ICON    = 52f;
        private const float COL_NAME    = 140f;
        private const float COL_TYPE    = 60f;
        private const float COL_RARITY  = 80f;
        private const float COL_STAT    = 110f;
        private const float COL_VALUE   = 60f;
        private const float COL_MAXLV   = 42f;
        private const float COL_BONUS   = 120f;
        private const float ROW_HEIGHT  = 50f;
        private const float ICON_SIZE   = 44f;

        // Sort order: Rarity ascending, then Type: Weapon → Arms → Body → Feet
        private static readonly ItemRarity[] RarityOrder =
            { ItemRarity.Common, ItemRarity.Uncommon, ItemRarity.Rare, ItemRarity.Legendary };

        private static readonly ItemType[] TypeOrder =
            { ItemType.Weapon, ItemType.Arms, ItemType.Body, ItemType.Feet };

        private void OnEnable()
        {
            _itemsProp = serializedObject.FindProperty("_items");
            foreach (var r in RarityOrder)
                if (!_foldouts.ContainsKey(r))
                    _foldouts[r] = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var config = (CatalogConfig)target;

            DrawTitle(config);
            DrawColumnHeaders();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // Group items by rarity first, then type within each rarity
            int globalIdx = 0;
            foreach (var rarity in RarityOrder)
            {
                // Collect items for this rarity, ordered by TypeOrder then list order
                var items = new List<(int idx, ItemData data)>();
                foreach (var type in TypeOrder)
                {
                    for (int i = 0; i < config.Items.Count; i++)
                    {
                        var item = config.Items[i];
                        if (item != null && item.Rarity == rarity && item.ItemType == type)
                            items.Add((i, item));
                    }
                }
                if (items.Count == 0) continue;

                DrawRarityHeader(rarity, items.Count);

                if (_foldouts[rarity])
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        DrawItemRow(items[j].idx, items[j].data, globalIdx, j);
                        globalIdx++;
                    }
                }
                else
                {
                    globalIdx += items.Count;
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6);
            DrawFooter();

            serializedObject.ApplyModifiedProperties();
        }

        // ─────────────────────── TITLE ───────────────────────

        private void DrawTitle(CatalogConfig config)
        {
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("⚡ WattsTap Item Catalog", titleStyle, GUILayout.Height(24));

            int total = config.Items.Count(i => i != null);
            var codes = new HashSet<string>();
            foreach (var it in config.Items)
                if (it != null) codes.Add(it.Code ?? it.name);

            var infoStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 10 };
            EditorGUILayout.LabelField(
                $"Total variants: {total}   |   Unique items: {codes.Count}   |   Types: {TypeOrder.Length}",
                infoStyle);
            EditorGUILayout.Space(2);
        }

        // ─────────────────────── COLUMN HEADERS ───────────────────────

        private void DrawColumnHeaders()
        {
            Rect headerRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            EditorGUI.DrawRect(headerRect, new Color(0.18f, 0.18f, 0.18f, 1f));

            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.65f, 0.65f, 0.65f) },
                alignment = TextAnchor.MiddleLeft,
                fontSize = 9
            };
            var styleC = new GUIStyle(style) { alignment = TextAnchor.MiddleCenter };

            GUILayout.Label("#",        styleC, GUILayout.Width(COL_NUM));
            GUILayout.Label("Icon",     styleC, GUILayout.Width(COL_ICON));
            GUILayout.Label("Name",     style,  GUILayout.Width(COL_NAME));
            GUILayout.Label("Type",     styleC, GUILayout.Width(COL_TYPE));
            GUILayout.Label("Rarity",   styleC, GUILayout.Width(COL_RARITY));
            GUILayout.Label("Main Stat", style, GUILayout.Width(COL_STAT));
            GUILayout.Label("Lv1",      styleC, GUILayout.Width(COL_VALUE));
            GUILayout.Label("Max",      styleC, GUILayout.Width(COL_MAXLV));
            GUILayout.Label("Bonus",    style,  GUILayout.Width(COL_BONUS));

            EditorGUILayout.EndHorizontal();

            // Separator line
            Rect sep = GUILayoutUtility.GetRect(0, 1);
            EditorGUI.DrawRect(sep, new Color(0.35f, 0.35f, 0.35f));
        }

        // ─────────────────────── RARITY HEADER ───────────────────────

        private void DrawRarityHeader(ItemRarity rarity, int count)
        {
            Color rarityColor = GetRarityColor(rarity);

            Rect headerRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(26));
            EditorGUI.DrawRect(headerRect, new Color(rarityColor.r * 0.15f, rarityColor.g * 0.15f, rarityColor.b * 0.15f, 0.95f));

            // Left stripe
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, 4, headerRect.height), rarityColor);

            GUILayout.Space(10);

            var foldStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = rarityColor },
                onNormal  = { textColor = rarityColor },
                focused   = { textColor = rarityColor },
                onFocused = { textColor = rarityColor },
                active    = { textColor = rarityColor },
                onActive  = { textColor = rarityColor },
            };

            string symbol = GetRaritySymbol(rarity);
            _foldouts[rarity] = EditorGUILayout.Foldout(_foldouts[rarity],
                $"  {symbol}  {rarity.ToString().ToUpper()}  ({count})", true, foldStyle);

            EditorGUILayout.EndHorizontal();
        }

        // ─────────────────────── ITEM ROW ───────────────────────

        private void DrawItemRow(int listIndex, ItemData item, int globalIdx, int localIdx)
        {
            Color rarityColor = GetRarityColor(item.Rarity);
            Color typeColor = GetTypeColor(item.ItemType);

            // Row background — alternating + rarity tint
            Rect rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(ROW_HEIGHT));
            Color baseBg = localIdx % 2 == 0
                ? new Color(0.22f, 0.22f, 0.22f, 0.25f)
                : new Color(0.19f, 0.19f, 0.19f, 0.25f);
            Color tintedBg = Color.Lerp(baseBg,
                new Color(rarityColor.r * 0.1f, rarityColor.g * 0.1f, rarityColor.b * 0.1f, 0.3f), 0.35f);
            EditorGUI.DrawRect(rowRect, tintedBg);

            // Left type-colored stripe
            EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, 2, rowRect.height), typeColor);

            // ── # ──
            var numStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = new Color(0.45f, 0.45f, 0.45f) }
            };
            GUILayout.Label((globalIdx + 1).ToString(), numStyle,
                GUILayout.Width(COL_NUM), GUILayout.Height(ROW_HEIGHT));

            // ── Icon (EDITABLE) ──
            EditorGUILayout.BeginVertical(GUILayout.Width(COL_ICON), GUILayout.Height(ROW_HEIGHT));
            GUILayout.FlexibleSpace();

            if (item != null)
            {
                var so = new SerializedObject(item);
                var iconField = so.FindProperty("_icon");

                Rect iconRect = GUILayoutUtility.GetRect(ICON_SIZE, ICON_SIZE, GUILayout.Width(ICON_SIZE));

                // Rarity border behind icon
                Rect borderRect = new Rect(iconRect.x - 1, iconRect.y - 1,
                    iconRect.width + 2, iconRect.height + 2);
                EditorGUI.DrawRect(borderRect, rarityColor);
                EditorGUI.DrawRect(iconRect, new Color(0.12f, 0.12f, 0.12f));

                // Draw current icon as texture
                if (item.Icon != null)
                    GUI.DrawTexture(iconRect, item.Icon.texture, ScaleMode.ScaleToFit);

                // Overlay invisible ObjectField for drag-drop sprite assignment
                EditorGUI.BeginChangeCheck();
                var newSprite = (Sprite)EditorGUI.ObjectField(iconRect, item.Icon, typeof(Sprite), false);
                if (EditorGUI.EndChangeCheck())
                {
                    iconField.objectReferenceValue = newSprite;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(item);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            GUILayout.Space(4);

            // ── Name ──
            EditorGUILayout.BeginVertical(GUILayout.Width(COL_NAME), GUILayout.Height(ROW_HEIGHT));
            GUILayout.FlexibleSpace();

            var nameStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = rarityColor },
                wordWrap = false
            };
            GUILayout.Label(item.DisplayName ?? item.name, nameStyle, GUILayout.Width(COL_NAME));

            var codeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 8,
                normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
            };
            GUILayout.Label(item.Code ?? "", codeStyle, GUILayout.Width(COL_NAME));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            // ── Type badge ──
            EditorGUILayout.BeginVertical(GUILayout.Width(COL_TYPE), GUILayout.Height(ROW_HEIGHT));
            GUILayout.FlexibleSpace();

            Rect typeArea = GUILayoutUtility.GetRect(COL_TYPE, 16);
            float tw = 52f;
            float tx = typeArea.x + (COL_TYPE - tw) / 2f;
            Rect typeRect = new Rect(tx, typeArea.y, tw, 16);
            EditorGUI.DrawRect(typeRect,
                new Color(typeColor.r * 0.2f, typeColor.g * 0.2f, typeColor.b * 0.2f, 0.8f));
            DrawBorder(typeRect, typeColor, 1);
            var typeStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 8,
                fontStyle = FontStyle.Bold,
                normal = { textColor = typeColor }
            };
            GUI.Label(typeRect, GetTypeIcon(item.ItemType) + " " + GetTypeShort(item.ItemType), typeStyle);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            // ── Rarity badge ──
            EditorGUILayout.BeginVertical(GUILayout.Width(COL_RARITY), GUILayout.Height(ROW_HEIGHT));
            GUILayout.FlexibleSpace();

            Rect badgeAreaRect = GUILayoutUtility.GetRect(COL_RARITY, 16);
            float badgeW = Mathf.Min(COL_RARITY - 4, 72);
            float badgeX = badgeAreaRect.x + (COL_RARITY - badgeW) / 2f;
            Rect badgeRect = new Rect(badgeX, badgeAreaRect.y, badgeW, 16);
            EditorGUI.DrawRect(badgeRect,
                new Color(rarityColor.r * 0.25f, rarityColor.g * 0.25f, rarityColor.b * 0.25f, 0.9f));
            DrawBorder(badgeRect, rarityColor, 1);

            var badgeStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                normal = { textColor = rarityColor }
            };
            string symbol = GetRaritySymbol(item.Rarity);
            GUI.Label(badgeRect, $"{symbol} {item.Rarity}", badgeStyle);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            // ── Main Stat ──
            string statText = $"{GetStatIcon(item.MainStatType)} {item.MainStatType}";
            DrawCellCentered(statText, COL_STAT,
                new Color(0.75f, 0.85f, 0.95f), 9, TextAnchor.MiddleLeft);

            // ── Lv1 Value ──
            string lv1Text = "";
            if (item.Levels != null && item.Levels.Count > 0)
            {
                var lv1 = item.Levels[0];
                lv1Text = lv1.Value > 0 ? $"{lv1.Value:F0}" : $"{lv1.ValuePercent:F1}%";
            }
            DrawCellCentered(lv1Text, COL_VALUE, new Color(0.9f, 0.95f, 1f), 10);

            // ── Max Level ──
            DrawCellCentered(item.MaxLevel.ToString(), COL_MAXLV,
                new Color(0.6f, 0.6f, 0.6f), 9);

            // ── Bonus ──
            string bonusText = "";
            if (item.Bonuses != null && item.Bonuses.Count > 0)
            {
                var b = item.Bonuses[0];
                bonusText = b.Value > 0
                    ? $"+{b.Value:F0} {b.StatType}"
                    : $"+{b.ValuePercent:F1}% {b.StatType}";
            }
            DrawCellCentered(bonusText, COL_BONUS,
                new Color(1f, 0.9f, 0.4f), 8, TextAnchor.MiddleLeft);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Row bottom separator
            Rect sepRect = GUILayoutUtility.GetRect(0, 1);
            EditorGUI.DrawRect(sepRect, new Color(0.25f, 0.25f, 0.25f, 0.5f));

            // Double-click → select item SO
            if (Event.current.type == EventType.MouseDown
                && Event.current.clickCount == 2
                && rowRect.Contains(Event.current.mousePosition))
            {
                Selection.activeObject = item;
                Event.current.Use();
            }
        }

        // ─────────────────────── FOOTER ───────────────────────

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Expand All", GUILayout.Height(20)))
                foreach (var r in RarityOrder) _foldouts[r] = true;

            if (GUILayout.Button("Collapse All", GUILayout.Height(20)))
                foreach (var r in RarityOrder) _foldouts[r] = false;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_showRawList ? "Hide Raw List" : "Show Raw List",
                    GUILayout.Height(20), GUILayout.Width(110)))
                _showRawList = !_showRawList;

            EditorGUILayout.EndHorizontal();

            if (_showRawList)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(_itemsProp, new GUIContent("Items (Raw)"), true);
            }
        }

        // ─────────────────────── HELPERS ───────────────────────

        private static void DrawCellCentered(string text, float width, Color color,
            int fontSize, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = fontSize,
                alignment = align,
                normal = { textColor = color },
                wordWrap = false
            };
            GUILayout.Label(text, style, GUILayout.Width(width), GUILayout.Height(ROW_HEIGHT));
        }

        // ─────────────────────── COLORS / ICONS ───────────────────────

        private static Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:    return new Color(0.70f, 0.70f, 0.70f);
                case ItemRarity.Uncommon:  return new Color(0.30f, 0.90f, 0.30f);
                case ItemRarity.Rare:      return new Color(0.30f, 0.50f, 1.00f);
                case ItemRarity.Epic:      return new Color(0.70f, 0.30f, 1.00f);
                case ItemRarity.Legendary: return new Color(1.00f, 0.75f, 0.15f);
                default:                   return Color.white;
            }
        }

        private static string GetRaritySymbol(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:    return "●";
                case ItemRarity.Uncommon:  return "◆";
                case ItemRarity.Rare:      return "★";
                case ItemRarity.Epic:      return "✦";
                case ItemRarity.Legendary: return "♛";
                default:                   return "○";
            }
        }

        private static Color GetTypeColor(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon: return new Color(1.0f, 0.4f, 0.3f);
                case ItemType.Arms:   return new Color(0.9f, 0.7f, 0.3f);
                case ItemType.Body:   return new Color(0.3f, 0.7f, 1.0f);
                case ItemType.Feet:   return new Color(0.4f, 0.9f, 0.5f);
                default:              return new Color(0.6f, 0.6f, 0.6f);
            }
        }

        private static string GetTypeIcon(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon: return "⚔️";
                case ItemType.Arms:   return "🧤";
                case ItemType.Body:   return "🛡️";
                case ItemType.Feet:   return "👢";
                default:              return "📦";
            }
        }

        private static string GetTypeShort(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon: return "WPN";
                case ItemType.Arms:   return "ARM";
                case ItemType.Body:   return "BDY";
                case ItemType.Feet:   return "FT";
                default:              return "?";
            }
        }

        private static string GetTypeName(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon: return "WEAPONS";
                case ItemType.Arms:   return "GLOVES";
                case ItemType.Body:   return "BODY ARMOR";
                case ItemType.Feet:   return "FOOTWEAR";
                default:              return type.ToString().ToUpper();
            }
        }

        private static string GetStatIcon(StatType stat)
        {
            switch (stat)
            {
                case StatType.CoinsPerTap:          return "🪙";
                case StatType.XpPerTap:             return "⭐";
                case StatType.CapacityHits:         return "⚡";
                case StatType.RecoverHitsPerSecond: return "🔄";
                case StatType.CritChance:           return "🎯";
                case StatType.CritMultiplier:       return "💥";
                case StatType.OfflineBonusPercent:  return "💤";
                default:                            return "📊";
            }
        }

        private static void DrawBorder(Rect rect, Color color, float w)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, w), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - w, rect.width, w), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, w, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - w, rect.y, w, rect.height), color);
        }
    }
}
#endif

