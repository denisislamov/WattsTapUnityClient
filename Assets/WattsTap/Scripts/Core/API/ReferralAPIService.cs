using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using WattsTap.Constants;
using WattsTap.Core.Configs;
using WattsTap.Core.Configs.Referral;

namespace WattsTap.Core.API
{
    /// <summary>
    /// Interface for referral API operations
    /// </summary>
    public interface IReferralAPIService : IService
    {
        /// <summary>JWT token for authentication</summary>
        string AuthToken { get; }
        
        /// <summary>Whether user is authenticated</summary>
        bool IsAuthenticated { get; }
        
        /// <summary>Authenticate via Telegram initData</summary>
        IEnumerator Authenticate(string initData, string referralCode, Action<AuthResponse> onSuccess, Action<string> onError);
        
        /// <summary>Get user's referral information</summary>
        IEnumerator GetMyReferral(Action<MyReferralResponse> onSuccess, Action<string> onError);
        
        /// <summary>Get user's friends list</summary>
        IEnumerator GetFriends(Action<FriendsListResponse> onSuccess, Action<string> onError);
    }
    
    /// <summary>
    /// Service for communicating with the referral backend API
    /// </summary>
    public class ReferralAPIService : IReferralAPIService
    {
        #region Private Fields
        
        private string _authToken;
        private ReferralConfig _config;
        private ISharedDataService _sharedDataService;
        
        #endregion
        
        #region IService Implementation
        
        public int InitializationOrder => 5;
        public bool IsInitialized { get; private set; }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            // Get SharedDataService
            if (ServiceLocator.TryGet<ISharedDataService>(out var sharedDataService))
            {
                _sharedDataService = sharedDataService;
                
                // Try to load cached token from SharedData
                if (_sharedDataService.TryGetData<string>(SharedDataConstants.ReferralAuthToken, out var cachedToken))
                {
                    _authToken = cachedToken;
                    Debug.Log("<color=#00FFFF>[ReferralAPIService] Restored auth token from SharedData</color>");
                }
            }
            
            // Load config
            if (ServiceLocator.TryGet<IConfigService>(out var configService))
            {
                _config = configService.GetConfig<ReferralConfig>(ConfigsConstants.ReferralConfig);
            }
            
            if (_config == null)
            {
                Debug.LogError("[ReferralAPIService] ReferralConfig not found! Using default values.");
            }
            
            IsInitialized = true;
            Debug.Log("<color=#00FFFF>[ReferralAPIService] Initialized</color>");
        }
        
        public void Shutdown()
        {
            _authToken = null;
            IsInitialized = false;
        }
        
        #endregion
        
        #region IReferralAPIService Properties
        
