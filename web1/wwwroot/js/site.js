// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Toggle menu on mobile
document.addEventListener('DOMContentLoaded', function() {
    const menuToggle = document.querySelector('.menu-toggle');
    const leftMenu = document.querySelector('.left-menu');
    const menuOverlay = document.querySelector('.menu-overlay');
    
    if (menuToggle && leftMenu && menuOverlay) {
        menuToggle.addEventListener('click', function() {
            leftMenu.classList.toggle('show');
            menuOverlay.classList.toggle('show');
        });

        // Đóng menu khi click vào overlay
        menuOverlay.addEventListener('click', function() {
            leftMenu.classList.remove('show');
            menuOverlay.classList.remove('show');
        });
    }
});
