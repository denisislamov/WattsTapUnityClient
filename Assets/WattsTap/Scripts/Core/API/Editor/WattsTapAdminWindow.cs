#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace WattsTap.Core.API.Editor
{
    /// <summary>
    /// Unity Editor window — Admin API Console (Postman-like) for WattsTap admin endpoints.
    /// Menu: WattsTap → Admin API Console
    /// </summary>
    public class WattsTapAdminWindow : EditorWindow
    {
        #region Constants

        private const string DEFAULT_BASE_URL = "https://api-dev.wattstap.energy";
        private const string PREFS_BASE_URL = "WattsTapAdmin_BaseUrl";
        private const string PREFS_ADMIN_TOKEN = "WattsTapAdmin_AdminToken";
        private const string PREFS_HISTORY = "WattsTapAdmin_History";
        private const int MAX_HISTORY = 30;

        #endregion

        #region Enums

        private enum AdminCategory
        {
            General,
            GameConfig,
            User,
            Avatars,
            Orders,
            Boosters,
            ChestsConfig,
            ChestsPools,
            ChestsGrants
        }

        private enum HttpMethod
        {
            GET,
            POST,
            PUT,
            PATCH,
            DELETE
        }

        #endregion

        #region Fields

        // Settings
        private string _baseUrl;
        private string _adminToken;

        // UI state
        private Vector2 _mainScrollPos;
        private Vector2 _responseScrollPos;
        private Vector2 _bodyScrollPos;
        private Vector2 _historyScrollPos;
        private AdminCategory _selectedCategory = AdminCategory.General;
        private bool _showSettings = true;
        private bool _showHistory;
        private bool _showResponseHeaders;
        private bool _isRequestInProgress;
        private float _requestStartTime;

        // Response
        private string _lastResponseBody = "";
        private long _lastResponseCode;
        private string _lastResponseHeaders = "";
        private string _lastRequestInfo = "";
        private float _lastRequestDuration;
        private bool _lastResponseIsError;

        // Active request
        private UnityWebRequest _activeRequest;
        private bool _autoExtractTokenPending;

        // ─── Per-endpoint input fields ───────────────────────────
        // General
        private string _testUserId = "";
        private string _testBody = "{}";

        // Game Config
        private string _gameConfigId = "";
        private string _gameConfigBody = "{\n  \"key\": \"value\"\n}";

        // User
        private string _createUserBody = "{\n  \"telegramId\": 123456,\n  \"username\": \"testuser\"\n}";

        // Avatars
        private string _avatarCreateBody = "{\n  \"name\": \"avatar1\",\n  \"imageUrl\": \"https://...\"\n}";
        private string _avatarPatchId = "";
        private string _avatarPatchBody = "{\n  \"name\": \"updated\"\n}";

        // Orders
        private string _orderId = "";

        // Boosters
        private string _boosterId = "";
        private string _boosterCreateBody = "{\n  \"type\": \"multiplier\",\n  \"value\": 2,\n  \"durationSec\": 3600\n}";
        private string _boosterPatchBody = "{\n  \"value\": 3\n}";

        // Chests Config
        private string _chestConfigId = "";
        private string _chestConfigBody = "{\n  \"name\": \"chest_v1\"\n}";
        private string _chestConfigPatchBody = "{\n  \"name\": \"updated\"\n}";
        private string _chestBagCode = "";
        private string _chestSlotsBody = "[\n  { \"slotIndex\": 0, \"poolId\": \"pool1\" }\n]";
        private string _chestSimulateBody = "{\n  \"count\": 10\n}";

        // Chests Pools
        private string _chestPoolId = "";
        private string _chestPoolCreateBody = "{\n  \"name\": \"pool1\"\n}";
        private string _chestPoolPatchBody = "{\n  \"name\": \"updated_pool\"\n}";
        private string _chestPoolEntryId = "";
        private string _chestPoolEntryCreateBody = "{\n  \"itemId\": \"item1\",\n  \"weight\": 100\n}";
        private string _chestPoolEntryPatchBody = "{\n  \"weight\": 200\n}";

        // Chests Grants
        private string _chestGrantBody = "{\n  \"playerId\": \"player1\",\n  \"configId\": \"cfg1\",\n  \"count\": 1\n}";
        private string _chestPlayerId = "";

        // History
        private List<HistoryEntry> _history = new List<HistoryEntry>();

        [Serializable]
        private class HistoryEntry
        {
            public string method;
            public string url;
            public string body;
            public long statusCode;
            public string timestamp;
        }

        [Serializable]
        private class HistoryList
        {
            public List<HistoryEntry> items = new List<HistoryEntry>();
        }

        #endregion

        #region Menu Item

        [MenuItem("WattsTap/Admin API Console")]
        public static void ShowWindow()
        {
            var window = GetWindow<WattsTapAdminWindow>("Admin API Console");
            window.minSize = new Vector2(620, 700);
            window.Show();
        }

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            _baseUrl = EditorPrefs.GetString(PREFS_BASE_URL, DEFAULT_BASE_URL);
            _adminToken = EditorPrefs.GetString(PREFS_ADMIN_TOKEN, "");
            LoadHistory();
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(PREFS_BASE_URL, _baseUrl);
            EditorPrefs.SetString(PREFS_ADMIN_TOKEN, _adminToken);
            SaveHistory();
            AbortActiveRequest();
        }

        private void Update()
        {
            if (_isRequestInProgress && _activeRequest != null && _activeRequest.isDone)
            {
                ProcessResponse(_activeRequest);
                _activeRequest.Dispose();
                _activeRequest = null;
                _isRequestInProgress = false;
                Repaint();
            }

            if (_isRequestInProgress)
            {
                Repaint();
            }
        }

        #endregion

        #region Main GUI

        private void OnGUI()
        {
            _mainScrollPos = EditorGUILayout.BeginScrollView(_mainScrollPos);

            DrawToolbar();
            EditorGUILayout.Space(4);
            DrawSettingsSection();
            EditorGUILayout.Space(4);
            DrawCategoryTabs();
            EditorGUILayout.Space(4);
            DrawEndpointsForCategory();
            EditorGUILayout.Space(8);
            DrawResponseSection();
            EditorGUILayout.Space(4);
            DrawHistorySection();

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Toolbar

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("🔧 Admin API Console", EditorStyles.boldLabel, GUILayout.Width(180));
            GUILayout.FlexibleSpace();

            if (_isRequestInProgress)
            {
                float elapsed = (float)(EditorApplication.timeSinceStartup - _requestStartTime);
                GUILayout.Label($"⏳ Requesting... {elapsed:F1}s", EditorStyles.miniLabel);

                if (GUILayout.Button("Abort", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    AbortActiveRequest();
                }
            }
            else
            {
                var statusStyle = new GUIStyle(EditorStyles.miniLabel);
                if (!string.IsNullOrEmpty(_adminToken))
                {
                    statusStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
                    GUILayout.Label("🔑 Token Set", statusStyle);
                }
                else
                {
                    statusStyle.normal.textColor = new Color(0.8f, 0.4f, 0.2f);
                    GUILayout.Label("🔒 No Admin Token", statusStyle);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Settings Section

        private void DrawSettingsSection()
        {
            _showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showSettings, "⚙ Settings");

            if (_showSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Base URL
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Base URL", GUILayout.Width(80));
                _baseUrl = EditorGUILayout.TextField(_baseUrl);
                if (GUILayout.Button("Reset", GUILayout.Width(50)))
                    _baseUrl = DEFAULT_BASE_URL;
                EditorGUILayout.EndHorizontal();

                // Admin Token
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Admin Token", GUILayout.Width(80));
                _adminToken = EditorGUILayout.TextField(_adminToken);
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                    _adminToken = "";
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox(
                    "ADMIN_TOKEN — JWT-токен для авторизации админских запросов.\nДобавляется в заголовок: Authorization: Bearer <token>",
                    MessageType.Info);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Category Tabs

        private void DrawCategoryTabs()
        {
            var categories = (AdminCategory[])Enum.GetValues(typeof(AdminCategory));
            int perRow = 5;

            for (int row = 0; row < Mathf.CeilToInt(categories.Length / (float)perRow); row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < perRow; col++)
                {
                    int idx = row * perRow + col;
                    if (idx >= categories.Length) break;

                    var cat = categories[idx];
                    bool isSelected = _selectedCategory == cat;
                    var style = isSelected ? GetSelectedTabStyle() : EditorStyles.miniButton;

                    string label = cat.ToString();
                    // Nicer labels
                    switch (cat)
                    {
                        case AdminCategory.GameConfig: label = "Game Config"; break;
                        case AdminCategory.ChestsConfig: label = "Chests Cfg"; break;
                        case AdminCategory.ChestsPools: label = "Chests Pools"; break;
                        case AdminCategory.ChestsGrants: label = "Chests Grants"; break;
                    }

                    if (GUILayout.Button(label, style, GUILayout.Height(24)))
                    {
                        _selectedCategory = cat;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private GUIStyle GetSelectedTabStyle()
        {
            var style = new GUIStyle(EditorStyles.miniButton);
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(0.8f, 0.3f, 0.1f));
            tex.Apply();
            style.normal.background = tex;
            return style;
        }

        #endregion

        #region Endpoints Drawing

        private void DrawEndpointsForCategory()
        {
            switch (_selectedCategory)
            {
                case AdminCategory.General:
                    DrawGeneralEndpoints();
                    break;
                case AdminCategory.GameConfig:
                    DrawGameConfigEndpoints();
                    break;
                case AdminCategory.User:
                    DrawUserEndpoints();
                    break;
                case AdminCategory.Avatars:
                    DrawAvatarsEndpoints();
                    break;
                case AdminCategory.Orders:
                    DrawOrdersEndpoints();
                    break;
                case AdminCategory.Boosters:
                    DrawBoostersEndpoints();
                    break;
                case AdminCategory.ChestsConfig:
                    DrawChestsConfigEndpoints();
                    break;
                case AdminCategory.ChestsPools:
                    DrawChestsPoolsEndpoints();
                    break;
                case AdminCategory.ChestsGrants:
                    DrawChestsGrantsEndpoints();
                    break;
            }
        }

        // ─── General ─────────────────────────────────────────────
        private void DrawGeneralEndpoints()
        {
            DrawSectionHeader("Admin Test");

            // POST /admin/test/:userId
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/test/:userId", "Run admin test for a user");

            DrawParamField("User ID", ref _testUserId);
            DrawBodyField(ref _testBody, 60);

            DrawSendButtonWithCurl("Send Test", () =>
            {
                SendRequest(HttpMethod.POST, $"/admin/test/{Uri.EscapeDataString(_testUserId)}", _testBody);
            }, () => BuildCurl("POST", $"/admin/test/{Uri.EscapeDataString(_testUserId)}", _testBody));
            EditorGUILayout.EndVertical();
        }

        // ─── Game Config ─────────────────────────────────────────
        private void DrawGameConfigEndpoints()
        {
            DrawSectionHeader("Game Configuration");

            // GET /admin/game/configs
            DrawSimpleEndpoint("GET", "/admin/game/configs", "List all game configs");

            EditorGUILayout.Space(4);

            // GET /admin/game/config/:id
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("GET", "/admin/game/config/:id", "Get game config by ID");
            DrawParamField("Config ID", ref _gameConfigId);
            DrawSendButtonWithCurl("Get Config", () =>
            {
                SendRequest(HttpMethod.GET, $"/admin/game/config/{Uri.EscapeDataString(_gameConfigId)}");
            }, () => BuildCurl("GET", $"/admin/game/config/{Uri.EscapeDataString(_gameConfigId)}"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /admin/game/config
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/game/config", "Create new game config");
            DrawBodyField(ref _gameConfigBody, 80);
            DrawSendButtonWithCurl("Create Config", () =>
            {
                SendRequest(HttpMethod.POST, "/admin/game/config", _gameConfigBody);
            }, () => BuildCurl("POST", "/admin/game/config", _gameConfigBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // PUT /admin/game/config/:id
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("PUT", "/admin/game/config/:id", "Update game config by ID");
            DrawParamField("Config ID", ref _gameConfigId);
            DrawBodyField(ref _gameConfigBody, 80);
            DrawSendButtonWithCurl("Update Config", () =>
            {
                SendRequest(HttpMethod.PUT, $"/admin/game/config/{Uri.EscapeDataString(_gameConfigId)}", _gameConfigBody);
            }, () => BuildCurl("PUT", $"/admin/game/config/{Uri.EscapeDataString(_gameConfigId)}", _gameConfigBody));
            EditorGUILayout.EndVertical();
        }

        // ─── User ────────────────────────────────────────────────
        private void DrawUserEndpoints()
        {
            DrawSectionHeader("Admin User Management");

            // POST /admin/user
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/user", "Create a new user (admin)");
            DrawBodyField(ref _createUserBody, 80);
            DrawSendButtonWithCurl("Create User", () =>
            {
                SendRequest(HttpMethod.POST, "/admin/user", _createUserBody);
            }, () => BuildCurl("POST", "/admin/user", _createUserBody));
            EditorGUILayout.EndVertical();
        }

        // ─── Avatars ─────────────────────────────────────────────
        private void DrawAvatarsEndpoints()
        {
            DrawSectionHeader("Admin Avatars");

            // POST /admin/avatars
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/avatars", "Create a new avatar");
            DrawBodyField(ref _avatarCreateBody, 80);
            DrawSendButtonWithCurl("Create Avatar", () =>
            {
                SendRequest(HttpMethod.POST, "/admin/avatars", _avatarCreateBody);
            }, () => BuildCurl("POST", "/admin/avatars", _avatarCreateBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // PATCH /admin/avatars/:id
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("PATCH", "/admin/avatars/:id", "Update avatar by ID");
            DrawParamField("Avatar ID", ref _avatarPatchId);
            DrawBodyField(ref _avatarPatchBody, 60);
            DrawSendButtonWithCurl("Update Avatar", () =>
            {
                SendRequest(HttpMethod.PATCH, $"/admin/avatars/{Uri.EscapeDataString(_avatarPatchId)}", _avatarPatchBody);
            }, () => BuildCurl("PATCH", $"/admin/avatars/{Uri.EscapeDataString(_avatarPatchId)}", _avatarPatchBody));
            EditorGUILayout.EndVertical();
        }

        // ─── Orders ──────────────────────────────────────────────
        private void DrawOrdersEndpoints()
        {
            DrawSectionHeader("Admin Orders");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawParamField("Order ID", ref _orderId);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /admin/orders/:id/fulfill
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/orders/:id/fulfill", "Fulfill an order");
            DrawSendButtonWithCurl("Fulfill Order", () =>
            {
                SendRequest(HttpMethod.POST, $"/admin/orders/{Uri.EscapeDataString(_orderId)}/fulfill", "{}");
            }, () => BuildCurl("POST", $"/admin/orders/{Uri.EscapeDataString(_orderId)}/fulfill", "{}"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /admin/orders/:id/cancel
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/orders/:id/cancel", "Cancel an order");
            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
            DrawSendButtonWithCurl("Cancel Order", () =>
            {
                if (EditorUtility.DisplayDialog("Cancel Order",
                    $"Are you sure you want to cancel order '{_orderId}'?", "Yes, Cancel", "No"))
                {
                    SendRequest(HttpMethod.POST, $"/admin/orders/{Uri.EscapeDataString(_orderId)}/cancel", "{}");
                }
            }, () => BuildCurl("POST", $"/admin/orders/{Uri.EscapeDataString(_orderId)}/cancel", "{}"));
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        // ─── Boosters ────────────────────────────────────────────
        private void DrawBoostersEndpoints()
        {
            DrawSectionHeader("Admin Boosters");

            // GET /admin/boosters
            DrawSimpleEndpoint("GET", "/admin/boosters", "List all boosters");

            EditorGUILayout.Space(4);

            // POST /admin/boosters
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/boosters", "Create a new booster");
            DrawBodyField(ref _boosterCreateBody, 80);
            DrawSendButtonWithCurl("Create Booster", () =>
            {
                SendRequest(HttpMethod.POST, "/admin/boosters", _boosterCreateBody);
            }, () => BuildCurl("POST", "/admin/boosters", _boosterCreateBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawParamField("Booster ID", ref _boosterId);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // PATCH /admin/boosters/:id
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("PATCH", "/admin/boosters/:id", "Update booster");
            DrawBodyField(ref _boosterPatchBody, 60);
            DrawSendButtonWithCurl("Update Booster", () =>
            {
                SendRequest(HttpMethod.PATCH, $"/admin/boosters/{Uri.EscapeDataString(_boosterId)}", _boosterPatchBody);
            }, () => BuildCurl("PATCH", $"/admin/boosters/{Uri.EscapeDataString(_boosterId)}", _boosterPatchBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /admin/boosters/:id/activate
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/boosters/:id/activate", "Activate booster");
            DrawSendButtonWithCurl("Activate", () =>
            {
                SendRequest(HttpMethod.POST, $"/admin/boosters/{Uri.EscapeDataString(_boosterId)}/activate", "{}");
            }, () => BuildCurl("POST", $"/admin/boosters/{Uri.EscapeDataString(_boosterId)}/activate", "{}"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2);

            // POST /admin/boosters/:id/deactivate
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/boosters/:id/deactivate", "Deactivate booster");
            DrawSendButtonWithCurl("Deactivate", () =>
            {
                SendRequest(HttpMethod.POST, $"/admin/boosters/{Uri.EscapeDataString(_boosterId)}/deactivate", "{}");
            }, () => BuildCurl("POST", $"/admin/boosters/{Uri.EscapeDataString(_boosterId)}/deactivate", "{}"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2);

            // DELETE /admin/boosters/:id
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("DELETE", "/admin/boosters/:id", "Delete booster");
            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
            DrawSendButtonWithCurl("⚠ Delete Booster", () =>
            {
                if (EditorUtility.DisplayDialog("Delete Booster",
                    $"Are you sure you want to delete booster '{_boosterId}'?", "Yes, Delete", "Cancel"))
                {
                    SendRequest(HttpMethod.DELETE, $"/admin/boosters/{Uri.EscapeDataString(_boosterId)}");
                }
            }, () => BuildCurl("DELETE", $"/admin/boosters/{Uri.EscapeDataString(_boosterId)}"));
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        // ─── Chests Config ───────────────────────────────────────
        private void DrawChestsConfigEndpoints()
        {
            DrawSectionHeader("Chests Configuration");

            // GET /admin/chests/configs
            DrawSimpleEndpoint("GET", "/admin/chests/configs", "List all chest configs");

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Config ID (shared across endpoints below):", EditorStyles.miniLabel);
            DrawParamField("Config ID", ref _chestConfigId);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // GET /admin/chests/configs/:id
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("GET", "/admin/chests/configs/:id", "Get chest config by ID");
            DrawSendButtonWithCurl("Get Config", () =>
            {
                SendRequest(HttpMethod.GET, $"/admin/chests/configs/{U(_chestConfigId)}");
            }, () => BuildCurl("GET", $"/admin/chests/configs/{U(_chestConfigId)}"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /admin/chests/configs
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/chests/configs", "Create new chest config");
            DrawBodyField(ref _chestConfigBody, 60);
            DrawSendButtonWithCurl("Create Config", () =>
            {
                SendRequest(HttpMethod.POST, "/admin/chests/configs", _chestConfigBody);
            }, () => BuildCurl("POST", "/admin/chests/configs", _chestConfigBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // PATCH /admin/chests/configs/:id
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("PATCH", "/admin/chests/configs/:id", "Update chest config");
            DrawBodyField(ref _chestConfigPatchBody, 60);
            DrawSendButtonWithCurl("Update Config", () =>
            {
                SendRequest(HttpMethod.PATCH, $"/admin/chests/configs/{U(_chestConfigId)}", _chestConfigPatchBody);
            }, () => BuildCurl("PATCH", $"/admin/chests/configs/{U(_chestConfigId)}", _chestConfigPatchBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            DrawSectionHeader("Config Lifecycle Actions");

            // Action buttons row
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Quick actions for config ID above:", EditorStyles.miniLabel);

            DrawActionRow("POST", "/admin/chests/configs/:id/clone", "Clone", "clone");
            DrawActionRow("POST", "/admin/chests/configs/:id/validate", "Validate", "validate");
            DrawActionRow("POST", "/admin/chests/configs/:id/publish", "Publish", "publish");
            DrawActionRow("POST", "/admin/chests/configs/:id/archive", "Archive", "archive");
            DrawActionRow("POST", "/admin/chests/configs/:id/load-default-v1", "Load Default v1", "load-default-v1");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            DrawSectionHeader("Config Bags & Slots");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawParamField("Bag Code", ref _chestBagCode);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // GET /admin/chests/configs/:id/bags/:bagCode/slots
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("GET", "/admin/chests/configs/:id/bags/:bagCode/slots", "Get bag slots");
            DrawSendButtonWithCurl("Get Slots", () =>
            {
                SendRequest(HttpMethod.GET, $"/admin/chests/configs/{U(_chestConfigId)}/bags/{U(_chestBagCode)}/slots");
            }, () => BuildCurl("GET", $"/admin/chests/configs/{U(_chestConfigId)}/bags/{U(_chestBagCode)}/slots"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // PUT /admin/chests/configs/:id/bags/:bagCode/slots
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("PUT", "/admin/chests/configs/:id/bags/:bagCode/slots", "Set bag slots");
            DrawBodyField(ref _chestSlotsBody, 60);
            DrawSendButtonWithCurl("Set Slots", () =>
            {
                SendRequest(HttpMethod.PUT, $"/admin/chests/configs/{U(_chestConfigId)}/bags/{U(_chestBagCode)}/slots", _chestSlotsBody);
            }, () => BuildCurl("PUT", $"/admin/chests/configs/{U(_chestConfigId)}/bags/{U(_chestBagCode)}/slots", _chestSlotsBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            DrawSectionHeader("Config Item Pools");

            // GET /admin/chests/configs/:id/item-pools
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("GET", "/admin/chests/configs/:id/item-pools", "List item pools for config");
            DrawSendButtonWithCurl("Get Pools", () =>
            {
                SendRequest(HttpMethod.GET, $"/admin/chests/configs/{U(_chestConfigId)}/item-pools");
            }, () => BuildCurl("GET", $"/admin/chests/configs/{U(_chestConfigId)}/item-pools"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /admin/chests/configs/:id/item-pools
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/chests/configs/:id/item-pools", "Create item pool for config");
            DrawBodyField(ref _chestPoolCreateBody, 60);
            DrawSendButtonWithCurl("Create Pool", () =>
            {
                SendRequest(HttpMethod.POST, $"/admin/chests/configs/{U(_chestConfigId)}/item-pools", _chestPoolCreateBody);
            }, () => BuildCurl("POST", $"/admin/chests/configs/{U(_chestConfigId)}/item-pools", _chestPoolCreateBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /admin/chests/configs/:id/simulate
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/chests/configs/:id/simulate", "Simulate chest drops");
            DrawBodyField(ref _chestSimulateBody, 40);
            DrawSendButtonWithCurl("Simulate", () =>
            {
                SendRequest(HttpMethod.POST, $"/admin/chests/configs/{U(_chestConfigId)}/simulate", _chestSimulateBody);
            }, () => BuildCurl("POST", $"/admin/chests/configs/{U(_chestConfigId)}/simulate", _chestSimulateBody));
            EditorGUILayout.EndVertical();
        }

        // ─── Chests Pools ────────────────────────────────────────
        private void DrawChestsPoolsEndpoints()
        {
            DrawSectionHeader("Item Pools (standalone)");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawParamField("Pool ID", ref _chestPoolId);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // PATCH /admin/chests/item-pools/:poolId
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("PATCH", "/admin/chests/item-pools/:poolId", "Update item pool");
            DrawBodyField(ref _chestPoolPatchBody, 60);
            DrawSendButtonWithCurl("Update Pool", () =>
            {
                SendRequest(HttpMethod.PATCH, $"/admin/chests/item-pools/{U(_chestPoolId)}", _chestPoolPatchBody);
            }, () => BuildCurl("PATCH", $"/admin/chests/item-pools/{U(_chestPoolId)}", _chestPoolPatchBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /admin/chests/item-pools/:poolId/entries
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/chests/item-pools/:poolId/entries", "Add entry to item pool");
            DrawBodyField(ref _chestPoolEntryCreateBody, 60);
            DrawSendButtonWithCurl("Add Entry", () =>
            {
                SendRequest(HttpMethod.POST, $"/admin/chests/item-pools/{U(_chestPoolId)}/entries", _chestPoolEntryCreateBody);
            }, () => BuildCurl("POST", $"/admin/chests/item-pools/{U(_chestPoolId)}/entries", _chestPoolEntryCreateBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            DrawSectionHeader("Item Pool Entries");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawParamField("Entry ID", ref _chestPoolEntryId);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // PATCH /admin/chests/item-pool-entries/:entryId
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("PATCH", "/admin/chests/item-pool-entries/:entryId", "Update pool entry");
            DrawBodyField(ref _chestPoolEntryPatchBody, 60);
            DrawSendButtonWithCurl("Update Entry", () =>
            {
                SendRequest(HttpMethod.PATCH, $"/admin/chests/item-pool-entries/{U(_chestPoolEntryId)}", _chestPoolEntryPatchBody);
            }, () => BuildCurl("PATCH", $"/admin/chests/item-pool-entries/{U(_chestPoolEntryId)}", _chestPoolEntryPatchBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // DELETE /admin/chests/item-pool-entries/:entryId
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("DELETE", "/admin/chests/item-pool-entries/:entryId", "Delete pool entry");
            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
            DrawSendButtonWithCurl("⚠ Delete Entry", () =>
            {
                if (EditorUtility.DisplayDialog("Delete Entry",
                    $"Are you sure you want to delete entry '{_chestPoolEntryId}'?", "Yes, Delete", "Cancel"))
                {
                    SendRequest(HttpMethod.DELETE, $"/admin/chests/item-pool-entries/{U(_chestPoolEntryId)}");
                }
            }, () => BuildCurl("DELETE", $"/admin/chests/item-pool-entries/{U(_chestPoolEntryId)}"));
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        // ─── Chests Grants ───────────────────────────────────────
        private void DrawChestsGrantsEndpoints()
        {
            DrawSectionHeader("Chests Grants & Player Ledger");

            // GET /admin/chests/grants
            DrawSimpleEndpoint("GET", "/admin/chests/grants", "List all chest grants");

            EditorGUILayout.Space(4);

            // POST /admin/chests/grants
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/admin/chests/grants", "Create a chest grant");
            DrawBodyField(ref _chestGrantBody, 80);
            DrawSendButtonWithCurl("Create Grant", () =>
            {
                SendRequest(HttpMethod.POST, "/admin/chests/grants", _chestGrantBody);
            }, () => BuildCurl("POST", "/admin/chests/grants", _chestGrantBody));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            DrawSectionHeader("Player Stock Ledger");

            // GET /admin/chests/players/:playerId/stock-ledger
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("GET", "/admin/chests/players/:playerId/stock-ledger", "Get player's chest stock ledger");
            DrawParamField("Player ID", ref _chestPlayerId);
            DrawSendButtonWithCurl("Get Ledger", () =>
            {
                SendRequest(HttpMethod.GET, $"/admin/chests/players/{U(_chestPlayerId)}/stock-ledger");
            }, () => BuildCurl("GET", $"/admin/chests/players/{U(_chestPlayerId)}/stock-ledger"));
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Response Section

        private void DrawResponseSection()
        {
            EditorGUILayout.LabelField("Response", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (_isRequestInProgress)
            {
                float elapsed = (float)(EditorApplication.timeSinceStartup - _requestStartTime);
                EditorGUILayout.LabelField($"⏳ Request in progress... ({elapsed:F1}s)");
            }
            else if (!string.IsNullOrEmpty(_lastRequestInfo))
            {
                // Request info
                EditorGUILayout.LabelField(_lastRequestInfo, EditorStyles.miniLabel);

                // Status
                EditorGUILayout.BeginHorizontal();
                var statusStyle = new GUIStyle(EditorStyles.boldLabel);
                if (_lastResponseIsError)
                    statusStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
                else
                    statusStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);

                EditorGUILayout.LabelField($"Status: {_lastResponseCode}", statusStyle, GUILayout.Width(150));
                EditorGUILayout.LabelField($"Time: {_lastRequestDuration:F2}s", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Copy Response", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    EditorGUIUtility.systemCopyBuffer = _lastResponseBody;
                    Debug.Log("[Admin Console] Response copied to clipboard.");
                }

                if (GUILayout.Button("Copy Pretty", EditorStyles.miniButton, GUILayout.Width(90)))
                {
                    EditorGUIUtility.systemCopyBuffer = PrettyPrintJson(_lastResponseBody);
                    Debug.Log("[Admin Console] Pretty response copied to clipboard.");
                }

                EditorGUILayout.EndHorizontal();

                // Headers foldout
                _showResponseHeaders = EditorGUILayout.Foldout(_showResponseHeaders, "Response Headers");
                if (_showResponseHeaders && !string.IsNullOrEmpty(_lastResponseHeaders))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(_lastResponseHeaders, EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndVertical();
                }

                // Body
                EditorGUILayout.LabelField("Response Body:", EditorStyles.miniLabel);

                string prettyBody = PrettyPrintJson(_lastResponseBody);
                float bodyHeight = Mathf.Min(Mathf.Max(EditorStyles.wordWrappedMiniLabel.CalcHeight(
                    new GUIContent(prettyBody), position.width - 40), 60), 400);

                _responseScrollPos = EditorGUILayout.BeginScrollView(_responseScrollPos,
                    GUILayout.MinHeight(bodyHeight), GUILayout.MaxHeight(400));

                EditorGUILayout.TextArea(prettyBody, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No requests sent yet. Select a category and use endpoints above.",
                    EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region History Section

        private void DrawHistorySection()
        {
            _showHistory = EditorGUILayout.BeginFoldoutHeaderGroup(_showHistory, $"📜 Request History ({_history.Count})");

            if (_showHistory && _history.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (GUILayout.Button("Clear History", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    _history.Clear();
                    SaveHistory();
                }

                _historyScrollPos = EditorGUILayout.BeginScrollView(_historyScrollPos, GUILayout.MaxHeight(200));

                for (int i = _history.Count - 1; i >= 0; i--)
                {
                    var entry = _history[i];
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    // Method color
                    var methodStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
                    SetMethodColor(methodStyle, entry.method);
                    GUILayout.Label(entry.method, methodStyle, GUILayout.Width(45));

                    // Status badge
                    var statusStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
                    statusStyle.normal.textColor = entry.statusCode >= 200 && entry.statusCode < 300
                        ? new Color(0.2f, 0.8f, 0.2f)
                        : new Color(0.9f, 0.3f, 0.3f);
                    GUILayout.Label(entry.statusCode.ToString(), statusStyle, GUILayout.Width(30));

                    GUILayout.Label(entry.url, EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(entry.timestamp, EditorStyles.miniLabel, GUILayout.Width(55));

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region UI Helpers

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.LabelField($"── {title} ──", EditorStyles.boldLabel);
        }

        private void DrawEndpointLabel(string method, string path, string description)
        {
            EditorGUILayout.BeginHorizontal();

            var methodStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
            SetMethodColor(methodStyle, method);

            GUILayout.Label(method, methodStyle, GUILayout.Width(45));
            GUILayout.Label(path, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
        }

        private void SetMethodColor(GUIStyle style, string method)
        {
            switch (method)
            {
                case "GET": style.normal.textColor = new Color(0.2f, 0.7f, 0.3f); break;
                case "POST": style.normal.textColor = new Color(0.9f, 0.6f, 0.1f); break;
                case "PUT": style.normal.textColor = new Color(0.3f, 0.5f, 0.9f); break;
                case "PATCH": style.normal.textColor = new Color(0.6f, 0.4f, 0.9f); break;
                case "DELETE": style.normal.textColor = new Color(0.9f, 0.3f, 0.3f); break;
            }
        }

        private void DrawParamField(string label, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(90));
            value = EditorGUILayout.TextField(value);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBodyField(ref string body, float minHeight)
        {
            EditorGUILayout.LabelField("Request Body (JSON):", EditorStyles.miniLabel);
            _bodyScrollPos = EditorGUILayout.BeginScrollView(_bodyScrollPos,
                GUILayout.MinHeight(minHeight), GUILayout.MaxHeight(160));
            var wordWrapStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            body = EditorGUILayout.TextArea(body, wordWrapStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void DrawSimpleEndpoint(string method, string path, string description)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel(method, path, description);
            DrawSendButtonWithCurl($"{method} {path}", () =>
            {
                var m = (HttpMethod)Enum.Parse(typeof(HttpMethod), method);
                SendRequest(m, path);
            }, () => BuildCurl(method, path));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawActionRow(string method, string pathTemplate, string label, string action)
        {
            EditorGUILayout.BeginHorizontal();

            var methodStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
            SetMethodColor(methodStyle, method);
            GUILayout.Label(method, methodStyle, GUILayout.Width(40));
            GUILayout.Label(pathTemplate, EditorStyles.miniLabel, GUILayout.Width(280));

            GUI.enabled = !_isRequestInProgress && !string.IsNullOrEmpty(_chestConfigId);
            if (GUILayout.Button(label, GUILayout.Height(22)))
            {
                SendRequest(HttpMethod.POST, $"/admin/chests/configs/{U(_chestConfigId)}/{action}", "{}");
            }
            if (GUILayout.Button("📋", GUILayout.Width(28), GUILayout.Height(22)))
            {
                string curl = BuildCurl("POST", $"/admin/chests/configs/{U(_chestConfigId)}/{action}", "{}");
                EditorGUIUtility.systemCopyBuffer = curl;
                Debug.Log($"[Admin Console] cURL copied:\n{curl}");
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSendButtonWithCurl(string label, Action onClick, Func<string> buildCurl)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !_isRequestInProgress;
            if (GUILayout.Button(label, GUILayout.Height(28)))
            {
                onClick?.Invoke();
            }
            GUI.enabled = true;
            if (GUILayout.Button("📋 cURL", GUILayout.Width(60), GUILayout.Height(28)))
            {
                string curl = buildCurl?.Invoke() ?? "";
                EditorGUIUtility.systemCopyBuffer = curl;
                Debug.Log($"[Admin Console] cURL copied:\n{curl}");
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Shorthand for Uri.EscapeDataString</summary>
        private static string U(string s) => Uri.EscapeDataString(s ?? "");

        #endregion

        #region HTTP Requests

        private void SendRequest(HttpMethod method, string endpoint, string jsonBody = null)
        {
            if (_isRequestInProgress) return;

            string url = _baseUrl + endpoint;
            UnityWebRequest www;

            switch (method)
            {
                case HttpMethod.GET:
                    www = UnityWebRequest.Get(url);
                    break;
                case HttpMethod.DELETE:
                    www = UnityWebRequest.Delete(url);
                    www.downloadHandler = new DownloadHandlerBuffer();
                    break;
                case HttpMethod.POST:
                case HttpMethod.PUT:
                case HttpMethod.PATCH:
                    www = new UnityWebRequest(url, method.ToString());
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody ?? "{}");
                    www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");
                    break;
                default:
                    return;
            }

            www.timeout = 30;

            if (!string.IsNullOrEmpty(_adminToken))
            {
                www.SetRequestHeader("Authorization", $"Bearer {_adminToken}");
            }

            var sb = new StringBuilder();
            sb.Append($"{method} {url}");
            if (!string.IsNullOrEmpty(jsonBody) && method != HttpMethod.GET && method != HttpMethod.DELETE)
                sb.Append($"\nBody: {jsonBody}");
            _lastRequestInfo = sb.ToString();

            StartRequest(www);
        }

        private void StartRequest(UnityWebRequest www)
        {
            _isRequestInProgress = true;
            _requestStartTime = (float)EditorApplication.timeSinceStartup;
            _activeRequest = www;
            _activeRequest.SendWebRequest();
        }

        private void ProcessResponse(UnityWebRequest www)
        {
            _lastRequestDuration = (float)(EditorApplication.timeSinceStartup - _requestStartTime);
            _lastResponseCode = www.responseCode;
            _lastResponseBody = www.downloadHandler?.text ?? "";
            _lastResponseIsError = www.result != UnityWebRequest.Result.Success;

            // Collect headers
            var headersSb = new StringBuilder();
            var responseHeaders = www.GetResponseHeaders();
            if (responseHeaders != null)
            {
                foreach (var kvp in responseHeaders)
                {
                    headersSb.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
            }
            _lastResponseHeaders = headersSb.ToString();

            // Add to history
            AddToHistory(www.method, www.url, _lastResponseBody, _lastResponseCode);

            // Log
            string logColor = _lastResponseIsError ? "#FF4444" : "#44FF44";
            Debug.Log($"<color={logColor}>[Admin Console] {www.method} {www.url} → {_lastResponseCode} ({_lastRequestDuration:F2}s)</color>");
        }

        private void AbortActiveRequest()
        {
            if (_activeRequest != null)
            {
                _activeRequest.Abort();
                _activeRequest.Dispose();
                _activeRequest = null;
            }
            _isRequestInProgress = false;
        }

        #endregion

        #region History

        private void AddToHistory(string method, string url, string body, long statusCode)
        {
            _history.Add(new HistoryEntry
            {
                method = method,
                url = url,
                body = body.Length > 500 ? body.Substring(0, 500) + "..." : body,
                statusCode = statusCode,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });

            if (_history.Count > MAX_HISTORY)
                _history.RemoveAt(0);

            SaveHistory();
        }

        private void SaveHistory()
        {
            try
            {
                var list = new HistoryList { items = _history };
                string json = JsonUtility.ToJson(list);
                EditorPrefs.SetString(PREFS_HISTORY, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Admin Console] Failed to save history: {ex.Message}");
            }
        }

        private void LoadHistory()
        {
            try
            {
                string json = EditorPrefs.GetString(PREFS_HISTORY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    var list = JsonUtility.FromJson<HistoryList>(json);
                    _history = list?.items ?? new List<HistoryEntry>();
                }
            }
            catch
            {
                _history = new List<HistoryEntry>();
            }
        }

        #endregion

        #region cURL Helpers

        private string BuildCurl(string method, string endpoint, string jsonBody = null)
        {
            string url = _baseUrl + endpoint;
            var sb = new StringBuilder();
            sb.Append($"curl -X {method} '{url}'");

            if (!string.IsNullOrEmpty(_adminToken))
                sb.Append($" \\\n  -H 'Authorization: Bearer {_adminToken}'");

            if (!string.IsNullOrEmpty(jsonBody) && method != "GET" && method != "DELETE")
            {
                sb.Append(" \\\n  -H 'Content-Type: application/json'");
                sb.Append($" \\\n  -d '{jsonBody}'");
            }

            return sb.ToString();
        }

        #endregion

        #region JSON Helpers

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                    .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        /// <summary>
        /// Simple JSON pretty printer (no external dependencies).
        /// </summary>
        private static string PrettyPrintJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            try
            {
                var sb = new StringBuilder();
                int indent = 0;
                bool inString = false;
                bool escaped = false;

                foreach (char c in json)
                {
                    if (escaped)
                    {
                        sb.Append(c);
                        escaped = false;
                        continue;
                    }

                    if (c == '\\' && inString)
                    {
                        sb.Append(c);
                        escaped = true;
                        continue;
                    }

                    if (c == '"')
                    {
                        inString = !inString;
                        sb.Append(c);
                        continue;
                    }

                    if (inString)
                    {
                        sb.Append(c);
                        continue;
                    }

                    switch (c)
                    {
                        case '{':
                        case '[':
                            sb.Append(c);
                            sb.AppendLine();
                            indent++;
                            sb.Append(new string(' ', indent * 2));
                            break;
                        case '}':
                        case ']':
                            sb.AppendLine();
                            indent--;
                            sb.Append(new string(' ', indent * 2));
                            sb.Append(c);
                            break;
                        case ',':
                            sb.Append(c);
                            sb.AppendLine();
                            sb.Append(new string(' ', indent * 2));
                            break;
                        case ':':
                            sb.Append(": ");
                            break;
                        default:
                            if (!char.IsWhiteSpace(c))
                                sb.Append(c);
                            break;
                    }
                }

                return sb.ToString();
            }
            catch
            {
                return json;
            }
        }

        #endregion
    }
}
#endif


