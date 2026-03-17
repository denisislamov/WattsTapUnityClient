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
    /// Unity Editor window for testing WattsTap REST API endpoints.
    /// Allows sending real HTTP requests to the dev server with a convenient UI.
    /// Menu: WattsTap → API Tester
    /// </summary>
    public class WattsTapAPITesterWindow : EditorWindow
    {
        #region Constants

        private const string DEFAULT_BASE_URL = "https://api-dev.wattstap.energy";
        private const string PREFS_BASE_URL = "WattsTapAPITester_BaseUrl";
        private const string PREFS_AUTH_TOKEN = "WattsTapAPITester_AuthToken";
        private const string PREFS_INIT_DATA = "WattsTapAPITester_InitData";

        #endregion

        #region Enums

        private enum ApiCategory
        {
            Health,
            Auth,
            Progress,
            Social,
            Avatars,
            Items,
            Inventory,
            Wallet,
            User
        }

        #endregion

        #region Fields

        // Settings
        private string _baseUrl;
        private string _authToken;
        private string _initData;

        // UI state
        private Vector2 _mainScrollPos;
        private Vector2 _responseScrollPos;
        private Vector2 _initDataScrollPos;
        private Vector2 _parsedInitDataScrollPos;
        private ApiCategory _selectedCategory = ApiCategory.Health;
        private bool _showSettings = true;
        private bool _showParsedInitData;
        private bool _isRequestInProgress;
        private float _requestStartTime;
        private string _cachedInitDataForParse = "";
        private List<KeyValuePair<string, string>> _parsedInitDataFields = new List<KeyValuePair<string, string>>();

        // Response
        private string _lastResponseBody = "";
        private long _lastResponseCode;
        private string _lastResponseHeaders = "";
        private string _lastRequestInfo = "";
        private float _lastRequestDuration;
        private bool _lastResponseIsError;

        // Input fields per endpoint
        private string _authReferralCode = "";
        private int _tapCount = 5;
        private bool _resetConfirm = true;
        private string _avatarId = "";
        private string _avatarCurrency = "watts";
        private string _avatarClaimId = "";
        private string _orderId = "";
        private string _equipPlayerItemId = "";
        private string _unequipSlot = "WEAPON";
        private string _upgradePlayerItemId = "";
        private string _walletAddress = "";
        private string _walletNetwork = "";
        private string _walletPublicKey = "";
        private string _walletProof = "{}";
        private string _userLanguage = "en";
        private bool _eventsReadAll = true;
        private string _eventIds = "";
        private string _referralApplyCode = "";
        private string _referralApplySource = "miniapp";

        // Foldouts
        private bool _showResponseHeaders;

        // Active request
        private UnityWebRequest _activeRequest;

        #endregion

        #region Menu Item

        [MenuItem("WattsTap/API Tester")]
        public static void ShowWindow()
        {
            var window = GetWindow<WattsTapAPITesterWindow>("API Tester");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            _baseUrl = EditorPrefs.GetString(PREFS_BASE_URL, DEFAULT_BASE_URL);
            _authToken = EditorPrefs.GetString(PREFS_AUTH_TOKEN, "");
            _initData = EditorPrefs.GetString(PREFS_INIT_DATA, "");
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(PREFS_BASE_URL, _baseUrl);
            EditorPrefs.SetString(PREFS_AUTH_TOKEN, _authToken);
            EditorPrefs.SetString(PREFS_INIT_DATA, _initData);
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

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Toolbar

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("⚡ WattsTap API Tester", EditorStyles.boldLabel, GUILayout.Width(180));
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
                var authStatusStyle = new GUIStyle(EditorStyles.miniLabel);
                if (!string.IsNullOrEmpty(_authToken))
                {
                    authStatusStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
                    GUILayout.Label("🔑 Authenticated", authStatusStyle);
                }
                else
                {
                    authStatusStyle.normal.textColor = new Color(0.8f, 0.4f, 0.2f);
                    GUILayout.Label("🔒 No Token", authStatusStyle);
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
                EditorGUILayout.LabelField("Base URL", GUILayout.Width(70));
                _baseUrl = EditorGUILayout.TextField(_baseUrl);
                if (GUILayout.Button("Reset", GUILayout.Width(50)))
                    _baseUrl = DEFAULT_BASE_URL;
                EditorGUILayout.EndHorizontal();

                // Auth Token
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("JWT Token", GUILayout.Width(70));
                _authToken = EditorGUILayout.TextField(_authToken);
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                    _authToken = "";
                EditorGUILayout.EndHorizontal();

                // Init Data
                EditorGUILayout.LabelField("Telegram initData (for /auth/telegram):");
                _initDataScrollPos = EditorGUILayout.BeginScrollView(_initDataScrollPos,
                    GUILayout.MinHeight(40), GUILayout.MaxHeight(80));
                var wordWrapStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                _initData = EditorGUILayout.TextArea(_initData, wordWrapStyle, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                // Parsed initData
                if (!string.IsNullOrEmpty(_initData))
                {
                    // Re-parse only when content changed
                    if (_cachedInitDataForParse != _initData)
                    {
                        _cachedInitDataForParse = _initData;
                        _parsedInitDataFields = ParseInitData(_initData);
                    }

                    _showParsedInitData = EditorGUILayout.Foldout(_showParsedInitData, $"📖 Parsed initData ({_parsedInitDataFields.Count} fields)");
                    if (_showParsedInitData && _parsedInitDataFields.Count > 0)
                    {
                        _parsedInitDataScrollPos = EditorGUILayout.BeginScrollView(_parsedInitDataScrollPos,
                            GUILayout.MinHeight(60), GUILayout.MaxHeight(200));
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        foreach (var kvp in _parsedInitDataFields)
                        {
                            EditorGUILayout.BeginHorizontal();

                            // Key label
                            var keyStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
                            EditorGUILayout.LabelField(kvp.Key, keyStyle, GUILayout.Width(100));

                            // Value — if it looks like JSON, show pretty-printed; otherwise plain
                            string val = kvp.Value;
                            if (val.StartsWith("{") || val.StartsWith("["))
                            {
                                string pretty = PrettyPrintJson(val);
                                EditorGUILayout.SelectableLabel(pretty,
                                    EditorStyles.wordWrappedMiniLabel,
                                    GUILayout.MinHeight(EditorStyles.wordWrappedMiniLabel.CalcHeight(
                                        new GUIContent(pretty), position.width - 160)));
                            }
                            else
                            {
                                EditorGUILayout.SelectableLabel(val, EditorStyles.wordWrappedMiniLabel,
                                    GUILayout.Height(16));
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndScrollView();
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Category Tabs

        private void DrawCategoryTabs()
        {
            EditorGUILayout.BeginHorizontal();

            var categories = (ApiCategory[])Enum.GetValues(typeof(ApiCategory));
            foreach (var cat in categories)
            {
                bool isSelected = _selectedCategory == cat;
                var style = isSelected ? GetSelectedTabStyle() : EditorStyles.miniButton;

                if (GUILayout.Button(cat.ToString(), style, GUILayout.Height(24)))
                {
                    _selectedCategory = cat;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private GUIStyle GetSelectedTabStyle()
        {
            var style = new GUIStyle(EditorStyles.miniButton);
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(0.2f, 0.5f, 0.9f));
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
                case ApiCategory.Health:
                    DrawHealthEndpoints();
                    break;
                case ApiCategory.Auth:
                    DrawAuthEndpoints();
                    break;
                case ApiCategory.Progress:
                    DrawProgressEndpoints();
                    break;
                case ApiCategory.Social:
                    DrawSocialEndpoints();
                    break;
                case ApiCategory.Avatars:
                    DrawAvatarsEndpoints();
                    break;
                case ApiCategory.Items:
                    DrawItemsEndpoints();
                    break;
                case ApiCategory.Inventory:
                    DrawInventoryEndpoints();
                    break;
                case ApiCategory.Wallet:
                    DrawWalletEndpoints();
                    break;
                case ApiCategory.User:
                    DrawUserEndpoints();
                    break;
            }
        }

        // ─── Health ───────────────────────────────────────────────
        private void DrawHealthEndpoints()
        {
            DrawSectionHeader("Health Check");

            DrawEndpointButton("GET", "/health", "Health check — verify server is running", () =>
            {
                SendGetRequest("/health", false);
            }, useAuth: false);
        }

        // ─── Auth ─────────────────────────────────────────────────
        private void DrawAuthEndpoints()
        {
            DrawSectionHeader("Authentication");

            // POST /auth/telegram
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/auth/telegram", "Authenticate via Telegram initData");

            EditorGUILayout.LabelField("initData (set in Settings above):", EditorStyles.miniLabel);
            if (string.IsNullOrEmpty(_initData))
            {
                EditorGUILayout.HelpBox("⚠ initData is empty. Paste it in Settings section.", MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Referral Code", GUILayout.Width(90));
            _authReferralCode = EditorGUILayout.TextField(_authReferralCode);
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Authenticate", () =>
            {
                var body = new Dictionary<string, object> { { "initData", _initData } };
                if (!string.IsNullOrEmpty(_authReferralCode))
                    body["referralCode"] = _authReferralCode;
                SendPostRequest("/auth/telegram", DictToJson(body), false, autoExtractToken: true);
            }, () =>
            {
                var body = new Dictionary<string, object> { { "initData", _initData } };
                if (!string.IsNullOrEmpty(_authReferralCode))
                    body["referralCode"] = _authReferralCode;
                return BuildCurlPost("/auth/telegram", DictToJson(body), false);
            });
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /auth/refresh
            DrawEndpointButton("POST", "/auth/refresh", "Refresh access token (uses cookie)", () =>
            {
                SendPostRequest("/auth/refresh", "{}", true);
            });

            // POST /auth/logout
            DrawEndpointButton("POST", "/auth/logout", "Logout (clears refresh token)", () =>
            {
                SendPostRequest("/auth/logout", "{}", false);
            });
        }

        // ─── Progress ─────────────────────────────────────────────
        private void DrawProgressEndpoints()
        {
            DrawSectionHeader("Progress");

            // GET /progress
            DrawEndpointButton("GET", "/progress", "Get user progress with detailed state", () =>
            {
                SendGetRequest("/progress", true);
            });

            EditorGUILayout.Space(4);

            // POST /progress (taps)
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/progress", "Apply taps (server-side)");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tap Count", GUILayout.Width(90));
            _tapCount = EditorGUILayout.IntSlider(_tapCount, 1, 50);
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Send Taps", () =>
            {
                var body = $"{{\"tapCount\":{_tapCount}}}";
                SendPostRequest("/progress", body, true);
            }, () => BuildCurlPost("/progress", $"{{\"tapCount\":{_tapCount}}}", true));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /progress/reset
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/progress/reset", "Reset progress (dev only)");
            _resetConfirm = EditorGUILayout.Toggle("Confirm Reset", _resetConfirm);

            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
            DrawSendButtonWithCurl("⚠ Reset Progress", () =>
            {
                if (EditorUtility.DisplayDialog("Reset Progress",
                    "Are you sure you want to reset all progress?", "Yes, Reset", "Cancel"))
                {
                    var body = $"{{\"confirm\":{(_resetConfirm ? "true" : "false")}}}";
                    SendPostRequest("/progress/reset", body, true);
                }
            }, () => BuildCurlPost("/progress/reset", $"{{\"confirm\":{(_resetConfirm ? "true" : "false")}}}", true));
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // GET /balance/mining
            DrawEndpointButton("GET", "/balance/mining", "Get mining/balance config (public)", () =>
            {
                SendGetRequest("/balance/mining", false);
            }, useAuth: false);
        }

        // ─── Social ──────────────────────────────────────────────
        private void DrawSocialEndpoints()
        {
            DrawSectionHeader("Social & Referrals");

            // GET /social/my-referral
            DrawEndpointButton("GET", "/social/my-referral", "Get referral info and totals", () =>
            {
                SendGetRequest("/social/my-referral", true);
            });

            // GET /social/friends
            DrawEndpointButton("GET", "/social/friends", "Get referral friends list", () =>
            {
                SendGetRequest("/social/friends", true);
            });

            EditorGUILayout.Space(4);

            // POST /social/bonus/claim
            DrawEndpointButton("POST", "/social/bonus/claim", "Claim all pending referral bonuses", () =>
            {
                SendPostRequest("/social/bonus/claim", "{}", true);
            });

            EditorGUILayout.Space(4);

            // POST /referral/apply
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/referral/apply", "Apply referral code manually");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Referral Code", GUILayout.Width(90));
            _referralApplyCode = EditorGUILayout.TextField(_referralApplyCode);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source", GUILayout.Width(90));
            int sourceIdx = Array.IndexOf(new[] { "web", "miniapp", "system" }, _referralApplySource);
            if (sourceIdx < 0) sourceIdx = 1;
            sourceIdx = EditorGUILayout.Popup(sourceIdx, new[] { "web", "miniapp", "system" });
            _referralApplySource = new[] { "web", "miniapp", "system" }[sourceIdx];
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Apply Referral", () =>
            {
                var body = new Dictionary<string, object> { { "source", _referralApplySource } };
                if (!string.IsNullOrEmpty(_referralApplyCode))
                    body["referralCode"] = _referralApplyCode;
                SendPostRequest("/referral/apply", DictToJson(body), true);
            }, () =>
            {
                var body = new Dictionary<string, object> { { "source", _referralApplySource } };
                if (!string.IsNullOrEmpty(_referralApplyCode))
                    body["referralCode"] = _referralApplyCode;
                return BuildCurlPost("/referral/apply", DictToJson(body), true);
            });
            EditorGUILayout.EndVertical();
        }

        // ─── Avatars ─────────────────────────────────────────────
        private void DrawAvatarsEndpoints()
        {
            DrawSectionHeader("Avatars");

            // GET /avatars
            DrawEndpointButton("GET", "/avatars", "Get avatar catalog for current user", () =>
            {
                SendGetRequest("/avatars", true);
            });

            EditorGUILayout.Space(4);

            // POST /avatars/purchase
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/avatars/purchase", "Purchase avatar with watts or btn");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Avatar ID", GUILayout.Width(90));
            _avatarId = EditorGUILayout.TextField(_avatarId);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Currency", GUILayout.Width(90));
            int curIdx = _avatarCurrency == "btn" ? 1 : 0;
            curIdx = EditorGUILayout.Popup(curIdx, new[] { "watts", "btn" });
            _avatarCurrency = curIdx == 0 ? "watts" : "btn";
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Purchase Avatar", () =>
            {
                var body = $"{{\"avatarId\":\"{EscapeJson(_avatarId)}\",\"currency\":\"{_avatarCurrency}\"}}";
                SendPostRequest("/avatars/purchase", body, true);
            }, () => BuildCurlPost("/avatars/purchase",
                $"{{\"avatarId\":\"{EscapeJson(_avatarId)}\",\"currency\":\"{_avatarCurrency}\"}}", true));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /avatars/{id}/claim
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/avatars/{id}/claim", "Claim free avatar");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Avatar ID", GUILayout.Width(90));
            _avatarClaimId = EditorGUILayout.TextField(_avatarClaimId);
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Claim Avatar", () =>
            {
                SendPostRequest($"/avatars/{Uri.EscapeDataString(_avatarClaimId)}/claim", "{}", true);
            }, () => BuildCurlPost($"/avatars/{Uri.EscapeDataString(_avatarClaimId)}/claim", "{}", true));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // GET /orders/{id}
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("GET", "/orders/{id}", "Get order by id");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Order ID", GUILayout.Width(90));
            _orderId = EditorGUILayout.TextField(_orderId);
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Get Order", () =>
            {
                SendGetRequest($"/orders/{Uri.EscapeDataString(_orderId)}", true);
            }, () => BuildCurlGet($"/orders/{Uri.EscapeDataString(_orderId)}", true));
            EditorGUILayout.EndVertical();
        }

        // ─── Items ────────────────────────────────────────────────
        private void DrawItemsEndpoints()
        {
            DrawSectionHeader("Items Catalog");

            // GET /game/items/catalog
            DrawEndpointButton("GET", "/game/items/catalog", "Get items catalog", () =>
            {
                SendGetRequest("/game/items/catalog", true);
            });
        }

        // ─── Inventory ────────────────────────────────────────────
        private void DrawInventoryEndpoints()
        {
            DrawSectionHeader("Inventory");

            // GET /game/inventory
            DrawEndpointButton("GET", "/game/inventory", "Get player inventory", () =>
            {
                SendGetRequest("/game/inventory", true);
            });

            EditorGUILayout.Space(4);

            // POST /game/inventory/equip
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/game/inventory/equip", "Equip inventory item");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Item ID", GUILayout.Width(100));
            _equipPlayerItemId = EditorGUILayout.TextField(_equipPlayerItemId);
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Equip Item", () =>
            {
                var body = $"{{\"playerItemId\":\"{EscapeJson(_equipPlayerItemId)}\"}}";
                SendPostRequest("/game/inventory/equip", body, true);
            }, () => BuildCurlPost("/game/inventory/equip",
                $"{{\"playerItemId\":\"{EscapeJson(_equipPlayerItemId)}\"}}", true));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /game/inventory/unequip
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/game/inventory/unequip", "Unequip slot");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Slot", GUILayout.Width(100));
            string[] slots = { "WEAPON", "ARMS", "BODY", "FEET" };
            int slotIdx = Array.IndexOf(slots, _unequipSlot);
            if (slotIdx < 0) slotIdx = 0;
            slotIdx = EditorGUILayout.Popup(slotIdx, slots);
            _unequipSlot = slots[slotIdx];
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Unequip Slot", () =>
            {
                var body = $"{{\"slot\":\"{_unequipSlot}\"}}";
                SendPostRequest("/game/inventory/unequip", body, true);
            }, () => BuildCurlPost("/game/inventory/unequip", $"{{\"slot\":\"{_unequipSlot}\"}}", true));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /game/inventory/upgrade
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/game/inventory/upgrade", "Upgrade inventory item");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Item ID", GUILayout.Width(100));
            _upgradePlayerItemId = EditorGUILayout.TextField(_upgradePlayerItemId);
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Upgrade Item", () =>
            {
                var body = $"{{\"playerItemId\":\"{EscapeJson(_upgradePlayerItemId)}\"}}";
                SendPostRequest("/game/inventory/upgrade", body, true);
            }, () => BuildCurlPost("/game/inventory/upgrade",
                $"{{\"playerItemId\":\"{EscapeJson(_upgradePlayerItemId)}\"}}", true));
            EditorGUILayout.EndVertical();
        }

        // ─── Wallet ──────────────────────────────────────────────
        private void DrawWalletEndpoints()
        {
            DrawSectionHeader("Wallet");

            // GET /wallet/my-wallet
            DrawEndpointButton("GET", "/wallet/my-wallet", "Get current wallet(s)", () =>
            {
                SendGetRequest("/wallet/my-wallet", true);
            });

            // GET /wallet/payload
            DrawEndpointButton("GET", "/wallet/payload", "Generate wallet payload", () =>
            {
                SendGetRequest("/wallet/payload", true);
            });

            EditorGUILayout.Space(4);

            // POST /wallet/validate
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/wallet/validate", "Validate wallet proof");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Address", GUILayout.Width(80));
            _walletAddress = EditorGUILayout.TextField(_walletAddress);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Network", GUILayout.Width(80));
            _walletNetwork = EditorGUILayout.TextField(_walletNetwork);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Public Key", GUILayout.Width(80));
            _walletPublicKey = EditorGUILayout.TextField(_walletPublicKey);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Proof (JSON):", EditorStyles.miniLabel);
            _walletProof = EditorGUILayout.TextArea(_walletProof, GUILayout.MinHeight(40));

            DrawSendButtonWithCurl("Validate Wallet", () =>
            {
                var body = $"{{\"address\":\"{EscapeJson(_walletAddress)}\",\"network\":\"{EscapeJson(_walletNetwork)}\",\"public_key\":\"{EscapeJson(_walletPublicKey)}\",\"proof\":{_walletProof}}}";
                SendPostRequest("/wallet/validate", body, true);
            }, () => BuildCurlPost("/wallet/validate",
                $"{{\"address\":\"{EscapeJson(_walletAddress)}\",\"network\":\"{EscapeJson(_walletNetwork)}\",\"public_key\":\"{EscapeJson(_walletPublicKey)}\",\"proof\":{_walletProof}}}", true));
            EditorGUILayout.EndVertical();
        }

        // ─── User ────────────────────────────────────────────────
        private void DrawUserEndpoints()
        {
            DrawSectionHeader("User");

            // GET /user/me
            DrawEndpointButton("GET", "/user/me", "Get current user profile and events", () =>
            {
                SendGetRequest("/user/me", true);
            });

            EditorGUILayout.Space(4);

            // POST /user/language
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/user/language", "Update user language");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language", GUILayout.Width(80));
            _userLanguage = EditorGUILayout.TextField(_userLanguage);
            EditorGUILayout.EndHorizontal();

            DrawSendButtonWithCurl("Update Language", () =>
            {
                var body = $"{{\"newLangCode\":\"{EscapeJson(_userLanguage)}\"}}";
                SendPostRequest("/user/language", body, true);
            }, () => BuildCurlPost("/user/language",
                $"{{\"newLangCode\":\"{EscapeJson(_userLanguage)}\"}}", true));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // POST /user/events/read
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel("POST", "/user/events/read", "Mark user events as read");

            _eventsReadAll = EditorGUILayout.Toggle("Read All", _eventsReadAll);

            if (!_eventsReadAll)
            {
                EditorGUILayout.LabelField("Event IDs (comma separated):", EditorStyles.miniLabel);
                _eventIds = EditorGUILayout.TextField(_eventIds);
            }

            DrawSendButtonWithCurl("Mark Events Read", () =>
            {
                string body = BuildEventsReadBody();
                SendPostRequest("/user/events/read", body, true);
            }, () => BuildCurlPost("/user/events/read", BuildEventsReadBody(), true));
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
                    Debug.Log("[API Tester] Response copied to clipboard.");
                }

                if (GUILayout.Button("Copy Pretty", EditorStyles.miniButton, GUILayout.Width(90)))
                {
                    EditorGUIUtility.systemCopyBuffer = PrettyPrintJson(_lastResponseBody);
                    Debug.Log("[API Tester] Pretty response copied to clipboard.");
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
                EditorGUILayout.LabelField("No requests sent yet. Select a category and click a button above.",
                    EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
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

            var methodStyle = new GUIStyle(EditorStyles.miniLabel);
            methodStyle.fontStyle = FontStyle.Bold;

            if (method == "GET")
                methodStyle.normal.textColor = new Color(0.2f, 0.7f, 0.3f);
            else if (method == "POST")
                methodStyle.normal.textColor = new Color(0.9f, 0.6f, 0.1f);
            else if (method == "PATCH")
                methodStyle.normal.textColor = new Color(0.3f, 0.5f, 0.9f);
            else if (method == "DELETE")
                methodStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);

            GUILayout.Label(method, methodStyle, GUILayout.Width(40));
            GUILayout.Label(path, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawEndpointButton(string method, string path, string description, Action onClick,
            bool useAuth = true, string postBody = "{}")
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEndpointLabel(method, path, description);
            DrawSendButtonWithCurl($"{method} {path}", onClick,
                () => method == "GET" ? BuildCurlGet(path, useAuth) : BuildCurlPost(path, postBody, useAuth));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
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
                Debug.Log($"[API Tester] cURL copied:\n{curl}");
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region HTTP Requests

        private void SendGetRequest(string endpoint, bool useAuth)
        {
            if (_isRequestInProgress) return;

            string url = _baseUrl + endpoint;
            var www = UnityWebRequest.Get(url);
            www.timeout = 30;

            if (useAuth && !string.IsNullOrEmpty(_authToken))
            {
                www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            }

            _lastRequestInfo = $"GET {url}";
            StartRequest(www);
        }

        private void SendPostRequest(string endpoint, string jsonBody, bool useAuth, bool autoExtractToken = false)
        {
            if (_isRequestInProgress) return;

            string url = _baseUrl + endpoint;
            var www = new UnityWebRequest(url, "POST");
            www.timeout = 30;

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            if (useAuth && !string.IsNullOrEmpty(_authToken))
            {
                www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            }

            _lastRequestInfo = $"POST {url}\nBody: {jsonBody}";

            if (autoExtractToken)
            {
                // Mark that we should extract token from the response
                www.SetRequestHeader("X-AutoExtractToken", "true");
            }

            StartRequest(www, autoExtractToken);
        }

        private bool _autoExtractTokenPending;

        private void StartRequest(UnityWebRequest www, bool autoExtractToken = false)
        {
            _isRequestInProgress = true;
            _requestStartTime = (float)EditorApplication.timeSinceStartup;
            _autoExtractTokenPending = autoExtractToken;
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

            // Auto-extract token from auth response
            if (_autoExtractTokenPending && !_lastResponseIsError)
            {
                TryExtractToken(_lastResponseBody);
            }
            _autoExtractTokenPending = false;

            // Log
            string logColor = _lastResponseIsError ? "#FF4444" : "#44FF44";
            Debug.Log($"<color={logColor}>[API Tester] {_lastRequestInfo} → {_lastResponseCode} ({_lastRequestDuration:F2}s)</color>");
        }

        private void TryExtractToken(string responseBody)
        {
            try
            {
                // Simple JSON parsing for "token" field without external dependencies
                // Looking for: "token":"<value>"
                int tokenIdx = responseBody.IndexOf("\"token\"", StringComparison.Ordinal);
                if (tokenIdx < 0) return;

                int colonIdx = responseBody.IndexOf(':', tokenIdx + 7);
                if (colonIdx < 0) return;

                int quoteStart = responseBody.IndexOf('"', colonIdx + 1);
                if (quoteStart < 0) return;

                int quoteEnd = responseBody.IndexOf('"', quoteStart + 1);
                if (quoteEnd < 0) return;

                string token = responseBody.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                if (!string.IsNullOrEmpty(token) && token.Length > 10)
                {
                    _authToken = token;
                    EditorPrefs.SetString(PREFS_AUTH_TOKEN, _authToken);
                    Debug.Log($"<color=#00FFFF>[API Tester] ✅ JWT token auto-extracted and saved! (length: {token.Length})</color>");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[API Tester] Failed to auto-extract token: {ex.Message}");
            }
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
            _autoExtractTokenPending = false;
        }

        #endregion

        #region cURL Helpers

        private string BuildEventsReadBody()
        {
            if (_eventsReadAll)
                return "{\"readAll\":true}";

            var ids = _eventIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder("{\"readAll\":false,\"eventIds\":[");
            for (int i = 0; i < ids.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append($"\"{EscapeJson(ids[i].Trim())}\"");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private string BuildCurlGet(string endpoint, bool useAuth)
        {
            string url = _baseUrl + endpoint;
            var sb = new StringBuilder();
            sb.Append($"curl -X GET '{url}'");
            if (useAuth && !string.IsNullOrEmpty(_authToken))
                sb.Append($" \\\n  -H 'Authorization: Bearer {_authToken}'");
            return sb.ToString();
        }

        private string BuildCurlPost(string endpoint, string jsonBody, bool useAuth)
        {
            string url = _baseUrl + endpoint;
            var sb = new StringBuilder();
            sb.Append($"curl -X POST '{url}'");
            sb.Append(" \\\n  -H 'Content-Type: application/json'");
            if (useAuth && !string.IsNullOrEmpty(_authToken))
                sb.Append($" \\\n  -H 'Authorization: Bearer {_authToken}'");
            if (!string.IsNullOrEmpty(jsonBody))
                sb.Append($" \\\n  -d '{jsonBody}'");
            return sb.ToString();
        }

        #endregion

        #region JSON Helpers

        /// <summary>
        /// Parses Telegram initData (URL-encoded query string) into readable key-value pairs.
        /// </summary>
        private static List<KeyValuePair<string, string>> ParseInitData(string raw)
        {
            var result = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(raw)) return result;

            try
            {
                string data = raw.Trim();
                string[] pairs = data.Split('&');

                foreach (string pair in pairs)
                {
                    if (string.IsNullOrEmpty(pair)) continue;

                    int eqIdx = pair.IndexOf('=');
                    if (eqIdx < 0)
                    {
                        result.Add(new KeyValuePair<string, string>(Uri.UnescapeDataString(pair), ""));
                        continue;
                    }

                    string key = Uri.UnescapeDataString(pair.Substring(0, eqIdx));
                    string value = Uri.UnescapeDataString(pair.Substring(eqIdx + 1));
                    result.Add(new KeyValuePair<string, string>(key, value));
                }
            }
            catch (Exception ex)
            {
                result.Clear();
                result.Add(new KeyValuePair<string, string>("⚠ Parse error", ex.Message));
            }

            return result;
        }

        private static string DictToJson(Dictionary<string, object> dict)
        {
            var sb = new StringBuilder("{");
            bool first = true;
            foreach (var kvp in dict)
            {
                if (!first) sb.Append(",");
                first = false;

                sb.Append($"\"{EscapeJson(kvp.Key)}\":");

                if (kvp.Value is string strVal)
                    sb.Append($"\"{EscapeJson(strVal)}\"");
                else if (kvp.Value is bool boolVal)
                    sb.Append(boolVal ? "true" : "false");
                else if (kvp.Value is int intVal)
                    sb.Append(intVal);
                else if (kvp.Value is long longVal)
                    sb.Append(longVal);
                else
                    sb.Append($"\"{EscapeJson(kvp.Value?.ToString() ?? "")}\"");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
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