        public string AuthToken => _authToken;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Authenticate user via Telegram initData
        /// </summary>
        public IEnumerator Authenticate(
            string initData, 
            string referralCode, 
            Action<AuthResponse> onSuccess, 
            Action<string> onError)
        {
            var request = new TelegramAuthRequest
            {
                initData = initData,
                referralCode = referralCode
            };
            
            string json = JsonUtility.ToJson(request);
            Debug.Log($"[ReferralAPIService] Sending auth request: {json}");
            
            using (var www = CreatePostRequest("/auth/telegram", json, useAuth: false))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Debug.Log($"[ReferralAPIService] Auth response: {www.downloadHandler.text}");
                        
                        // Server returns data directly, not wrapped in success/data
                        var response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                        
                        if (response != null && !string.IsNullOrEmpty(response.token))
                        {
                            // Store token in SharedData
                            _authToken = response.token;
                            _sharedDataService?.SetData(SharedDataConstants.ReferralAuthToken, _authToken);
                            
                            // Store referral code if present
                            if (response.player != null && !string.IsNullOrEmpty(response.player.referralCode))
                            {
                                _sharedDataService?.SetData(SharedDataConstants.ReferralCode, response.player.referralCode);
                            }
                            
                            Debug.Log($"<color=#00FF00>[ReferralAPIService] Authentication successful. Token stored in SharedData.</color>");
                            onSuccess?.Invoke(response);
                        }
                        else
                        {
                            Debug.LogError("[ReferralAPIService] Auth failed: Empty token in response");
                            onError?.Invoke("Empty token in response");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ReferralAPIService] Parse error: {ex.Message}");
                        onError?.Invoke($"Failed to parse response: {ex.Message}");
                    }
                }
                else
                {
                    HandleRequestError(www, onError);
                }
            }
        }
        
        /// <summary>
        /// Get user's referral information
        /// </summary>
        public IEnumerator GetMyReferral(Action<MyReferralResponse> onSuccess, Action<string> onError)
        {
            if (!IsAuthenticated)
            {
                onError?.Invoke("Not authenticated");
                yield break;
            }
            
            using (var www = CreateGetRequest("/social/my-referral"))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Debug.Log($"[ReferralAPIService] MyReferral response: {www.downloadHandler.text}");
                        
                        // Server returns data directly, not wrapped
                        var response = JsonUtility.FromJson<MyReferralResponse>(www.downloadHandler.text);
                        
                        if (response != null)
                        {
                            // Cache in SharedData
                            _sharedDataService?.SetData(SharedDataConstants.ReferralData, response);
                            onSuccess?.Invoke(response);
                        }
                        else
                        {
                            onError?.Invoke("Empty response");
                        }
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"Failed to parse response: {ex.Message}");
                    }
                }
                else
                {
                    HandleRequestError(www, onError);
                }
            }
        }
        
        /// <summary>
        /// Get user's friends list
        /// </summary>
        public IEnumerator GetFriends(Action<FriendsListResponse> onSuccess, Action<string> onError)
        {
            if (!IsAuthenticated)
            {
                onError?.Invoke("Not authenticated");
                yield break;
            }
            
            using (var www = CreateGetRequest("/social/friends"))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Debug.Log($"[ReferralAPIService] Friends response: {www.downloadHandler.text}");
                        
                        // Server returns data directly, not wrapped
                        var response = JsonUtility.FromJson<FriendsListResponse>(www.downloadHandler.text);
                        
                        if (response != null)
                        {
                            // Cache in SharedData
                            _sharedDataService?.SetData(SharedDataConstants.FriendsData, response);
                            onSuccess?.Invoke(response);
                        }
                        else
                        {
                            onError?.Invoke("Empty response");
                        }
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"Failed to parse response: {ex.Message}");
                    }
                }
                else
                {
                    HandleRequestError(www, onError);
                }
            }
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private string BaseUrl => _config?.BaseUrl ?? "http://localhost:8000";
        private int RequestTimeout => _config?.RequestTimeout ?? 30;
        
        private UnityWebRequest CreateGetRequest(string endpoint)
        {
            var www = UnityWebRequest.Get(BaseUrl + endpoint);
            www.timeout = RequestTimeout;
            
            if (IsAuthenticated)
            {
                www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            }
            
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
            {
                www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            }
            
            return www;
        }
        
        private void HandleRequestError(UnityWebRequest www, Action<string> onError)
        {
            string errorMessage = www.error;
            
            // Try to parse error response from server
            if (!string.IsNullOrEmpty(www.downloadHandler?.text))
            {
                try
                {
                    // Server may return error in { "detail": "message" } format (FastAPI standard)
                    var errorResponse = JsonUtility.FromJson<ApiErrorResponse>(www.downloadHandler.text);
                    if (!string.IsNullOrEmpty(errorResponse?.detail))
                    {
                        errorMessage = errorResponse.detail;
                    }
                }
                catch
                {
                    // Use default error message
                }
            }
            
            Debug.LogError($"[ReferralAPIService] Request failed: {errorMessage}");
            onError?.Invoke(errorMessage);
        }
        
        #endregion
    }
    
    #region Error Response
    
    /// <summary>
    /// FastAPI standard error response format
    /// </summary>
    [Serializable]
    public class ApiErrorResponse
    {
        public string detail;
    }
    
    #endregion
}
