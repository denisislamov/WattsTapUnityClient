#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using WattsTap.Core.API;
using WattsTap.Core.Configs.Telegram;
using WattsTap.Core.Inventory;

namespace WattsTap.Core.Editor
{
    /// <summary>
    /// Editor window to fetch the game catalog and inventory from the Core Server
    /// and import them as ScriptableObject assets.
    /// Menu: WattsTap → Import Catalog from Server
    /// </summary>
    public class CatalogImporterWindow : EditorWindow
    {
        private const string DEFAULT_BASE_URL = "https://api-dev.wattstap.energy";
        private const string PREFS_BASE_URL = "CatalogImporter_BaseUrl";
        private const string PREFS_TOKEN = "CatalogImporter_Token";
        private const string ITEMS_OUTPUT_FOLDER = "Assets/WattsTap/Configs/Catalog/Items";
        private const string CATALOG_CONFIG_PATH = "Assets/WattsTap/Configs/Catalog/CatalogConfig.asset";

        private string _baseUrl;
        private string _authToken;
        private string _initData;
        private string _statusLog = "";

        private Vector2 _scrollPos;
        private Vector2 _logScrollPos;

        private UnityWebRequest _activeRequest;
        private bool _isRequestInProgress;
        private Action<string> _onRequestDone;

        // Fetched data
        private CatalogResponse _fetchedCatalog;
        private InventoryResponse _fetchedInventory;

        [MenuItem("WattsTap/Import Catalog from Server")]
        public static void ShowWindow()
        {
            var window = GetWindow<CatalogImporterWindow>("Catalog Importer");
            window.minSize = new Vector2(500, 600);
        }

        private void OnEnable()
        {
            _baseUrl = EditorPrefs.GetString(PREFS_BASE_URL, DEFAULT_BASE_URL);
            _authToken = EditorPrefs.GetString(PREFS_TOKEN, "");

            // Try to load initData from TelegramDebugData SO
            TryLoadInitData();
        }

        private void TryLoadInitData()
        {
            string[] guids = AssetDatabase.FindAssets("t:TelegramDebugData");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<TelegramDebugData>(path);
                if (data != null && !string.IsNullOrEmpty(data.InitData))
                {
                    _initData = data.InitData;
                    Log($"Loaded initData from {path} (user: {data.UserId})");
                    return;
                }
            }
            _initData = "";
            Log("Warning: No TelegramDebugData found. Enter initData manually or create one.");
        }

        private void Update()
        {
            if (_isRequestInProgress && _activeRequest != null && _activeRequest.isDone)
            {
                string body = _activeRequest.downloadHandler?.text ?? "";
                bool isError = _activeRequest.result != UnityWebRequest.Result.Success;

                if (isError)
                {
                    Log($"Request FAILED [{_activeRequest.responseCode}]: {_activeRequest.error}\n{body}");
                }

                _isRequestInProgress = false;
                var callback = _onRequestDone;
                _activeRequest.Dispose();
                _activeRequest = null;
                _onRequestDone = null;
                callback?.Invoke(isError ? null : body);

                Repaint();
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("Catalog Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // ── Settings ──
            EditorGUILayout.LabelField("Server Settings", EditorStyles.boldLabel);
            _baseUrl = EditorGUILayout.TextField("Base URL", _baseUrl);
            if (GUI.changed) EditorPrefs.SetString(PREFS_BASE_URL, _baseUrl);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Init Data (from TelegramDebugData):");
            _initData = EditorGUILayout.TextArea(_initData, GUILayout.Height(60));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Auth Token: {(string.IsNullOrEmpty(_authToken) ? "NOT SET" : _authToken.Substring(0, Math.Min(30, _authToken.Length)) + "...")}");

            EditorGUILayout.Space(10);

            // ── Actions ──
            using (new EditorGUI.DisabledScope(_isRequestInProgress))
            {
                // Auth
                if (GUILayout.Button("1. Authenticate", GUILayout.Height(30)))
                {
                    Authenticate();
                }

                EditorGUILayout.Space(5);

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_authToken)))
                {
                    // Fetch Catalog
                    if (GUILayout.Button("2. Fetch Catalog from Server", GUILayout.Height(30)))
                    {
                        FetchCatalog();
                    }

                    // Import to SOs
                    using (new EditorGUI.DisabledScope(_fetchedCatalog == null))
                    {
                        if (GUILayout.Button("3. Import Catalog → ScriptableObjects", GUILayout.Height(30)))
                        {
                            ImportCatalogToSO();
                        }
                    }

                    EditorGUILayout.Space(5);

                    // Fetch Inventory
                    if (GUILayout.Button("Fetch Inventory from Server", GUILayout.Height(25)))
                    {
                        FetchInventory();
                    }

                    // Show inventory info
                    if (_fetchedInventory != null)
                    {
                        EditorGUILayout.HelpBox(
                            $"Inventory: {_fetchedInventory.inventory?.Count ?? 0} items\n" +
                            $"Coins: {_fetchedInventory.currencies?.coins ?? 0}, Drawings: {_fetchedInventory.currencies?.drawings ?? 0}",
                            MessageType.Info);
                    }
                }

                EditorGUILayout.Space(5);

                // One-click: Auth + Fetch + Import
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                if (GUILayout.Button("⚡ Auth + Fetch + Import All", GUILayout.Height(35)))
                {
                    AuthFetchImportAll();
                }
            }

