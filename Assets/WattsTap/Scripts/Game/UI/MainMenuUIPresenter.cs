using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.React;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Avatars;

namespace WattsTap.Game.UI
{
    public class MainMenuUIPresenter : UIBasePresenter<MainMenuUIView, MainMenuUIModel>
    {
        private CancellationTokenSource _avatarLoadCts;
        private ISharedDataService _sharedDataService;
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;
        private IAvatarsService _avatarsService;
        private bool _tapHintHidden;

        protected override void OnInit()
        {
            // Подписка на изменения в модели
            Model.PlayerName.OnValueChanged += OnPlayerNameChanged;
            Model.PlayerLevel.OnValueChanged += OnPlayerLevelChanged;
            Model.IsLoading.OnValueChanged += OnLoadingStateChanged;
            Model.TotalCoins.OnValueChanged += OnTotalCoinsChanged;
            Model.CoinsPerTap.OnValueChanged += OnCoinsPerTapChanged;
            Model.HitsCurrent.OnValueChanged += OnHitsChanged;
            Model.HitsMax.OnValueChanged += OnHitsChanged;
            Model.Level.OnValueChanged += OnLevelChanged;
            Model.CurrentXp.OnValueChanged += OnCurrentXpChanged;
            
            // Инициализация начальных значений
            OnTotalCoinsChanged(Model.TotalCoins.Value);
            OnCoinsPerTapChanged(Model.CoinsPerTap.Value);
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
            
            _sharedDataService = ServiceLocator.Get<ISharedDataService>();
            ServiceLocator.TryGet(out _hapticService);
            
            // Subscribe to avatar changes
            if (ServiceLocator.TryGet(out _avatarsService))
            {
                _avatarsService.OnAvatarChanged += OnAvatarChanged;
                _avatarsService.OnTelegramAvatarLoaded += OnTelegramAvatarLoaded;
                UpdateCurrentAvatar();
            }
            
            if (_sharedDataService.TryGetData(SharedDataConstants.TelegramUser, out TelegramService.User telegramUser))
            {
                Model.PlayerName.Value = GetPlayerName(telegramUser);
                
                // Загрузка аватара если присутствует URL
                // if (!string.IsNullOrEmpty(telegramUser.photo_url))
                // {
                //     _avatarLoadCts = new CancellationTokenSource();
                //     LoadAvatarAsync(telegramUser.photo_url, _avatarLoadCts.Token).Forget();
                // }
            }
            
            _sharedDataService.OnDataUpdated += OnSharedDataUpdated;

            View.ChangeSkinButton.onClick.AddListener(ChangeSkinButtonOnClick);
            if (View.ProfileButton != null)
            {
                View.ProfileButton.onClick.AddListener(OnProfileButtonClicked);
            }

            if (View.FriendsReferralButton != null)
            {
                View.FriendsReferralButton.onClick.AddListener(OnFriendsReferralButtonClicked);
            }

            if (View.ShopButton != null)
            {
                View.ShopButton.onClick.AddListener(OnShopButtonClicked);
            }

            if (View.QuestsButton != null)
            {
                View.QuestsButton.onClick.AddListener(OnQuestsButtonClicked);
            }

            if (View.BugReportButton != null)
            {
                View.BugReportButton.onClick.AddListener(OnBugReportButtonClicked);
            }

            View.SetDefaultSkin();
            
            View.UpdateCurrentLevel(Model.Level.Value);
            View.UpdateLevelProgressBar(Model.CurrentXp.Value);
            
            View.UpdateVersionText(Application.version);
        }

        private void OnCurrentXpChanged(long value)
        {
            var progress = (float) value / Model.XpToNextLevel.Value;
            View.UpdateLevelProgressBar(progress);
        }

        private void OnLevelChanged(int value)
        {
            View.UpdateCurrentLevel(value);
        }

        private void OnProfileButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }
            
