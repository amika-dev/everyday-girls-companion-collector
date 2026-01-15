// Countdown timer for daily reset
(function() {
    'use strict';
    
    var intervalId = null;
    
    function initializeCountdown() {
        var timerElement = document.querySelector('[data-countdown-seconds]');
        if (!timerElement) return;
        
        var totalSeconds = parseInt(timerElement.getAttribute('data-countdown-seconds'), 10);
        if (isNaN(totalSeconds)) return;
        
        // Clear any existing interval before creating a new one
        if (intervalId !== null) {
            clearInterval(intervalId);
            intervalId = null;
        }
        
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
            timerElement.textContent = formatTime(totalSeconds);
            
            if (totalSeconds > 0) {
                totalSeconds--;
            }
        }
        
        // Render immediately
        updateTimer();
        
        // Update every second and store the interval ID
        intervalId = setInterval(updateTimer, 1000);
    }
    
    function cleanup() {
        if (intervalId !== null) {
            clearInterval(intervalId);
            intervalId = null;
        }
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeCountdown);
    } else {
        initializeCountdown();
    }
    
    // Clean up on page unload
    window.addEventListener('beforeunload', cleanup);
    window.addEventListener('pagehide', cleanup);
})();