            if (_isRequestInProgress)
            {
                EditorGUILayout.HelpBox("Request in progress...", MessageType.Info);
            }

            // ── Log ──
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
            _logScrollPos = EditorGUILayout.BeginScrollView(_logScrollPos, GUILayout.Height(200));
            EditorGUILayout.TextArea(_statusLog, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear Log"))
            {
                _statusLog = "";
            }

            EditorGUILayout.EndScrollView();
        }

        // ─────────────────────────────────────────
        //  Actions
        // ─────────────────────────────────────────

        private void Authenticate()
        {
            if (string.IsNullOrEmpty(_initData))
            {
                Log("ERROR: initData is empty. Set it in TelegramDebugData or paste manually.");
                return;
            }

            Log("Authenticating...");
            string json = JsonUtility.ToJson(new TelegramAuthRequest { initData = _initData, referralCode = "" });
            PostRequest("/auth/telegram", json, false, body =>
            {
                if (body == null) return;

                var resp = JsonUtility.FromJson<AuthResponse>(body);
                if (resp != null && !string.IsNullOrEmpty(resp.token))
                {
                    _authToken = resp.token;
                    EditorPrefs.SetString(PREFS_TOKEN, _authToken);
                    Log($"Auth OK! Player: {resp.player?.playerId}, expiresIn: {resp.expiresIn}s");
                }
                else
                {
                    Log("Auth failed: no token in response.");
                }
            });
        }

        private void FetchCatalog()
        {
            Log("Fetching catalog...");
            GetRequest("/game/items/catalog", body =>
            {
                if (body == null) return;

                _fetchedCatalog = JsonUtility.FromJson<CatalogResponse>(body);
                if (_fetchedCatalog?.items != null)
                {
                    int variantCount = 0;
                    foreach (var item in _fetchedCatalog.items)
                        variantCount += item.variants?.Count ?? 0;
                    Log($"Catalog fetched: {_fetchedCatalog.items.Count} items, {variantCount} variants total.");
                }
                else
                {
                    Log("Catalog fetch: empty or parse error. Trying Newtonsoft...");
                    // Fallback: try manual parse for the wrapper
                    TryParseCatalogManual(body);
                }
            });
        }

        private void TryParseCatalogManual(string json)
        {
            // JsonUtility doesn't handle null values well. Try a basic workaround:
            // Replace "null" with 0 for numeric fields
            string cleaned = json.Replace(":null", ":0").Replace(": null", ": 0");
            _fetchedCatalog = JsonUtility.FromJson<CatalogResponse>(cleaned);
            if (_fetchedCatalog?.items != null && _fetchedCatalog.items.Count > 0)
            {
                int variantCount = 0;
                foreach (var item in _fetchedCatalog.items)
                    variantCount += item.variants?.Count ?? 0;
                Log($"Catalog parsed (with null→0 cleanup): {_fetchedCatalog.items.Count} items, {variantCount} variants.");
            }
            else
            {
                Log("ERROR: Could not parse catalog even after cleanup.");
            }
        }

