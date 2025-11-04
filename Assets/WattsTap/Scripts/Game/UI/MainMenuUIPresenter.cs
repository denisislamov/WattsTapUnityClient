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
    public class MainMenuUIPresenter : UIBasePresenter<MainMenuUIView, MainMenuUIModel>
    {
        private CancellationTokenSource _avatarLoadCts;

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
            
            // Инициализация начальных значений
            OnTotalCoinsChanged(Model.TotalCoins.Value);
            OnCoinsPerTapChanged(Model.CoinsPerTap.Value);
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
            
            var sharedDataService = ServiceLocator.Get<ISharedDataService>();
            if (sharedDataService.TryGetData(SharedDataConstants.TelegramUser, out TelegramService.User telegramUser))
            {
                Model.PlayerName.Value = telegramUser.first_name + " " + telegramUser.last_name + " " + telegramUser.username;
                
                // Загрузка аватара если присутствует URL
                // if (!string.IsNullOrEmpty(telegramUser.photo_url))
                // {
                //     _avatarLoadCts = new CancellationTokenSource();
                //     LoadAvatarAsync(telegramUser.photo_url, _avatarLoadCts.Token).Forget();
                // }
            }
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
            }
        }
    }
}