            _uiService?.Open(UIConstants.ProfileScreen);
        }

        private void OnFriendsReferralButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Open(UIConstants.FriendsReferralScreen);
        }

        private void OnShopButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Open(UIConstants.ShopScreen);
        }

        private void OnQuestsButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Open(UIConstants.QuestsScreen);
        }

        private void OnBugReportButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Open(UIConstants.BugReportScreen);
        }

        private void ChangeSkinButtonOnClick()
        {
            _hapticService?.ButtonPressed();
            View.SetNextSkin();
        }

        private void OnSharedDataUpdated(string name)
        {
            if (name != SharedDataConstants.TelegramUser)
            {
                return;
            }
            
            if (_sharedDataService.TryGetData(SharedDataConstants.TelegramUser, out TelegramService.User telegramUser))
            {
                Model.PlayerName.Value = GetPlayerName(telegramUser);
            }
        }

        /// <summary>
        /// Returns player name based on priority:
        /// 1. username if present
        /// 2. first_name + last_name if both present
        /// 3. first_name or last_name (whichever is present)
        /// 4. user id if present
        /// 5. "Player" as fallback
        /// </summary>
        private string GetPlayerName(TelegramService.User user)
        {
            // 1) username if present
            if (!string.IsNullOrEmpty(user.username))
            {
                return user.username;
            }

            // 2) first_name + last_name if both present
            var hasFirstName = !string.IsNullOrEmpty(user.first_name);
            var hasLastName = !string.IsNullOrEmpty(user.last_name);
            
            if (hasFirstName && hasLastName)
            {
                return user.first_name + " " + user.last_name;
            }

            // 3) first_name or last_name (whichever is present)
            if (hasFirstName)
            {
                return user.first_name;
            }
            
            if (hasLastName)
            {
                return user.last_name;
            }

            // 4) user id if present
            if (user.id != 0)
            {
                return user.id.ToString();
            }

            // 5) fallback
            return "Player";
        }

        /// <summary>
        /// Асинхронная загрузка аватара пользователя из URL
        /// </summary>
        /// <param name="url">URL изображения аватара</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        private async UniTask LoadAvatarAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("Avatar URL is empty");
                return;
            }

            Debug.Log($"Loading avatar from URL: {url}");

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
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        
                        if (texture != null)
                        {
                            // Создаем спрайт из загруженной текстуры
                            Sprite avatarSprite = Sprite.Create(
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f)
                            );
                            
                            View.UpdateAvatar(avatarSprite);
                            Debug.Log("Avatar loaded successfully");
                        }
                        else
                        {
                            Debug.LogError("Failed to extract texture from download");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to load avatar: {request.error}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Avatar loading was cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading avatar: {ex.Message}");
            }
        }

        private void OnAvatarChanged(string avatarId)
        {
            UpdateCurrentAvatar();
        }
        
        private void OnTelegramAvatarLoaded(Sprite sprite)
        {
            // If current avatar is Telegram avatar, update it
            if (_avatarsService != null && _avatarsService.GetCurrentAvatarId() == _avatarsService.TelegramAvatarId)
            {
                View.UpdateAvatar(sprite);
            }
        }
        
        private void UpdateCurrentAvatar()
        {
            if (_avatarsService == null)
            {
                return;
            }
            
            var sprite = _avatarsService.GetCurrentAvatarSprite();
            if (sprite != null)
            {
                View.UpdateAvatar(sprite);
            }
        }

        private void OnPlayerNameChanged(string newName)
        {
            View.UpdateUserName(newName);
        }

        private void OnPlayerLevelChanged(int newLevel)
        {
            // Здесь можно обновить View или выполнить другую логику
        }

        private void OnLoadingStateChanged(bool isLoading)
        {
            // Здесь можно обновить View или выполнить другую логику
        }

        private void OnTotalCoinsChanged(long totalCoins)
        {
            View.UpdateTotalCoins(totalCoins);
            
            // Hide tap hint on first coin earning
            if (!_tapHintHidden && totalCoins > 0)
            {
                _tapHintHidden = true;
                View.HideTapHint();
            }
        }

        private void OnCoinsPerTapChanged(int coinsPerTap)
        {
            View.UpdateCoinsPerTap(coinsPerTap);
        }

        private void OnHitsChanged(int _)
        {
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
        }

        protected override void OnDispose()
        {
            // Отмена загрузки аватара при удалении
            _avatarLoadCts?.Cancel();
            _avatarLoadCts?.Dispose();
            _avatarLoadCts = null;
            
            // Unsubscribe from avatar service
            if (_avatarsService != null)
            {
                _avatarsService.OnAvatarChanged -= OnAvatarChanged;
                _avatarsService.OnTelegramAvatarLoaded -= OnTelegramAvatarLoaded;
            }

            View.ChangeSkinButton.onClick.RemoveListener(ChangeSkinButtonOnClick);
            if (View?.ProfileButton != null)
            {
                View.ProfileButton.onClick.RemoveListener(OnProfileButtonClicked);
            }

            if (View?.FriendsReferralButton != null)
            {
                View.FriendsReferralButton.onClick.RemoveListener(OnFriendsReferralButtonClicked);
            }

            if (View?.ShopButton != null)
            {
                View.ShopButton.onClick.RemoveListener(OnShopButtonClicked);
            }

            if (View?.QuestsButton != null)
            {
                View.QuestsButton.onClick.RemoveListener(OnQuestsButtonClicked);
            }

            if (View?.BugReportButton != null)
            {
                View.BugReportButton.onClick.RemoveListener(OnBugReportButtonClicked);
            }
            
            // Отписка от событий модели
            if (Model != null)
            {
                Model.PlayerName.OnValueChanged -= OnPlayerNameChanged;
                Model.PlayerLevel.OnValueChanged -= OnPlayerLevelChanged;
                Model.IsLoading.OnValueChanged -= OnLoadingStateChanged;
                Model.TotalCoins.OnValueChanged -= OnTotalCoinsChanged;
                Model.CoinsPerTap.OnValueChanged -= OnCoinsPerTapChanged;
                Model.HitsCurrent.OnValueChanged -= OnHitsChanged;
                Model.HitsMax.OnValueChanged -= OnHitsChanged;
                Model.Level.OnValueChanged -= OnLevelChanged;
                Model.CurrentXp.OnValueChanged -= OnCurrentXpChanged;

            }
        }
    }
}