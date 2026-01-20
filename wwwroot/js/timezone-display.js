// Display daily reset time in user's local timezone
(function() {
    'use strict';
    
    function initializeTimezoneDisplay() {
        var resetTimeElement = document.querySelector('[data-reset-time-utc]');
        if (!resetTimeElement) return;
        
        var resetHourUtc = parseInt(resetTimeElement.getAttribute('data-reset-time-utc'), 10);
        if (isNaN(resetHourUtc)) return;
        
        try {
            // Get current date in UTC
            var now = new Date();
            var utcNow = new Date(now.getTime() + (now.getTimezoneOffset() * 60000));
            
            // Calculate next reset time (today or tomorrow at specified UTC hour)
            var nextReset = new Date(Date.UTC(
                utcNow.getUTCFullYear(),
                utcNow.getUTCMonth(),
                utcNow.getUTCDate(),
                resetHourUtc,
                0,
                0
            ));
            
            // If reset time has already passed today, move to tomorrow
            if (nextReset <= utcNow) {
                nextReset.setUTCDate(nextReset.getUTCDate() + 1);
            }
            
            // Format in user's local timezone
            var formatter = new Intl.DateTimeFormat('en-US', {
                hour: 'numeric',
                minute: '2-digit'
            });
            
            var localResetTime = formatter.format(nextReset);
            resetTimeElement.textContent = localResetTime;
        } catch (error) {
            // Fallback to UTC display if formatting fails
            resetTimeElement.textContent = resetHourUtc + ':00 UTC';
        }
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeTimezoneDisplay);
    } else {
        initializeTimezoneDisplay();
    }
})();
