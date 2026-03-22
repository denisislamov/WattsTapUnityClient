using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using WattsTap.Constants;
using WattsTap.Core.Configs;
using WattsTap.Core.Configs.CoreServer;

namespace WattsTap.Core.API
{
    /// <summary>
    /// Service for communicating with the new Core API server (api-dev.wattstap.energy).
    /// Implements authentication with 15-min token + auto-refresh, tap-based progress,
    /// avatar catalog, social/referral, user profile.
    /// </summary>
    public class CoreServerService : ICoreServerService
    {
        private const string Tag = "[CoreServerService]";

        #region Private Fields

        private string _authToken;
        private string _cachedInitData;
        private string _cachedReferralCode;
        private DateTime _tokenExpiresAt = DateTime.MinValue;
        private int _currentEnergy;

        private CoreServerConfig _config;
        private ISharedDataService _sharedDataService;
        private MonoBehaviour _refreshRunner;
        private Coroutine _autoRefreshCoroutine;

        private bool _isRefreshing;

        #endregion

        #region IService

        public int InitializationOrder => 6; // Right after ReferralAPIService (5)
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized) return;

            ServiceLocator.TryGet<ISharedDataService>(out _sharedDataService);

            if (ServiceLocator.TryGet<IConfigService>(out var configService))
            {
                _config = configService.GetConfig<CoreServerConfig>(ConfigsConstants.CoreServerConfig);
            }

            if (_config == null)
            {
                Debug.LogError($"{Tag} CoreServerConfig not found! Using defaults.");
            }

