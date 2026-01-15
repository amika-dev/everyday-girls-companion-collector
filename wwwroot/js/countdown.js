// Countdown timer for daily reset
(function() {
    'use strict';
    
    function initializeCountdown() {
        var timerElement = document.querySelector('[data-countdown-seconds]');
        if (!timerElement) return;
        
        var initialSeconds = parseInt(timerElement.getAttribute('data-countdown-seconds'), 10);
        if (isNaN(initialSeconds)) return;
        
        // Calculate target timestamp to avoid desync
        var targetTime = Date.now() + (initialSeconds * 1000);
        
        function formatTime(seconds) {
            if (seconds <= 0) {
                return '00:00:00';
            }
            
            var hours = Math.floor(seconds / 3600);
            var minutes = Math.floor((seconds % 3600) / 60);
            var secs = seconds % 60;
            
            return hours + ':' + 
                   (minutes < 10 ? '0' : '') + minutes + ':' + 
                   (secs < 10 ? '0' : '') + secs;
        }
        
        function updateTimer() {
            // Calculate remaining time from target timestamp
            var remainingMs = targetTime - Date.now();
            var remainingSeconds = Math.max(0, Math.floor(remainingMs / 1000));
            
            timerElement.textContent = formatTime(remainingSeconds);
            
            // Reload page when countdown reaches zero
            if (remainingSeconds <= 0) {
                location.reload();
            }
        }
        
        // Render immediately
        updateTimer();
        
        // Update every second
        setInterval(updateTimer, 1000);
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeCountdown);
    } else {
        initializeCountdown();
    }
})();
