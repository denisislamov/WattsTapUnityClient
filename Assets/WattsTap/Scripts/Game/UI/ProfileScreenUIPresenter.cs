using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.React;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class ProfileScreenUIPresenter : UIBasePresenter<ProfileScreenUIView, ProfileScreenUIModel>
    {
        private ISharedDataService _sharedDataService;
        private IUIService _uiService;
        private CancellationTokenSource _avatarLoadCts;

        protected override void OnInit()
        {
            Model.PlayerName.OnValueChanged += OnPlayerNameChanged;
            Model.Level.OnValueChanged += OnLevelChanged;
            Model.CurrentXp.OnValueChanged += OnProgressChanged;
            Model.XpToNextLevel.OnValueChanged += OnProgressChanged;

            _sharedDataService = ServiceLocator.Get<ISharedDataService>();
            _sharedDataService.OnDataUpdated += OnSharedDataUpdated;

            if (_sharedDataService.TryGetData(SharedDataConstants.TelegramUser, out TelegramService.User telegramUser))
            {
                Model.PlayerName.Value = ComposePlayerName(telegramUser);
                // TryStartAvatarLoad(telegramUser.photo_url);
            }

            if (View.CloseButton != null)
            {
                View.CloseButton.onClick.AddListener(OnCloseClicked);
            }

            ServiceLocator.TryGet(out _uiService);

            OnPlayerNameChanged(Model.PlayerName.Value);
            OnLevelChanged(Model.Level.Value);
            UpdateProgressBar();
        }

        private void OnCloseClicked()
        {
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.ProfileScreen);
        }

        private void OnSharedDataUpdated(string key)
        {
            if (key != SharedDataConstants.TelegramUser)
            {
                return;
            }

            if (_sharedDataService.TryGetData(SharedDataConstants.TelegramUser, out TelegramService.User telegramUser))
            {
                Model.PlayerName.Value = ComposePlayerName(telegramUser);
                // TryStartAvatarLoad(telegramUser.photo_url);
            }
        }

        private void TryStartAvatarLoad(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            _avatarLoadCts?.Cancel();
            _avatarLoadCts?.Dispose();
            _avatarLoadCts = new CancellationTokenSource();
        //    LoadAvatarAsync(url, _avatarLoadCts.Token).Forget();
        }

        private async UniTask LoadAvatarAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
                {
                    await request.SendWebRequest().WithCancellation(cancellationToken);

#if UNITY_2020_1_OR_NEWER
                    if (request.result == UnityWebRequest.Result.Success)
#else
                    if (!request.isNetworkError && !request.isHttpError)
#endif
                    {
                        var texture = DownloadHandlerTexture.GetContent(request);
                        if (texture != null)
                        {
                            var avatarSprite = Sprite.Create(
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f)
                            );
                            View.UpdateAvatar(avatarSprite);
                        }
                    }
                    else
                    {
                        Debug.LogError($"[ProfileScreen] Failed to load avatar: {request.error}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[ProfileScreen] Avatar loading cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProfileScreen] Error loading avatar: {ex.Message}");
            }
        }

        private void OnPlayerNameChanged(string name)
        {
            var value = string.IsNullOrWhiteSpace(name) ? "Player" : name;
            View.UpdateUserName(value);
        }

        private void OnLevelChanged(int level)
        {
            View.UpdateCurrentLevel(level);
        }

        private void OnProgressChanged(long _)
        {
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            var targetXp = Mathf.Max(1, Model.XpToNextLevel.Value);
            var progress = (float)Model.CurrentXp.Value / targetXp;
            View.UpdateLevelProgressBar(progress);
        }

        private static string ComposePlayerName(TelegramService.User user)
        {
            if (user == null)
            {
                return "Player";
            }

            string firstName = user.first_name ?? string.Empty;
            string lastName = user.last_name ?? string.Empty;
            string username = string.IsNullOrEmpty(user.username) ? string.Empty : $" @{user.username}";

            var combined = $"{firstName} {lastName}".Trim();
            if (string.IsNullOrEmpty(combined))
            {
                return string.IsNullOrEmpty(username) ? "Player" : username.Trim();
            }

            return $"{combined}{username}";
        }

        protected override void OnDispose()
        {
            _avatarLoadCts?.Cancel();
            _avatarLoadCts?.Dispose();
            _avatarLoadCts = null;

            if (_sharedDataService != null)
            {
                _sharedDataService.OnDataUpdated -= OnSharedDataUpdated;
            }

            if (View?.CloseButton != null)
            {
                View.CloseButton.onClick.RemoveListener(OnCloseClicked);
            }

            if (Model != null)
            {
                Model.PlayerName.OnValueChanged -= OnPlayerNameChanged;
                Model.Level.OnValueChanged -= OnLevelChanged;
                Model.CurrentXp.OnValueChanged -= OnProgressChanged;
                Model.XpToNextLevel.OnValueChanged -= OnProgressChanged;
            }
        }
    }
}