            IsInitialized = true;
            Log("<color=#00FFFF>Initialized</color>");
        }

        public void Shutdown()
        {
            StopAutoRefresh();
            _authToken = null;
            IsInitialized = false;
        }

        #endregion

        #region ICoreServerService — Auth State

        public string AuthToken => _authToken;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);
        public DateTime TokenExpiresAt => _tokenExpiresAt;
        public bool NeedsTokenRefresh => IsAuthenticated &&
            DateTime.UtcNow >= _tokenExpiresAt.AddSeconds(-BufferSeconds);
        public int CurrentEnergy => _currentEnergy;

        #endregion

        #region Auth

        public IEnumerator Authenticate(string initData, string referralCode,
            Action<AuthResponse> onSuccess, Action<string> onError)
        {
            // Cache initData for future re-auth (token refresh fallback)
            _cachedInitData = initData;
            _cachedReferralCode = referralCode;
            _sharedDataService?.SetData(SharedDataConstants.InitData, initData);

            var request = new TelegramAuthRequest
            {
                initData = initData,
                referralCode = referralCode
            };
            string json = JsonUtility.ToJson(request);

            Log($"Authenticating... URL: {BaseUrl + Cfg.AuthTelegram}");

            using (var www = CreatePostRequest(Cfg.AuthTelegram, json, useAuth: false))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var response = TryParse<AuthResponse>(www.downloadHandler.text);
                    if (response != null && !string.IsNullOrEmpty(response.token))
                    {
                        StoreToken(response.token, response.expiresIn);
                        Log($"<color=#00FF00>Auth OK. Player: {response.player?.playerId}, expiresIn: {response.expiresIn}s</color>");
                        onSuccess?.Invoke(response);
                    }
                    else
                    {
                        onError?.Invoke("Empty token in response");
                    }
                }
                else
                {
                    HandleRequestError(www, onError);
                }
            }
        }

        public IEnumerator RefreshToken(Action<AuthResponse> onSuccess, Action<string> onError)
        {
            if (_isRefreshing)
            {
                onError?.Invoke("Refresh already in progress");
                yield break;
            }
            _isRefreshing = true;

            Log("Attempting token refresh via /auth/refresh...");

            // Step 1: Try cookie-based refresh (works in WebGL where browser stores HTTP-only cookies)
            using (var www = CreatePostRequest(Cfg.AuthRefresh, "{}", useAuth: true))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var response = TryParse<AuthResponse>(www.downloadHandler.text);
                    if (response != null && !string.IsNullOrEmpty(response.token))
                    {
                        StoreToken(response.token, response.expiresIn);
                        Log("<color=#00FF00>Token refreshed via /auth/refresh</color>");
                        _isRefreshing = false;
                        onSuccess?.Invoke(response);
                        yield break;
                    }
                }

                Log($"<color=#FFFF00>/auth/refresh failed ({www.responseCode}). Falling back to re-auth...</color>");
            }

            // Step 2: Fallback — re-authenticate with cached initData
            if (!string.IsNullOrEmpty(_cachedInitData))
            {
                bool done = false;
                AuthResponse result = null;
                string error = null;

                yield return Authenticate(_cachedInitData, _cachedReferralCode,
                    s => { result = s; done = true; },
                    e => { error = e; done = true; });

                yield return new WaitUntil(() => done);

                _isRefreshing = false;

                if (result != null)
                {
                    Log("<color=#00FF00>Token refreshed via re-authentication</color>");
                    onSuccess?.Invoke(result);
                }
                else
                {
                    Debug.LogError($"{Tag} Re-auth fallback failed: {error}");
                    onError?.Invoke(error ?? "Re-authentication failed");
                }
            }
            else
            {
                _isRefreshing = false;
                Debug.LogError($"{Tag} No cached initData for re-auth fallback!");
                onError?.Invoke("No cached initData for re-authentication");
            }
        }

        public IEnumerator Logout(Action<LogoutResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedPost<LogoutResponse>(
                Cfg.AuthLogout, "{}", onSuccess, onError);
            _authToken = null;
            _tokenExpiresAt = DateTime.MinValue;
            StopAutoRefresh();
        }

        public void StartAutoRefresh(MonoBehaviour coroutineRunner)
        {
            if (!Cfg.EnableAutoRefresh) return;
            _refreshRunner = coroutineRunner;
            if (_autoRefreshCoroutine != null) return;
            _autoRefreshCoroutine = coroutineRunner.StartCoroutine(AutoRefreshCoroutine());
            Log("Auto-refresh started");
        }

        public void StopAutoRefresh()
        {
            if (_autoRefreshCoroutine != null && _refreshRunner != null)
            {
                _refreshRunner.StopCoroutine(_autoRefreshCoroutine);
                _autoRefreshCoroutine = null;
                Log("Auto-refresh stopped");
            }
        }

        #endregion

        #region User

        public IEnumerator GetUserMe(Action<UserMeResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedGet<UserMeResponse>(Cfg.UserMe, onSuccess, onError);
        }

        public IEnumerator UpdateLanguage(string langCode,
            Action<UpdateLanguageResponse> onSuccess, Action<string> onError)
        {
            var req = new UpdateLanguageRequest { newLangCode = langCode };
            yield return AuthenticatedPost<UpdateLanguageResponse>(
                Cfg.UserLanguage, JsonUtility.ToJson(req), onSuccess, onError);
        }

        public IEnumerator MarkEventsRead(MarkEventsReadRequest request,
            Action<MarkEventsReadResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedPost<MarkEventsReadResponse>(
                Cfg.UserEventsRead, JsonUtility.ToJson(request), onSuccess, onError);
        }

        #endregion

        #region Progress

        public IEnumerator LoadProgress(Action<ExtendedLoadProgressResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedGet(Cfg.Progress,
                (ExtendedLoadProgressResponse resp) =>
                {
                    UpdateEnergyFromState(resp.playerState);
                    onSuccess?.Invoke(resp);
                },
                onError);
        }

        public IEnumerator SendTaps(int tapCount,
            Action<TapProgressResponse> onSuccess, Action<string> onError)
        {
            var req = new TapProgressRequest { tapCount = tapCount };
            yield return AuthenticatedPost(Cfg.Progress, JsonUtility.ToJson(req),
                (TapProgressResponse resp) =>
                {
                    UpdateEnergyFromState(resp.playerState);
                    if (resp.tap?.tapMeta != null)
                    {
                        Log($"Taps: requested={resp.tap.tapMeta.requestedTapCount} applied={resp.tap.tapMeta.appliedTapCount} dropped={resp.tap.tapMeta.droppedTapCount} energy={resp.tap.energyAfter}");
                    }
                    onSuccess?.Invoke(resp);
                },
                onError);
        }

        public IEnumerator ResetProgress(Action<ResetProgressResponse> onSuccess, Action<string> onError)
        {
            var req = new ResetProgressRequest { confirm = true };
            yield return AuthenticatedPost<ResetProgressResponse>(
                Cfg.ProgressReset, JsonUtility.ToJson(req), onSuccess, onError);
        }

        #endregion

        #region Mining Balance

        public IEnumerator LoadMiningBalance(Action<MiningBalancePublicResponse> onSuccess, Action<string> onError)
        {
            // Public endpoint — no auth required
            using (var www = CreateGetRequest(Cfg.BalanceMining, useAuth: false))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var resp = TryParse<MiningBalancePublicResponse>(www.downloadHandler.text);
                    if (resp != null && resp.success && resp.balance != null)
                        onSuccess?.Invoke(resp);
                    else
                        onError?.Invoke("Empty or unsuccessful mining balance response");
                }
                else
                {
                    HandleRequestError(www, onError);
                }
            }
        }

        #endregion

        #region Avatars

        public IEnumerator GetAvatars(Action<GetAvatarsResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedGet<GetAvatarsResponse>(Cfg.Avatars, onSuccess, onError);
        }

        public IEnumerator PurchaseAvatar(PurchaseAvatarRequest request,
            Action<PurchaseAvatarResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedPost<PurchaseAvatarResponse>(
                Cfg.AvatarsPurchase, JsonUtility.ToJson(request), onSuccess, onError);
        }

        public IEnumerator ClaimAvatar(string avatarId,
            Action<ClaimAvatarResponse> onSuccess, Action<string> onError)
        {
            string endpoint = string.Format(Cfg.AvatarsClaimFmt, avatarId);
            yield return AuthenticatedPost<ClaimAvatarResponse>(
                endpoint, "{}", onSuccess, onError);
        }

        #endregion

        #region Social / Referral

        public IEnumerator GetMyReferral(Action<MyReferralResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedGet(Cfg.SocialMyReferral,
                (MyReferralResponse resp) =>
                {
                    _sharedDataService?.SetData(SharedDataConstants.ReferralData, resp);
                    onSuccess?.Invoke(resp);
                },
                onError);
        }

        public IEnumerator GetFriends(Action<FriendsListResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedGet(Cfg.SocialFriends,
                (FriendsListResponse resp) =>
                {
                    _sharedDataService?.SetData(SharedDataConstants.FriendsData, resp);
                    onSuccess?.Invoke(resp);
                },
                onError);
        }

        public IEnumerator ClaimReferralBonus(Action<ClaimBonusResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedPost<ClaimBonusResponse>(
                Cfg.SocialBonusClaim, "{}", onSuccess, onError);
        }

        public IEnumerator ApplyReferralCode(string referralCode, string source,
            Action<ReferralApplyResponse> onSuccess, Action<string> onError)
        {
            var req = new ReferralApplyRequest { referralCode = referralCode, source = source };
            yield return AuthenticatedPost<ReferralApplyResponse>(
                Cfg.ReferralApply, JsonUtility.ToJson(req), onSuccess, onError);
        }

        #endregion

        #region Game Items / Inventory

        public IEnumerator GetCatalog(Action<CatalogResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedGet<CatalogResponse>(Cfg.GameItemsCatalog, onSuccess, onError);
        }

        public IEnumerator GetInventory(Action<InventoryResponse> onSuccess, Action<string> onError)
        {
            yield return AuthenticatedGet<InventoryResponse>(Cfg.GameInventory, onSuccess, onError);
        }

        public IEnumerator EquipItem(string playerItemId,
            Action<EquipItemResponse> onSuccess, Action<string> onError)
        {
            var req = new EquipItemRequest { playerItemId = playerItemId };
            yield return AuthenticatedPost<EquipItemResponse>(
                Cfg.GameInventoryEquip, JsonUtility.ToJson(req), onSuccess, onError);
        }

        public IEnumerator UnequipItem(string slot,
            Action<UnequipItemResponse> onSuccess, Action<string> onError)
        {
            var req = new UnequipItemRequest { slot = slot };
            yield return AuthenticatedPost<UnequipItemResponse>(
                Cfg.GameInventoryUnequip, JsonUtility.ToJson(req), onSuccess, onError);
        }

        public IEnumerator UpgradeItem(string playerItemId,
            Action<UpgradeItemResponse> onSuccess, Action<string> onError)
        {
            var req = new UpgradeItemRequest { playerItemId = playerItemId };
            yield return AuthenticatedPost<UpgradeItemResponse>(
                Cfg.GameInventoryUpgrade, JsonUtility.ToJson(req), onSuccess, onError);
        }

        #endregion

        #region Generic Request Helpers

        /// <summary>
        /// Sends an authenticated GET request. On 401, refreshes token and retries once.
        /// </summary>
        private IEnumerator AuthenticatedGet<T>(string endpoint,
            Action<T> onSuccess, Action<string> onError) where T : class
        {
            if (!EnsureAuth(onError)) yield break;

            // Proactively refresh if about to expire
            if (NeedsTokenRefresh)
            {
                bool refreshDone = false;
                yield return RefreshToken(_ => refreshDone = true, _ => refreshDone = true);
                yield return new WaitUntil(() => refreshDone);
            }

            using (var www = CreateGetRequest(endpoint))
            {
                yield return www.SendWebRequest();

                if (www.responseCode == 401)
                {
                    // Token expired — try refresh and retry
                    yield return RetryAfterRefresh<T>(
                        () => CreateGetRequest(endpoint), onSuccess, onError);
                    yield break;
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var resp = TryParse<T>(www.downloadHandler.text);
                    if (resp != null)
                        onSuccess?.Invoke(resp);
                    else
                        onError?.Invoke("Failed to parse response");
                }
                else
                {
                    HandleRequestError(www, onError);
                }
            }
        }

        /// <summary>
        /// Sends an authenticated POST request. On 401, refreshes token and retries once.
        /// </summary>
        private IEnumerator AuthenticatedPost<T>(string endpoint, string jsonBody,
            Action<T> onSuccess, Action<string> onError) where T : class
        {
            if (!EnsureAuth(onError)) yield break;

            if (NeedsTokenRefresh)
            {
                bool refreshDone = false;
                yield return RefreshToken(_ => refreshDone = true, _ => refreshDone = true);
                yield return new WaitUntil(() => refreshDone);
            }

            using (var www = CreatePostRequest(endpoint, jsonBody))
            {
                yield return www.SendWebRequest();

                if (www.responseCode == 401)
                {
                    yield return RetryAfterRefresh<T>(
                        () => CreatePostRequest(endpoint, jsonBody), onSuccess, onError);
                    yield break;
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var resp = TryParse<T>(www.downloadHandler.text);
                    if (resp != null)
                        onSuccess?.Invoke(resp);
                    else
                        onError?.Invoke("Failed to parse response");
                }
                else
                {
                    HandleRequestError(www, onError);
                }
            }
        }

        /// <summary>Refresh token and retry the request once.</summary>
        private IEnumerator RetryAfterRefresh<T>(Func<UnityWebRequest> requestFactory,
            Action<T> onSuccess, Action<string> onError) where T : class
        {
            Log("<color=#FFFF00>Got 401 — refreshing token and retrying...</color>");

            bool refreshDone = false;
            bool refreshOk = false;
            yield return RefreshToken(
                _ => { refreshOk = true; refreshDone = true; },
                _ => { refreshDone = true; });
            yield return new WaitUntil(() => refreshDone);

            if (!refreshOk)
            {
                onError?.Invoke("Authentication expired and refresh failed");
                yield break;
            }

            using (var www = requestFactory())
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var resp = TryParse<T>(www.downloadHandler.text);
                    if (resp != null)
                        onSuccess?.Invoke(resp);
                    else
                        onError?.Invoke("Failed to parse response after retry");
                }
                else
                {
                    HandleRequestError(www, onError);
                }
            }
        }

        #endregion

        #region HTTP Helpers

        private string BaseUrl => _config?.BaseUrl ?? "https://api-dev.wattstap.energy";
        private int RequestTimeout => _config?.RequestTimeout ?? 30;
        private int BufferSeconds => _config?.TokenRefreshBufferSeconds ?? 60;
        private CoreServerConfig Cfg => _config;

        private UnityWebRequest CreateGetRequest(string endpoint, bool useAuth = true)
        {
            var www = UnityWebRequest.Get(BaseUrl + endpoint);
            www.timeout = RequestTimeout;
            if (useAuth && IsAuthenticated)
                www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            return www;
        }

        private UnityWebRequest CreatePostRequest(string endpoint, string jsonBody, bool useAuth = true)
        {
            var www = new UnityWebRequest(BaseUrl + endpoint, "POST");
            www.timeout = RequestTimeout;
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            if (useAuth && IsAuthenticated)
                www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            return www;
        }

        private bool EnsureAuth(Action<string> onError)
        {
            if (!IsAuthenticated)
            {
                onError?.Invoke("Not authenticated");
                return false;
            }
            return true;
        }

        private void HandleRequestError(UnityWebRequest www, Action<string> onError)
        {
            string errorMessage = www.error;
            if (!string.IsNullOrEmpty(www.downloadHandler?.text))
            {
                try
                {
                    var err = JsonUtility.FromJson<ApiErrorResponse>(www.downloadHandler.text);
                    if (!string.IsNullOrEmpty(err?.detail))
                        errorMessage = err.detail;
                }
                catch { /* use default */ }
            }
            Debug.LogError($"{Tag} Request failed [{www.responseCode}]: {errorMessage}");
            onError?.Invoke(errorMessage);
        }

        private T TryParse<T>(string json) where T : class
        {
            try
            {
                Log($"Response: {(json.Length > 500 ? json.Substring(0, 500) + "..." : json)}");
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Tag} JSON parse error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Token Management

        private void StoreToken(string token, int expiresInSeconds)
        {
            _authToken = token;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);

            _sharedDataService?.SetData(SharedDataConstants.CoreAuthToken, token);
            _sharedDataService?.SetData(SharedDataConstants.CoreTokenExpiresAt, _tokenExpiresAt);

            // Also store in legacy key so other services can read it
            _sharedDataService?.SetData(SharedDataConstants.ReferralAuthToken, token);
        }

        private void UpdateEnergyFromState(PlayerStateDTO playerState)
        {
            if (playerState == null) return;
            _currentEnergy = playerState.energy;
            _sharedDataService?.SetData(SharedDataConstants.PlayerEnergy, _currentEnergy);
            _sharedDataService?.SetData(SharedDataConstants.PlayerState, playerState);
        }

        private IEnumerator AutoRefreshCoroutine()
        {
            float checkInterval = _config?.RefreshCheckInterval ?? 30f;
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);

                if (!IsAuthenticated) continue;

                if (NeedsTokenRefresh)
                {
                    Log("Auto-refresh: token is about to expire, refreshing...");
                    bool done = false;
                    yield return RefreshToken(
                        _ => done = true,
                        err =>
                        {
                            Debug.LogError($"{Tag} Auto-refresh failed: {err}");
                            done = true;
                        });
                    yield return new WaitUntil(() => done);
                }
            }
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (_config == null || _config.DebugLogging)
                Debug.Log($"<color=#00FFCC>{Tag} {message}</color>");
        }

        #endregion
    }
}

