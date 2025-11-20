using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using WattsTap.Constants;
using WattsTap.Core.Configs;
using WattsTap.Core.Configs.Telegram;

namespace WattsTap.Core.React
{
    public class TelegramService : MonoBehaviour, ITelegramService
    {
        [Serializable]
        public class SafeArea
        {
            public float Top;
            public float Bottom;
            public float Left;
            public float Right;
        }
        
        [Serializable]
        public class User
        {
            public long id;
            public string first_name;
            public string last_name;
            public string username;
            public string language_code;
            public bool is_premium;
            public bool added_to_attachment_menu;
            public bool allows_write_to_pm;
            public string photo_url;
            public string created_at;
            public string updated_at;
            public string token;

            public override string ToString()
            {
                return $"<color=#FFFF00>User ID: {id}, Name: {first_name} {last_name}, Username: {username}, Language: {language_code}, Is Premium: {is_premium}, " +
                       $"Added to Attachment Menu: {added_to_attachment_menu}, Allows Write to PM: {allows_write_to_pm}, Photo URL: {photo_url}, " +
                       $"Created At: {created_at}, Updated At: {updated_at}, Token: {token} </color>";
            }
        }
        
        [Serializable]
        private class Payload
        {
            public long chat_id;
            public string text;
        }
        
        [DllImport("__Internal")]
        private static extern void GetTelegramWebAppInitDataUnsafeUser();
        
        [SerializeField] private string _botToken;
        
#if UNITY_EDITOR
        [Header("Debug Settings (Editor Only)")]
        [SerializeField] private bool _useDebugSafeArea = false;
        [SerializeField] private SafeArea _debugSafeAreaInsets = new SafeArea();
#endif
        
        public string Id { get; private set; }
        public string UserName { get; private set; }
        public string InitData { get; private set; }

#if UNITY_EDITOR       
        public bool DebugSafeAreaInsets => _useDebugSafeArea;
#endif
        public SafeArea SafeAreaInsets => _safeAreaInsets;
        
        public int InitializationOrder => 1000;
        public bool IsInitialized { get; private set; }

        public event Action<string> OnReceivedUserId;
        public event Action<string> OnReceivedUserName;
        public event Action<string> OnReceivedInitData;
        public event Action<SafeArea> OnReceivedSafeAreaInsets;
        
        private SafeArea _safeAreaInsets = new SafeArea();
         
        public static long GetUserIdFromInitData(string initData)
        {
            var match = Regex.Match(initData, @"user=([^&]+)");

            if (!match.Success)
                return -1;

            var userJsonEncoded = match.Groups[1].Value;
            var userJson = Uri.UnescapeDataString(userJsonEncoded);

            var user = JsonUtility.FromJson<User>(userJson);
            return user?.id ?? -1;
        }
        
