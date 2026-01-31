namespace Nop.Plugin.Shipping.EasyPost.Domain.Configuration;

/// <summary>
/// Represents a discovered carrier-service combination from EasyPost
/// </summary>
public class CarrierServiceConfig
{
    /// <summary>
    /// Gets or sets the carrier name (e.g., "USPS", "FedEx", "UPS")
    /// </summary>
    public string Carrier { get; set; }

    /// <summary>
    /// Gets or sets the service level (e.g., "Priority", "Ground", "Express")
    /// </summary>
    public string Service { get; set; }

    /// <summary>
    /// Gets or sets a friendly display name for this service (optional)
    /// If not set, defaults to "Carrier Service"
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets whether this service is visible to customers
    /// Default is true (visible)
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the display order for this service
    /// </summary>
    public int DisplayOrder { get; set; }
}
