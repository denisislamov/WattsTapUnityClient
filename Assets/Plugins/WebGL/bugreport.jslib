mergeInto(LibraryManager.library, {
    /**
     * Opens the default mail client with pre-filled email, subject and body
     * @param {number} emailPtr - Pointer to email address string
     * @param {number} subjectPtr - Pointer to subject string
     * @param {number} bodyPtr - Pointer to body string
     */
    OpenMailClient: function(emailPtr, subjectPtr, bodyPtr) {
        var email = UTF8ToString(emailPtr);
        var subject = UTF8ToString(subjectPtr);
        var body = UTF8ToString(bodyPtr);
        
        var mailtoUrl = 'mailto:' + encodeURIComponent(email) + 
                        '?subject=' + encodeURIComponent(subject) + 
                        '&body=' + encodeURIComponent(body);
        
        window.open(mailtoUrl, '_blank');
    },
    
    /**
     * Gets the browser's user agent string
     * @returns {string} - The user agent string
     */
    GetBrowserUserAgent: function() {
        var userAgent = navigator.userAgent || 'Unknown';
        var bufferSize = lengthBytesUTF8(userAgent) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(userAgent, buffer, bufferSize);
        return buffer;
    },
    
    /**
     * Gets the browser's language
     * @returns {string} - The browser language code
     */
    GetBrowserLanguage: function() {
        var language = navigator.language || navigator.userLanguage || 'Unknown';
        var bufferSize = lengthBytesUTF8(language) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(language, buffer, bufferSize);
        return buffer;
    },
    
    /**
     * Gets the screen orientation
     * @returns {string} - 'portrait' or 'landscape'
     */
    GetScreenOrientation: function() {
        var orientation = 'unknown';
        if (screen.orientation) {
            orientation = screen.orientation.type.includes('portrait') ? 'portrait' : 'landscape';
        } else if (window.innerWidth && window.innerHeight) {
            orientation = window.innerWidth > window.innerHeight ? 'landscape' : 'portrait';
        }
        var bufferSize = lengthBytesUTF8(orientation) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(orientation, buffer, bufferSize);
        return buffer;
    },
    
    /**
     * Gets additional browser/WebGL info as JSON string
     * @returns {string} - JSON string with additional info
     */
    GetWebGLInfo: function() {
        var info = {
            platform: navigator.platform || 'Unknown',
            cookiesEnabled: navigator.cookieEnabled,
            onLine: navigator.onLine,
            touchSupport: ('ontouchstart' in window) || (navigator.maxTouchPoints > 0),
            screenWidth: window.screen.width,
            screenHeight: window.screen.height,
            devicePixelRatio: window.devicePixelRatio || 1,
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone || 'Unknown',
            webglRenderer: 'Unknown',
            webglVendor: 'Unknown'
        };
        
        // Try to get WebGL renderer info
        try {
            var canvas = document.createElement('canvas');
            var gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            if (gl) {
                var debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
                if (debugInfo) {
                    info.webglRenderer = gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL);
                    info.webglVendor = gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL);
                }
            }
        } catch (e) {
            // WebGL info not available
        }
        
        var jsonString = JSON.stringify(info);
        var bufferSize = lengthBytesUTF8(jsonString) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(jsonString, buffer, bufferSize);
        return buffer;
    },
    
    /**
     * Shows a notification to the user (using Telegram WebApp if available, otherwise browser notification)
     * @param {number} messagePtr - Pointer to message string
     * @param {number} isSuccess - 1 for success, 0 for error
     */
    ShowBugReportNotification: function(messagePtr, isSuccess) {
        var message = UTF8ToString(messagePtr);
        
        // Try Telegram WebApp popup first
        if (window.Telegram && window.Telegram.WebApp && window.Telegram.WebApp.showPopup) {
            window.Telegram.WebApp.showPopup({
                title: isSuccess ? 'Success' : 'Error',
                message: message,
                buttons: [{ type: 'ok' }]
            });
        } else {
            // Fallback to browser alert
            alert(message);
        }
    },
    
    /**
     * Copies text to clipboard
     * @param {number} textPtr - Pointer to text string
     * @returns {number} - 1 if successful, 0 if failed
     */
    CopyToClipboard: function(textPtr) {
        var text = UTF8ToString(textPtr);
        
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(function() {
                return 1;
            }).catch(function() {
                return 0;
            });
            return 1; // Assume success for async operation
        }
        
        // Fallback for older browsers
        try {
            var textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand('copy');
            document.body.removeChild(textarea);
            return 1;
        } catch (e) {
            return 0;
        }
    },
    
    /**
     * Gets the current URL (useful for debugging)
     * @returns {string} - The current page URL
     */
    GetCurrentURL: function() {
        var url = window.location.href || 'Unknown';
        var bufferSize = lengthBytesUTF8(url) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(url, buffer, bufferSize);
        return buffer;
    },
    
    /**
     * Gets performance metrics if available
     * @returns {string} - JSON string with performance info
     */
    GetPerformanceInfo: function() {
        var info = {
            memoryUsed: 0,
            memoryTotal: 0,
            fps: 0
        };
        
        // Try to get memory info (Chrome only)
        if (performance.memory) {
            info.memoryUsed = Math.round(performance.memory.usedJSHeapSize / 1048576); // Convert to MB
            info.memoryTotal = Math.round(performance.memory.totalJSHeapSize / 1048576);
        }
        
        var jsonString = JSON.stringify(info);
        var bufferSize = lengthBytesUTF8(jsonString) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(jsonString, buffer, bufferSize);
        return buffer;
    }
});

