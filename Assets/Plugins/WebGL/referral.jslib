// WattsTap Referral System - JavaScript Library for WebGL
// Provides functionality for sharing and copying referral links

mergeInto(LibraryManager.library, {
    
    /**
     * Share a link via Telegram's share dialog
     * @param {string} urlPtr - Pointer to the URL string
     * @param {string} textPtr - Pointer to the share text string
     */
    ShareToTelegram: function(urlPtr, textPtr) {
        var url = UTF8ToString(urlPtr);
        var text = UTF8ToString(textPtr);
        
        console.log('[Referral] Sharing to Telegram:', url);
        
        if (window.Telegram && window.Telegram.WebApp) {
            // Use Telegram WebApp's native share
            var shareUrl = 'https://t.me/share/url?url=' + encodeURIComponent(url) + '&text=' + encodeURIComponent(text);
            
            try {
                window.Telegram.WebApp.openTelegramLink(shareUrl);
                console.log('[Referral] Opened Telegram share dialog');
            } catch (e) {
                console.error('[Referral] Failed to open Telegram link:', e);
                // Fallback to window.open
                window.open(shareUrl, '_blank');
            }
        } else {
            // Fallback for non-Telegram environment
            var shareUrl = 'https://t.me/share/url?url=' + encodeURIComponent(url) + '&text=' + encodeURIComponent(text);
            window.open(shareUrl, '_blank');
        }
    },
    
    /**
     * Copy text to clipboard
     * @param {string} textPtr - Pointer to the text string to copy
     */
    CopyToClipboard: function(textPtr) {
        var text = UTF8ToString(textPtr);
        
        console.log('[Referral] Copying to clipboard:', text);
        
        // Try modern clipboard API first
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text)
                .then(function() {
                    console.log('[Referral] Successfully copied to clipboard');
                    
                    // Show Telegram popup if available
                    if (window.Telegram && window.Telegram.WebApp && window.Telegram.WebApp.showPopup) {
                        window.Telegram.WebApp.showPopup({
                            title: 'Copied!',
                            message: 'Invite link copied to clipboard',
                            buttons: [{type: 'ok'}]
                        });
                    }
                })
                .catch(function(err) {
                    console.error('[Referral] Clipboard API failed:', err);
                    fallbackCopyToClipboard(text);
                });
        } else {
            // Fallback for older browsers
            fallbackCopyToClipboard(text);
        }
        
        function fallbackCopyToClipboard(text) {
            var textArea = document.createElement('textarea');
            textArea.value = text;
            
            // Avoid scrolling to bottom
            textArea.style.top = '0';
            textArea.style.left = '0';
            textArea.style.position = 'fixed';
            textArea.style.opacity = '0';
            
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            
            try {
                var successful = document.execCommand('copy');
                if (successful) {
                    console.log('[Referral] Fallback copy successful');
                } else {
                    console.error('[Referral] Fallback copy failed');
                }
            } catch (err) {
                console.error('[Referral] Fallback copy error:', err);
            }
            
            document.body.removeChild(textArea);
        }
    },
    
    /**
     * Open a URL in the browser
     * Uses Telegram's openLink if available
     * @param {string} urlPtr - Pointer to the URL string
     */
    OpenExternalLink: function(urlPtr) {
        var url = UTF8ToString(urlPtr);
        
        console.log('[Referral] Opening external link:', url);
        
        if (window.Telegram && window.Telegram.WebApp && window.Telegram.WebApp.openLink) {
            window.Telegram.WebApp.openLink(url);
        } else {
            window.open(url, '_blank');
        }
    }
});





