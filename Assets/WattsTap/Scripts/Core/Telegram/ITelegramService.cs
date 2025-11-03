using System;
using WattsTap.Core;

namespace WattsTap.Core.React
{
    public interface ITelegramService : IService
    {
        string Id { get; }
        string UserName { get; }
        string InitData { get; }
        public TelegramService.SafeArea SafeAreaInsets { get; }
        event Action<string> OnReceivedInitData;

        // UniTask SendMessageAsync(string message, long chatId, CancellationToken cancellationToken = default);
    }
}
    