        private void FetchInventory()
        {
            Log("Fetching inventory...");
            GetRequest("/game/inventory", body =>
            {
                if (body == null) return;

                string cleaned = body.Replace(":null", ":0").Replace(": null", ": 0");
                _fetchedInventory = JsonUtility.FromJson<InventoryResponse>(cleaned);
                if (_fetchedInventory != null)
                {
                    Log($"Inventory fetched: {_fetchedInventory.inventory?.Count ?? 0} items, " +
                        $"coins={_fetchedInventory.currencies?.coins ?? 0}");
                }
                else
                {
                    Log("ERROR: Could not parse inventory response.");
                }
            });
        }

        private void ImportCatalogToSO()
        {
            if (_fetchedCatalog?.items == null || _fetchedCatalog.items.Count == 0)
            {
                Log("ERROR: No catalog data to import. Fetch first.");
                return;
            }

            Log("Importing catalog to ScriptableObjects...");

            // Ensure output directory exists
            if (!AssetDatabase.IsValidFolder(ITEMS_OUTPUT_FOLDER))
            {
                string parent = Path.GetDirectoryName(ITEMS_OUTPUT_FOLDER).Replace("\\", "/");
                string folder = Path.GetFileName(ITEMS_OUTPUT_FOLDER);
                AssetDatabase.CreateFolder(parent, folder);
            }

            var allItemAssets = new List<ItemData>();
            int created = 0;
            int updated = 0;

            foreach (var template in _fetchedCatalog.items)
            {
                if (template.variants == null) continue;
                var itemType = ItemTypeExtensions.FromServerSlot(template.slot);

                foreach (var variant in template.variants)
                {
                    string assetName = $"{template.code}_{variant.rarity}";
                    string assetPath = $"{ITEMS_OUTPUT_FOLDER}/{assetName}.asset";

                    var levels = ConvertLevels(variant.levels);
                    var bonuses = ConvertBonuses(variant.bonuses);
                    var rarity = ItemRarityExtensions.FromServerString(variant.rarity);
                    var mainStat = StatTypeExtensions.FromServerString(variant.mainStatType);

                    // Try to load existing asset
                    var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
                    if (itemData == null)
                    {
                        itemData = ScriptableObject.CreateInstance<ItemData>();
                        itemData.PopulateFromServer(
                            template.id, template.code, template.name, template.description,
                            itemType, variant.id, rarity, mainStat, variant.maxLevel,
                            levels, bonuses);
                        AssetDatabase.CreateAsset(itemData, assetPath);
                        created++;
                    }
                    else
                    {
                        itemData.PopulateFromServer(
                            template.id, template.code, template.name, template.description,
                            itemType, variant.id, rarity, mainStat, variant.maxLevel,
                            levels, bonuses);
                        EditorUtility.SetDirty(itemData);
                        updated++;
                    }

                    allItemAssets.Add(itemData);
                }
            }

            // Update CatalogConfig
            var catalogConfig = AssetDatabase.LoadAssetAtPath<CatalogConfig>(CATALOG_CONFIG_PATH);
            if (catalogConfig != null)
            {
                // Use SerializedObject to update the items list
                var so = new SerializedObject(catalogConfig);
                var itemsProp = so.FindProperty("_items");
                itemsProp.ClearArray();
                for (int i = 0; i < allItemAssets.Count; i++)
                {
                    itemsProp.InsertArrayElementAtIndex(i);
                    itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = allItemAssets[i];
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(catalogConfig);
                Log($"Updated CatalogConfig with {allItemAssets.Count} items.");
            }
            else
            {
                Log($"WARNING: CatalogConfig not found at {CATALOG_CONFIG_PATH}. Create it manually.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Log($"Import complete! Created: {created}, Updated: {updated}, Total: {allItemAssets.Count}");
        }

        private void AuthFetchImportAll()
        {
            if (string.IsNullOrEmpty(_initData))
            {
                Log("ERROR: initData is empty.");
                return;
            }

            Log("=== Starting Auth + Fetch + Import pipeline ===");
            string json = JsonUtility.ToJson(new TelegramAuthRequest { initData = _initData, referralCode = "" });
            PostRequest("/auth/telegram", json, false, authBody =>
            {
                if (authBody == null) { Log("Pipeline aborted: auth failed."); return; }

                var resp = JsonUtility.FromJson<AuthResponse>(authBody);
                if (resp == null || string.IsNullOrEmpty(resp.token))
                {
                    Log("Pipeline aborted: no token.");
                    return;
                }

                _authToken = resp.token;
                EditorPrefs.SetString(PREFS_TOKEN, _authToken);
                Log($"Auth OK! Fetching catalog...");

                GetRequest("/game/items/catalog", catalogBody =>
                {
                    if (catalogBody == null) { Log("Pipeline aborted: catalog fetch failed."); return; }

                    string cleaned = catalogBody.Replace(":null", ":0").Replace(": null", ": 0");
                    _fetchedCatalog = JsonUtility.FromJson<CatalogResponse>(cleaned);

                    if (_fetchedCatalog?.items == null || _fetchedCatalog.items.Count == 0)
                    {
                        Log("Pipeline aborted: empty catalog.");
                        return;
                    }

                    Log($"Catalog fetched ({_fetchedCatalog.items.Count} items). Importing to SOs...");
                    ImportCatalogToSO();

                    // Also fetch inventory
                    GetRequest("/game/inventory", invBody =>
                    {
                        if (invBody != null)
                        {
                            string cleanedInv = invBody.Replace(":null", ":0").Replace(": null", ": 0");
                            _fetchedInventory = JsonUtility.FromJson<InventoryResponse>(cleanedInv);
                            Log($"Inventory fetched: {_fetchedInventory?.inventory?.Count ?? 0} items.");
                        }
                        Log("=== Pipeline complete! ===");
                    });
                });
            });
        }

        // ─────────────────────────────────────────
        //  HTTP Helpers
        // ─────────────────────────────────────────

        private void GetRequest(string endpoint, Action<string> onDone)
        {
            var www = UnityWebRequest.Get(_baseUrl + endpoint);
            www.timeout = 30;
            if (!string.IsNullOrEmpty(_authToken))
                www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            www.SetRequestHeader("User-Agent", "Unity-CatalogImporter");
            StartRequest(www, onDone);
        }

        private void PostRequest(string endpoint, string jsonBody, bool useAuth, Action<string> onDone)
        {
            var www = new UnityWebRequest(_baseUrl + endpoint, "POST");
            www.timeout = 30;
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("User-Agent", "Unity-CatalogImporter");
            if (useAuth && !string.IsNullOrEmpty(_authToken))
                www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            StartRequest(www, onDone);
        }

        private void StartRequest(UnityWebRequest www, Action<string> onDone)
        {
            _isRequestInProgress = true;
            _onRequestDone = onDone;
            _activeRequest = www;
            _activeRequest.SendWebRequest();
        }

        // ─────────────────────────────────────────
        //  Converters (same as CatalogService but accessible in editor)
        // ─────────────────────────────────────────

        private static List<ItemLevelData> ConvertLevels(List<CatalogLevelDTO> dtos)
        {
            var result = new List<ItemLevelData>();
            if (dtos == null) return result;
            foreach (var dto in dtos)
            {
                result.Add(new ItemLevelData
                {
                    Level = dto.level,
                    Value = dto.value,
                    ValuePercent = dto.valuePercent,
                });
            }
            return result;
        }

        private static List<ItemBonusData> ConvertBonuses(List<CatalogBonusDTO> dtos)
        {
            var result = new List<ItemBonusData>();
            if (dtos == null) return result;
            foreach (var dto in dtos)
            {
                result.Add(new ItemBonusData
                {
                    StatType = StatTypeExtensions.FromServerString(dto.statType),
                    Value = dto.value,
                    ValuePercent = dto.valuePercent,
                    Description = dto.description,
                });
            }
            return result;
        }

        private void Log(string message)
        {
            _statusLog += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            Debug.Log($"[CatalogImporter] {message}");
            Repaint();
        }
    }
}
#endif

