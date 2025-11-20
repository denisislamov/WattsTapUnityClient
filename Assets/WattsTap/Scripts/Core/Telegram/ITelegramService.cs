using System;
using WattsTap.Core;

namespace WattsTap.Core.React
{
    public interface ITelegramService : IService
    {
        string Id { get; }
        string UserName { get; }
        string InitData { get; }
        TelegramService.TelegramAppEnvironment AppEnvironment { get; }
        
#if UNITY_EDITOR
        bool DebugSafeAreaInsets { get; }
#endif
        
        public TelegramService.SafeArea SafeAreaInsets { get; }
        event Action<string> OnReceivedInitData;
        public event Action<TelegramService.SafeArea> OnReceivedSafeAreaInsets;
        event Action<TelegramService.TelegramAppEnvironment> OnReceivedAppEnvironment;

        // UniTask SendMessageAsync(string message, long chatId, CancellationToken cancellationToken = default);
    }
}
    