        /// <summary>
        /// Parses Telegram initData and extracts the User object
        /// Example: query_id=AAE71KpqAwAAADvUqmrbRN4l&user=%7B%22id%22%3A8232031291%2C%22first_name%22%3A%22Islamov%22...
        /// </summary>
        /// <param name="initData">The raw initData string from Telegram Web App</param>
        /// <returns>Parsed User object or null if parsing fails</returns>
        public static User ParseUserFromInitData(string initData)
        {
            if (string.IsNullOrEmpty(initData))
            {
                Debug.LogWarning("InitData is null or empty");
                return null;
            }

            var match = Regex.Match(initData, @"user=([^&]+)");

            if (!match.Success)
            {
                Debug.LogWarning("Failed to find user parameter in initData");
                return null;
            }

            var userJsonEncoded = match.Groups[1].Value;
            var userJson = Uri.UnescapeDataString(userJsonEncoded);

            try
            {
                var user = JsonUtility.FromJson<User>(userJson);
                if (user == null)
                {
                    Debug.LogWarning("Failed to deserialize user JSON");
                    return null;
                }
                
                Debug.Log($"Successfully parsed user from initData: {user}");
                return user;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing user from initData: {ex.Message}");
                return null;
            }
        }
        
        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
            
#if !UNITY_WEBGL || UNITY_EDITOR
             var configService = ServiceLocator.Get<IConfigService>();
             TelegramDebugData telegramDebugData = configService.GetConfig<TelegramDebugData>(ConfigsConstants.TelegramDebugData);
             ReceiveInitData(telegramDebugData.InitData);
#endif

#if UNITY_EDITOR
            if (_useDebugSafeArea)
            {
                _safeAreaInsets = _debugSafeAreaInsets;
                Debug.Log($"Using debug safe area insets: Top={_safeAreaInsets.Top}, Bottom={_safeAreaInsets.Bottom}, Left={_safeAreaInsets.Left}, Right={_safeAreaInsets.Right}");
                
                // Trigger the event so TelegramSafeZoneApplier can apply the debug safe area
                OnReceivedSafeAreaInsets?.Invoke(_safeAreaInsets);
            }
#endif
        }
        
        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
        }

        public void ReceiveUserId(string id)
        {
            Id = id;
            OnReceivedUserId?.Invoke(id);
            Debug.Log($"Received User ID: {id}");
        }

        public void ReceiveUserName(string userName)
        {
            UserName = userName;
            OnReceivedUserName?.Invoke(userName);
            Debug.Log($"Received User Name: {userName}");
        }

        public void ReceiveInitData(string initData)
        {
            InitData = initData;
            OnReceivedInitData?.Invoke(initData);
            Debug.Log($"Received Init Data: {initData}");
        }

        public void ReceiveSafeAreaInsets(string safeAreaInsets)
        {
            var result = safeAreaInsets.Split(',');
            Debug.LogErrorFormat("SafeAreaInsets top {0} left {1} bottom {2} right {3}", result[0], result[1], result[2], result[3]);
            _safeAreaInsets = new SafeArea
            {
                Top = float.Parse(result[0]),
                Left = float.Parse(result[1]),
                Bottom = float.Parse(result[2]),
                Right = float.Parse(result[3])
            };
            
            Debug.LogErrorFormat($"Received Safe Area Insets: {_safeAreaInsets}");
            OnReceivedSafeAreaInsets?.Invoke(_safeAreaInsets);
        }
        
        public void OnCreateUnityInstance()
        {
            GetTelegramWebAppInitDataUnsafeUser();
        }
        
//         public async UniTask SendMessageAsync(string message, long chatId, CancellationToken cancellationToken = default)
//         {
//             if (string.IsNullOrEmpty(_botToken))
//             {
//                 Debug.LogError("Bot token is not set. Cannot send message.");
//                 return;
//             }
//             
//             string url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
//
//             // if (!long.TryParse(chatId, out var id)) 
//             // {
//             //     Debug.LogError("Invalid chat ID. Cannot send message.");
//             //     return;
//             // }
//             
//             Payload payload = new Payload
//                 { chat_id = chatId, text = message };
//             
//             string jsonData = JsonUtility.ToJson(payload);
//
//             using UnityWebRequest request = new UnityWebRequest(url, "POST");
//             
//             byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
//             request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             request.downloadHandler = new DownloadHandlerBuffer();
//             request.SetRequestHeader("Content-Type", "application/json");
//
//             var asyncOp = request.SendWebRequest();
//
//             while (!asyncOp.isDone)
//             {
//                 if (cancellationToken.IsCancellationRequested)
//                 {
//                     request.Abort();
//                     Debug.LogWarning("Telegram request cancelled.");
//                     cancellationToken.ThrowIfCancellationRequested();
//                 }
//                 
//                 await UniTask.Yield();
//             }
//
// #if UNITY_2020_1_OR_NEWER
//             if (request.result != UnityWebRequest.Result.Success)
// #else
//             if (request.isNetworkError || request.isHttpError)
// #endif
//             {
//                 Debug.LogError($"Telegram error: {request.error}");
//             }
//             else
//             {
//                 Debug.Log("Telegram message sent successfully!");
//             }
//         }
    }
}

