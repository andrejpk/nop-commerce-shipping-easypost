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

        const [carrierPatternRaw, servicePatternRaw] = pattern.split(':');
        if (!carrierPatternRaw || !servicePatternRaw) return false;

        // Normalize to lowercase for case-insensitive matching
        const carrierPattern = carrierPatternRaw.toLowerCase();
        const servicePattern = servicePatternRaw.toLowerCase();
        const carrier = (service.carrier || '').toLowerCase();
        const serviceName = (service.service || '').toLowerCase();

        const matchCarrier = carrierPattern === '*' ||
            (carrierPattern.endsWith('*') && carrier.startsWith(carrierPattern.slice(0, -1))) ||
            (carrierPattern.startsWith('*') && carrier.endsWith(carrierPattern.slice(1))) ||
            carrier === carrierPattern;

        const matchService = servicePattern === '*' ||
            (servicePattern.endsWith('*') && serviceName.startsWith(servicePattern.slice(0, -1))) ||
            (servicePattern.startsWith('*') && serviceName.endsWith(servicePattern.slice(1))) ||
            serviceName === servicePattern;

        return matchCarrier && matchService;
    }

    // Export to global namespace
    window.EasyPost = window.EasyPost || {};
    window.EasyPost.matchesPattern = matchesPattern;
})();
