// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Everyday Girls: Companion Collector - Minimal JavaScript

// Confirm abandon action
function confirmAbandon(girlName) {
    return confirm("Abandon " + girlName + "? This cannot be undone.");
}

// Confirm adopt action (also defined inline in DailyAdopt view)
function confirmAdopt(girlName) {
    return confirm("Adopt " + girlName + "? You can only adopt one girl today.");
}
