using System.Collections.Generic;

namespace Nop.Plugin.Shipping.EasyPost.Domain.Configuration;

/// <summary>
/// Type of display rule
/// </summary>
public enum RuleType
{
    /// <summary>
    /// Hide a service conditionally or unconditionally
    /// </summary>
    ConditionalHide,

    /// <summary>
    /// Show only the first available service from a priority list
    /// </summary>
    PriorityList,

    /// <summary>
    /// Show only the cheapest service from a group
    /// </summary>
    PricePriorityList,

    /// <summary>
    /// Remove any services not matched by previous rules
    /// </summary>
    RemoveUnmatched
}

/// <summary>
/// Represents a rule for controlling which shipping services are displayed
/// Supports conditional hiding and priority-based filtering
/// </summary>
public class ServiceDisplayRule
{
    /// <summary>
    /// Gets or sets the type of rule
    /// </summary>
    public RuleType RuleType { get; set; } = RuleType.ConditionalHide;

    /// <summary>
    /// Gets or sets the service pattern to hide (supports wildcards)
    /// Format: "Carrier:Service" (e.g., "USPS:Priority", "FedEx:*", "*:Ground")
    /// Used only for ConditionalHide rules
    /// </summary>
    public string HideService { get; set; }

    /// <summary>
    /// Gets or sets the service pattern that triggers hiding (supports wildcards)
    /// Format: "Carrier:Service" (e.g., "USPS:GroundAdvantage", "UPS:*")
    /// If this service exists in the rate list, HideService will be hidden
    /// If null/empty, the service is always hidden (unconditional)
    /// Used only for ConditionalHide rules
    /// </summary>
    public string IfServiceExists { get; set; }

    /// <summary>
    /// Gets or sets the list of services in priority order
    /// Only the first available service from this list will be shown
    /// Format: List of "Carrier:Service" patterns
    /// Used only for PriorityList rules
    /// </summary>
    public List<string> PriorityServices { get; set; } = new();

    /// <summary>
    /// Gets or sets a description of this rule for admin reference
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets whether this rule is active
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the priority of this rule (lower number = higher priority)
    /// </summary>
    public int Priority { get; set; }
}
