/**
 * Pattern matching utility for EasyPost service filtering
 * Supports wildcards in carrier and service patterns
 */
(function() {
    'use strict';

    /**
     * Checks if a service matches a pattern
     * Pattern format: "Carrier:Service" (e.g., "USPS:Priority", "FedEx:*", "*:Ground")
     * @param {Object} service - Service object with carrier and service properties
     * @param {string} pattern - Pattern to match against
     * @returns {boolean} True if the service matches the pattern
     */
    function matchesPattern(service, pattern) {
        if (!pattern) return false;

        const [carrierPattern, servicePattern] = pattern.split(':');
        if (!carrierPattern || !servicePattern) return false;

        const matchCarrier = carrierPattern === '*' ||
            (carrierPattern.endsWith('*') && service.carrier.startsWith(carrierPattern.slice(0, -1))) ||
            (carrierPattern.startsWith('*') && service.carrier.endsWith(carrierPattern.slice(1))) ||
            service.carrier.toLowerCase() === carrierPattern.toLowerCase();

        const matchService = servicePattern === '*' ||
            (servicePattern.endsWith('*') && service.service.startsWith(servicePattern.slice(0, -1))) ||
            (servicePattern.startsWith('*') && service.service.endsWith(servicePattern.slice(1))) ||
            service.service.toLowerCase() === servicePattern.toLowerCase();

        return matchCarrier && matchService;
    }

    // Export to global namespace
    window.EasyPost = window.EasyPost || {};
    window.EasyPost.matchesPattern = matchesPattern;
})();
