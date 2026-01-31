namespace Nop.Plugin.Shipping.EasyPost.Domain.Configuration;

/// <summary>
/// Represents a conditional rule for hiding shipping services
/// Format: "Hide service A if service B exists"
/// Supports wildcards (*) in service matching
/// </summary>
public class ServiceDisplayRule
{
    /// <summary>
    /// Gets or sets the service pattern to hide (supports wildcards)
    /// Format: "Carrier:Service" (e.g., "USPS:Priority", "FedEx:*", "*:Ground")
    /// </summary>
    public string HideService { get; set; }

    /// <summary>
    /// Gets or sets the service pattern that triggers hiding (supports wildcards)
    /// Format: "Carrier:Service" (e.g., "USPS:GroundAdvantage", "UPS:*")
    /// If this service exists in the rate list, HideService will be hidden
    /// </summary>
    public string IfServiceExists { get; set; }

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
