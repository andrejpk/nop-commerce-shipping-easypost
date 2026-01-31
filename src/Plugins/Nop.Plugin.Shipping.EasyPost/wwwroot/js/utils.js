/**
 * Utility functions for EasyPost plugin
 */
(function() {
    'use strict';

    /**
     * Escape HTML to prevent XSS attacks
     * @param {string} unsafe - Unsafe HTML string
     * @returns {string} Escaped HTML string
     */
    function escapeHtml(unsafe) {
        if (typeof unsafe !== 'string') return '';

        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    /**
     * Format currency value safely
     * @param {number|null|undefined} value - Currency value
     * @returns {string} Formatted currency string
     */
    function formatCurrency(value) {
        return Number(value ?? 0).toFixed(2);
    }

    // Export to global namespace
    window.EasyPost = window.EasyPost || {};
    window.EasyPost.escapeHtml = escapeHtml;
    window.EasyPost.formatCurrency = formatCurrency;
})();
