/**
 * Rule Manager for EasyPost service filtering
 * Handles rule CRUD operations, rendering, and application logic
 */
(function() {
    'use strict';

    class RuleManager {
        constructor(initialRules = [], initialServices = []) {
            this.rules = this.normalizeRules(initialRules);
            this.discoveredServices = initialServices;
            this.nextRulePriority = this.rules.length;
            this.currentPriorityServices = [];
            this.currentPricePriorityServices = [];
        }

        /**
         * Normalize rules from C# PascalCase to JavaScript camelCase
         */
        normalizeRules(rules) {
            return rules.map(rule => {
                let ruleType = rule.ruleType || rule.RuleType || 0;

                // Convert numeric enum to string if needed
                if (typeof ruleType === 'number') {
                    const types = ['ConditionalHide', 'PriorityList', 'PricePriorityList', 'RemoveUnmatched'];
                    ruleType = types[ruleType] || 'ConditionalHide';
                }

                return {
                    ruleType: ruleType,
                    hideService: rule.hideService || rule.HideService,
                    ifServiceExists: rule.ifServiceExists || rule.IfServiceExists,
                    priorityServices: rule.priorityServices || rule.PriorityServices || [],
                    description: rule.description || rule.Description,
                    enabled: rule.enabled !== undefined ? rule.enabled : (rule.Enabled !== undefined ? rule.Enabled : true),
                    priority: rule.priority !== undefined ? rule.priority : (rule.Priority || 0)
                };
            });
        }

        /**
         * Apply all rules to services and return visible/hidden lists
         */
        applyRules(services) {
            let result = services.map(s => ({...s, hiddenBy: null, hiddenReason: null, matchedByRule: false}));

            // Apply visibility from discovered services
            result.forEach(s => {
                const config = this.discoveredServices.find(d => d.carrier === s.carrier && d.service === s.service);
                if (config && !config.visible) {
                    s.hiddenBy = 'manual';
                    s.hiddenReason = 'Manually disabled';
                }
            });

            // Apply all rules (order matters!)
            const activeRules = this.rules.filter(r => r.enabled).sort((a, b) => a.priority - b.priority);

            for (const rule of activeRules) {
                this.applyRule(rule, result);
            }

            return {
                visible: result.filter(s => !s.hiddenBy),
                hidden: result.filter(s => s.hiddenBy)
            };
        }

        /**
         * Apply a single rule to the result set
         */
        applyRule(rule, result) {
            const isPriorityList = rule.ruleType === 'PriorityList' || rule.ruleType === 1;
            const isPricePriorityList = rule.ruleType === 'PricePriorityList' || rule.ruleType === 2;
            const isRemoveUnmatched = rule.ruleType === 'RemoveUnmatched' || rule.ruleType === 3;

            if (isPriorityList) {
                this.applyPriorityListRule(rule, result);
            } else if (isPricePriorityList) {
                this.applyPricePriorityListRule(rule, result);
            } else if (isRemoveUnmatched) {
                this.applyRemoveUnmatchedRule(rule, result);
            } else {
                this.applyConditionalHideRule(rule, result);
            }
        }

        applyPriorityListRule(rule, result) {
            let firstFound = false;
            for (const pattern of rule.priorityServices) {
                const matchingServices = result.filter(s => !s.hiddenBy && window.EasyPost.matchesPattern(s, pattern));

                if (matchingServices.length > 0 && !firstFound) {
                    matchingServices[0].matchedByRule = true;
                    firstFound = true;
                } else if (matchingServices.length > 0) {
                    matchingServices.forEach(s => {
                        s.matchedByRule = true;
                        s.hiddenBy = 'rule';
                        s.hiddenReason = `Hidden by priority rule${rule.description ? ` (${rule.description})` : ''}`;
                    });
                }
            }
        }

        applyPricePriorityListRule(rule, result) {
            let allMatchingServices = [];
            for (const pattern of rule.priorityServices) {
                const matchingServices = result.filter(s => !s.hiddenBy && window.EasyPost.matchesPattern(s, pattern));
                allMatchingServices.push(...matchingServices);
            }

            if (allMatchingServices.length > 0) {
                allMatchingServices.forEach(s => s.matchedByRule = true);

                if (allMatchingServices.length > 1) {
                    allMatchingServices.sort((a, b) => (a.rate || 0) - (b.rate || 0));
                    const cheapest = allMatchingServices[0];

                    allMatchingServices.slice(1).forEach(s => {
                        s.hiddenBy = 'rule';
                        s.hiddenReason = `Hidden - more expensive than ${cheapest.carrier} ${cheapest.service} ($${cheapest.rate.toFixed(2)})${rule.description ? ` (${rule.description})` : ''}`;
                    });
                }
            }
        }

        applyRemoveUnmatchedRule(rule, result) {
            result.forEach(s => {
                if (!s.hiddenBy && !s.matchedByRule) {
                    s.hiddenBy = 'rule';
                    s.hiddenReason = `Not matched by any previous rule${rule.description ? ` (${rule.description})` : ''}`;
                }
            });
        }

        applyConditionalHideRule(rule, result) {
            const conditionMet = !rule.ifServiceExists ||
                result.some(s => !s.hiddenBy && window.EasyPost.matchesPattern(s, rule.ifServiceExists));

            if (conditionMet) {
                result.forEach(s => {
                    if (!s.hiddenBy && window.EasyPost.matchesPattern(s, rule.hideService)) {
                        s.matchedByRule = true;
                        s.hiddenBy = 'rule';
                        s.hiddenReason = rule.ifServiceExists
                            ? `Hidden because ${rule.ifServiceExists} exists${rule.description ? ` (${rule.description})` : ''}`
                            : `Always hidden${rule.description ? ` (${rule.description})` : ''}`;
                    }
                });
            }
        }

        /**
         * Add a new rule
         */
        addRule(rule) {
            this.rules.push({
                ...rule,
                enabled: true,
                priority: this.nextRulePriority++
            });
        }

        /**
         * Delete a rule by index
         */
        deleteRule(index) {
            this.rules.splice(index, 1);
        }

        /**
         * Move a rule up in priority
         */
        moveUp(index) {
            if (index > 0) {
                [this.rules[index - 1], this.rules[index]] =
                [this.rules[index], this.rules[index - 1]];

                this.rules[index - 1].priority = index - 1;
                this.rules[index].priority = index;
            }
        }

        /**
         * Move a rule down in priority
         */
        moveDown(index) {
            if (index < this.rules.length - 1) {
                [this.rules[index], this.rules[index + 1]] =
                [this.rules[index + 1], this.rules[index]];

                this.rules[index].priority = index;
                this.rules[index + 1].priority = index + 1;
            }
        }

        /**
         * Toggle rule enabled status
         */
        toggleEnabled(index) {
            if (this.rules[index]) {
                this.rules[index].enabled = !this.rules[index].enabled;
            }
        }

        /**
         * Get all rules as JSON
         */
        toJSON() {
            return JSON.stringify(this.rules);
        }
    }

    // Export to global namespace
    window.EasyPost = window.EasyPost || {};
    window.EasyPost.RuleManager = RuleManager;
})();
