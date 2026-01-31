using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Shipping.EasyPost.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.EasyPost.Models.Configuration
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor

        public ConfigurationModel()
        {
            CarrierAccounts = new List<string>();
            AvailableCarrierAccounts = new List<SelectListItem>();
            DiscoveredServices = new List<Domain.Configuration.CarrierServiceConfig>();
            ServiceDisplayRules = new List<Domain.Configuration.ServiceDisplayRule>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.ApiKey")]
        [DataType(DataType.Password)]
        public string ApiKey { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.TestApiKey")]
        [DataType(DataType.Password)]
        public string TestApiKey { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.UseAllAvailableCarriers")]
        public bool UseAllAvailableCarriers { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.CarrierAccounts")]
        public IList<SelectListItem> AvailableCarrierAccounts { get; set; }
        public IList<string> CarrierAccounts { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.AddressVerification")]
        public bool AddressVerification { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.StrictAddressVerification")]
        public bool StrictAddressVerification { get; set; }

        // Service configuration properties
        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.TestCity")]
        public string TestCity { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.TestStateProvinceId")]
        public string TestStateProvinceId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.TestZipCode")]
        public string TestZipCode { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.EasyPost.Configuration.Fields.TestCountryId")]
        public string TestCountryId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public IList<Domain.Configuration.CarrierServiceConfig> DiscoveredServices { get; set; }
        public IList<Domain.Configuration.ServiceDisplayRule> ServiceDisplayRules { get; set; }

        #endregion
    }
}