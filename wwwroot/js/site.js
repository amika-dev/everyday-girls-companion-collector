// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Everyday Girls: Companion Collector - Minimal JavaScript

// Confirm abandon action - gentle conversational tone
function confirmAbandon(girlName) {
    return confirm("Part ways with " + girlName + "?\n\nShe'll be okay, but you won't see her in your collection anymore.");
}

// Confirm adopt action - warm inviting tone
function confirmAdopt(girlName) {
    return confirm("Welcome " + girlName + " home?\n\nYou can only welcome one person each day, so choose who feels right 💖");
}
