/**
 * Service Discovery and UI Management for EasyPost Configuration
 * Handles service discovery, rendering, and user interactions
 */
(function() {
    'use strict';

    // Wait for DOM and config to be ready
    $(document).ready(function() {
        if (!window.EasyPostConfig) {
            console.error('EasyPostConfig not found');
            return;
        }

        const config = window.EasyPostConfig;
        let rawServices = [];
        let ruleManager = new window.EasyPost.RuleManager(
            config.serviceDisplayRules || [],
            config.discoveredServices || []
        );

        // Initialize
        loadTestScenario();
        renderRules();
        initializeEventHandlers();

        // ==================== Test Scenario Management ====================

        function loadTestScenario() {
            const scenario = JSON.parse(sessionStorage.getItem('easypost_test_scenario') || '{}');
            if (scenario.city) $('#test-city').val(scenario.city);
            if (scenario.state) $('#test-state').val(scenario.state);
            if (scenario.zip) $('#test-zip').val(scenario.zip);
            if (scenario.country) $('#test-country').val(scenario.country);
            if (scenario.weight) $('#test-weight').val(scenario.weight);
        }

        function saveTestScenario() {
            const scenario = {
                city: $('#test-city').val(),
                state: $('#test-state').val(),
                zip: $('#test-zip').val(),
                country: $('#test-country').val(),
                weight: $('#test-weight').val()
            };
            sessionStorage.setItem('easypost_test_scenario', JSON.stringify(scenario));
        }

        // ==================== Service Discovery ====================

        $('#btn-discover-services').click(function() {
            const btn = $(this);
            const city = $('#test-city').val();
            const state = $('#test-state').val();
            const zipCode = $('#test-zip').val();
            const countryId = $('#test-country').val();
            const weight = $('#test-weight').val() || '16';

            if (!city || !zipCode || !countryId) {
                showStatus('danger', 'Please fill in City, ZIP Code, and Country');
                return;
            }

            btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Discovering...');
            saveTestScenario();

            $.ajax({
                url: config.urls.discoverServices,
                type: 'POST',
                data: {
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                    city: city,
                    state: state,
                    zipCode: zipCode,
                    countryId: countryId,
                    weight: weight
                },
                success: function(response) {
                    if (response.success) {
                        rawServices = response.services;
                        ruleManager.discoveredServices = response.services.map(s => ({
                            carrier: s.carrier,
                            service: s.service,
                            displayName: s.displayName,
                            visible: true,
                            displayOrder: 0,
                            rate: s.rate
                        }));

                        showStatus('success', `Found ${rawServices.length} services`);
                        $('#services-placeholder').hide();
                        $('#services-panel').show();
                        renderServices();
                    } else {
                        showStatus('danger', response.message || 'Unknown error');
                    }
                },
                error: function(xhr, status, error) {
                    let errorMsg = 'Failed to discover services';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    } else if (xhr.responseText) {
                        errorMsg += ': ' + xhr.responseText.substring(0, 200);
                    }
                    showStatus('danger', errorMsg);
                },
                complete: function() {
                    btn.prop('disabled', false).html('<i class="fas fa-sync"></i> Discover Services');
                }
            });
        });

        function showStatus(type, message) {
            $('#discovery-status')
                .removeClass('alert-success alert-danger')
                .addClass('alert-' + type)
                .html(`<i class="fas fa-${type === 'success' ? 'check' : 'exclamation'}-circle"></i> ${message}`)
                .show();
        }

        // ==================== Service Rendering ====================

        function renderServices() {
            renderRawServices();
            renderFilteredServices();
            populateServiceDropdowns();
            renderRules();
        }

        function renderRawServices() {
            const html = rawServices.map(s =>
                `<div class="p-2 border-bottom d-flex justify-content-between align-items-center">
                    <span><strong>${s.carrier}</strong>: ${s.service}</span>
                    <span class="badge badge-info">$${s.rate ? s.rate.toFixed(2) : '0.00'}</span>
                </div>`
            ).join('');
            $('#raw-services-list').html(html);
        }

        function renderFilteredServices() {
            const { visible, hidden } = ruleManager.applyRules(rawServices);

            let html = visible.map(s =>
                `<div class="p-2 border-bottom d-flex justify-content-between align-items-center">
                    <span><strong>${s.carrier}</strong>: ${s.service}</span>
                    <span class="badge badge-success">$${s.rate ? s.rate.toFixed(2) : '0.00'}</span>
                </div>`
            ).join('');

            html += hidden.map(s =>
                `<div class="p-2 border-bottom">
                    <div class="d-flex justify-content-between align-items-center text-muted">
                        <span style="text-decoration: line-through;">
                            <strong>${s.carrier}</strong>: ${s.service}
                        </span>
                        <span class="badge badge-secondary" style="text-decoration: none;">$${s.rate ? s.rate.toFixed(2) : '0.00'}</span>
                    </div>
                    <small class="text-danger">
                        <i class="fas fa-ban"></i> ${s.hiddenReason}
                    </small>
                </div>`
            ).join('');

            $('#filtered-services-list').html(html || '<div class="text-muted text-center p-3">No services available</div>');
        }

        function populateServiceDropdowns() {
            const options = rawServices.map(s =>
                `<option value="${s.carrier}:${s.service}">${s.displayName}</option>`
            ).join('');

            $('#rule-hide-service, #rule-if-service, #priority-service-select, #price-priority-service-select')
                .html('<option value="">Select service...</option>' + options);
        }

        // ==================== Rule UI Management ====================

        function renderRules() {
            if (ruleManager.rules.length === 0) {
                $('#no-rules-message').show();
                $('#rules-list').hide();
                return;
            }

            $('#no-rules-message').hide();
            $('#rules-list').show();

            const rulesHtml = ruleManager.rules.map((rule, index) => {
                const ruleContent = getRuleContent(rule, index);
                return `
                    <div class="card mb-2" data-rule-index="${index}">
                        <div class="card-body p-2">
                            <div class="row align-items-center">
                                <div class="col-md-1 text-center">
                                    <button type="button" class="btn btn-sm btn-link rule-move-up" data-index="${index}" ${index === 0 ? 'disabled' : ''}>
                                        <i class="fas fa-arrow-up"></i>
                                    </button>
                                    <button type="button" class="btn btn-sm btn-link rule-move-down" data-index="${index}" ${index === ruleManager.rules.length - 1 ? 'disabled' : ''}>
                                        <i class="fas fa-arrow-down"></i>
                                    </button>
                                </div>
                                <div class="col-md-9">
                                    ${ruleContent}
                                </div>
                                <div class="col-md-2 text-right">
                                    <button type="button" class="btn btn-sm btn-danger rule-delete" data-index="${index}">
                                        <i class="fas fa-trash"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            }).join('');

            $('#rules-list').html(rulesHtml);
            attachRuleEventHandlers();
        }

        function getRuleContent(rule, index) {
            const isPriorityList = rule.ruleType === 'PriorityList' || rule.ruleType === 1;
            const isPricePriorityList = rule.ruleType === 'PricePriorityList' || rule.ruleType === 2;
            const isRemoveUnmatched = rule.ruleType === 'RemoveUnmatched' || rule.ruleType === 3;

            if (isPriorityList) {
                return `
                    <div class="form-check form-check-inline">
                        <input type="checkbox" class="form-check-input rule-enabled" data-index="${index}" ${rule.enabled ? 'checked' : ''}>
                    </div>
                    <strong>Priority List:</strong> Show first available from:
                    <ol class="mb-0 pl-4">
                        ${rule.priorityServices.map(s => `<li><code>${s}</code></li>`).join('')}
                    </ol>
                    ${rule.description ? `<small class="text-muted">${rule.description}</small>` : ''}
                `;
            } else if (isPricePriorityList) {
                return `
                    <div class="form-check form-check-inline">
                        <input type="checkbox" class="form-check-input rule-enabled" data-index="${index}" ${rule.enabled ? 'checked' : ''}>
                    </div>
                    <strong>Cheapest From:</strong>
                    <ul class="mb-0 pl-4">
                        ${rule.priorityServices.map(s => `<li><code>${s}</code></li>`).join('')}
                    </ul>
                    ${rule.description ? `<small class="text-muted">${rule.description}</small>` : ''}
                `;
            } else if (isRemoveUnmatched) {
                return `
                    <div class="form-check form-check-inline">
                        <input type="checkbox" class="form-check-input rule-enabled" data-index="${index}" ${rule.enabled ? 'checked' : ''}>
                    </div>
                    <strong>Remove Unmatched:</strong> Hide services not matched by previous rules
                    ${rule.description ? `<br><small class="text-muted">${rule.description}</small>` : ''}
                `;
            } else {
                const condition = rule.ifServiceExists
                    ? `if <code>${rule.ifServiceExists}</code> exists`
                    : `<strong>always</strong>`;
                return `
                    <div class="form-check form-check-inline">
                        <input type="checkbox" class="form-check-input rule-enabled" data-index="${index}" ${rule.enabled ? 'checked' : ''}>
                    </div>
                    Hide <code>${rule.hideService}</code> ${condition}
                    ${rule.description ? `<br><small class="text-muted">${rule.description}</small>` : ''}
                `;
            }
        }

        function attachRuleEventHandlers() {
            $('.rule-enabled').change(function() {
                const index = $(this).data('index');
                ruleManager.toggleEnabled(index);
                renderServices();
            });

            $('.rule-delete').click(function() {
                const index = $(this).data('index');
                ruleManager.deleteRule(index);
                renderServices();
            });

            $('.rule-move-up').click(function() {
                const index = parseInt($(this).data('index'));
                ruleManager.moveUp(index);
                renderServices();
            });

            $('.rule-move-down').click(function() {
                const index = parseInt($(this).data('index'));
                ruleManager.moveDown(index);
                renderServices();
            });
        }

        // ==================== Event Handlers ====================

        function initializeEventHandlers() {
            // Rule type toggle
            $('input[name="rule-type"]').change(function() {
                const value = $(this).val();
                $('#hide-rule-fields, #priority-rule-fields, #price-priority-rule-fields, #remove-unmatched-rule-fields').hide();
                $(`#${value}-rule-fields`).show();
            });

            // Populate text inputs from dropdowns
            $('#rule-hide-service').change(function() {
                const value = $(this).val();
                if (value) $('#rule-hide-pattern').val(value);
            });

            $('#rule-if-service').change(function() {
                const value = $(this).val();
                if (value) $('#rule-if-pattern').val(value);
            });

            // Add conditional hide rule
            $('#btn-add-rule').click(function() {
                const hideService = $('#rule-hide-pattern').val().trim() || $('#rule-hide-service').val();
                const ifService = $('#rule-if-pattern').val().trim() || $('#rule-if-service').val();
                const description = $('#rule-description').val().trim();

                if (!hideService) {
                    alert('Please select or enter a service to hide');
                    return;
                }

                ruleManager.addRule({
                    ruleType: 'ConditionalHide',
                    hideService: hideService,
                    ifServiceExists: ifService || null,
                    description: description
                });

                $('#rule-hide-service, #rule-if-service, #rule-hide-pattern, #rule-if-pattern, #rule-description').val('');
                renderServices();
            });

            // Priority list management
            setupPriorityListHandlers();

            // Price-priority list management
            setupPricePriorityListHandlers();

            // Remove unmatched rule
            $('#btn-add-remove-unmatched-rule').click(function() {
                const description = $('#rule-description').val().trim();
                ruleManager.addRule({
                    ruleType: 'RemoveUnmatched',
                    description: description
                });
                $('#rule-description').val('');
                renderServices();
            });

            // Form submission
            $('form').submit(function() {
                $('#discovered-services-json').val(JSON.stringify(ruleManager.discoveredServices));
                $('#service-rules-json').val(ruleManager.toJSON());
            });
        }

        function setupPriorityListHandlers() {
            let priorityServices = [];

            $('#btn-add-priority-service').click(function() {
                const service = $('#priority-service-select').val();
                if (!service || priorityServices.includes(service)) {
                    if (priorityServices.includes(service)) alert('Service already in list');
                    return;
                }
                priorityServices.push(service);
                renderPriorityList(priorityServices);
                $('#priority-service-select').val('');
            });

            $('#btn-add-priority-rule').click(function() {
                if (priorityServices.length < 2) {
                    alert('Please add at least 2 services to the priority list');
                    return;
                }

                ruleManager.addRule({
                    ruleType: 'PriorityList',
                    priorityServices: [...priorityServices],
                    description: $('#rule-description').val().trim()
                });

                priorityServices = [];
                renderPriorityList(priorityServices);
                $('#rule-description').val('');
                renderServices();
            });

            function renderPriorityList(services) {
                if (services.length === 0) {
                    $('#priority-services-list').html('<div class="text-muted text-center py-2">No services added yet</div>');
                    return;
                }

                const html = services.map((service, index) => `
                    <div class="d-flex align-items-center justify-content-between p-2 border-bottom">
                        <span><strong>${index + 1}.</strong> ${service}</span>
                        <div>
                            <button type="button" class="btn btn-sm btn-link priority-move-up" data-index="${index}" ${index === 0 ? 'disabled' : ''}>
                                <i class="fas fa-arrow-up"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-link priority-move-down" data-index="${index}" ${index === services.length - 1 ? 'disabled' : ''}>
                                <i class="fas fa-arrow-down"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-link text-danger priority-remove" data-index="${index}">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                    </div>
                `).join('');

                $('#priority-services-list').html(html);

                $('.priority-move-up').click(function() {
                    const index = parseInt($(this).data('index'));
                    if (index > 0) {
                        [priorityServices[index - 1], priorityServices[index]] =
                        [priorityServices[index], priorityServices[index - 1]];
                        renderPriorityList(priorityServices);
                    }
                });

                $('.priority-move-down').click(function() {
                    const index = parseInt($(this).data('index'));
                    if (index < priorityServices.length - 1) {
                        [priorityServices[index], priorityServices[index + 1]] =
                        [priorityServices[index + 1], priorityServices[index]];
                        renderPriorityList(priorityServices);
                    }
                });

                $('.priority-remove').click(function() {
                    const index = parseInt($(this).data('index'));
                    priorityServices.splice(index, 1);
                    renderPriorityList(priorityServices);
                });
            }
        }

        function setupPricePriorityListHandlers() {
            let pricePriorityServices = [];

            $('#btn-add-price-priority-service').click(function() {
                const service = $('#price-priority-service-select').val();
                if (!service || pricePriorityServices.includes(service)) {
                    if (pricePriorityServices.includes(service)) alert('Service already in group');
                    return;
                }
                pricePriorityServices.push(service);
                renderPricePriorityList(pricePriorityServices);
                $('#price-priority-service-select').val('');
            });

            $('#btn-add-price-priority-rule').click(function() {
                if (pricePriorityServices.length < 2) {
                    alert('Please add at least 2 services to the group');
                    return;
                }

                ruleManager.addRule({
                    ruleType: 'PricePriorityList',
                    priorityServices: [...pricePriorityServices],
                    description: $('#rule-description').val().trim()
                });

                pricePriorityServices = [];
                renderPricePriorityList(pricePriorityServices);
                $('#rule-description').val('');
                renderServices();
            });

            function renderPricePriorityList(services) {
                if (services.length === 0) {
                    $('#price-priority-services-list').html('<div class="text-muted text-center py-2">No services added yet</div>');
                    return;
                }

                const html = services.map((service, index) => `
                    <div class="d-flex align-items-center justify-content-between p-2 border-bottom">
                        <span>${service}</span>
                        <button type="button" class="btn btn-sm btn-link text-danger price-priority-remove" data-index="${index}">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                `).join('');

                $('#price-priority-services-list').html(html);

                $('.price-priority-remove').click(function() {
                    const index = parseInt($(this).data('index'));
                    pricePriorityServices.splice(index, 1);
                    renderPricePriorityList(pricePriorityServices);
                });
            }
        }
    });
})();
