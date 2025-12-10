mergeInto(LibraryManager.library, {
    TelegramHapticImpact: function(stylePtr) {
        var style = UTF8ToString(stylePtr);
        if (window.Telegram && window.Telegram.WebApp && window.Telegram.WebApp.HapticFeedback) {
            window.Telegram.WebApp.HapticFeedback.impactOccurred(style);
        }
    },
    
    TelegramHapticNotification: function(typePtr) {
        var type = UTF8ToString(typePtr);
        if (window.Telegram && window.Telegram.WebApp && window.Telegram.WebApp.HapticFeedback) {
            window.Telegram.WebApp.HapticFeedback.notificationOccurred(type);
        }
    },
    
    TelegramHapticSelectionChanged: function() {
        if (window.Telegram && window.Telegram.WebApp && window.Telegram.WebApp.HapticFeedback) {
            window.Telegram.WebApp.HapticFeedback.selectionChanged();
        }
    }
});

