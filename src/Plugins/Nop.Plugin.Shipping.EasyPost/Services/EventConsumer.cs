using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Shipping.EasyPost.Services;

/// <summary>
/// Represents plugin event consumer
/// </summary>
public class EventConsumer :
    BaseAdminMenuCreatedEventConsumer,
    IConsumer<EntityDeletedEvent<Shipment>>,
    IConsumer<ModelReceivedEvent<BaseNopModel>>,
    IConsumer<OrderPlacedEvent>,
    IConsumer<ShipmentCreatedEvent>
{
    #region Fields

    private readonly EasyPostService _easyPostService;
    private readonly IAdminMenu _adminMenu;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IProductService _productService;
    private readonly IShippingPluginManager _shippingPluginManager;

    #endregion

    #region Ctor

    public EventConsumer(EasyPostService easyPostService,
        IAdminMenu adminMenu,
        IGenericAttributeService genericAttributeService,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IPluginManager<IPlugin> pluginManager,
        IProductService productService,
        IShippingPluginManager shippingPluginManager) : base(pluginManager)
    {
        _easyPostService = easyPostService;
        _adminMenu = adminMenu;
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _productService = productService;
        _shippingPluginManager = shippingPluginManager;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Checks is the current customer has rights to access this menu item
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the true if access is granted, otherwise false
    /// </returns>
    protected override async Task<bool> CheckAccessAsync()
    {
        return await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW);
    }

    /// <summary>
    /// Gets the menu item
    /// </summary>
    /// <param name="plugin">The instance of <see cref="IPlugin"/> interface</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the instance of <see cref="AdminMenuItem"/>
    /// </returns>
    protected override async Task<AdminMenuItem> GetAdminMenuItemAsync(IPlugin plugin)
    {
        return new AdminMenuItem
        {
            SystemName = "EasyPost Batches",
            Title = await _localizationService.GetResourceAsync("Plugins.Shipping.EasyPost.Batch"),
            Url = _adminMenu.GetMenuItemUrl("EasyPost", "BatchList"),
            IconClass = "far fa-dot-circle",
            Visible = true,
            PermissionNames = new List<string> { StandardPermission.Orders.ORDERS_VIEW }
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle entity deleted event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(EntityDeletedEvent<Shipment> eventMessage)
    {
        if (eventMessage.Entity is null)
            return;

        await _easyPostService.DeleteShipmentAsync(eventMessage.Entity);
    }

    /// <summary>
    /// Handle model received event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(ModelReceivedEvent<BaseNopModel> eventMessage)
    {
        if (eventMessage.Model is not ProductModel model)
            return;

        if (!await _shippingPluginManager.IsPluginActiveAsync(EasyPostDefaults.SystemName))
            return;

        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS))
            return;

        var product = await _productService.GetProductByIdAsync(model.Id);
        if (product is null)
            return;

        //try to get additional form values for the product
        var form = _httpContextAccessor.HttpContext.Request.Form;
        if (form.TryGetValue(EasyPostDefaults.ProductPredefinedPackageFormKey, out var predefinedPackageValue))
        {
            var predefinedPackage = !StringValues.IsNullOrEmpty(predefinedPackageValue) ? predefinedPackageValue.ToString() : null;
            await _genericAttributeService.SaveAttributeAsync(product, EasyPostDefaults.ProductPredefinedPackageAttribute, predefinedPackage);
        }
        if (form.TryGetValue(EasyPostDefaults.ProductHtsNumberFormKey, out var htsNumberValue))
        {
            var htsNumber = !StringValues.IsNullOrEmpty(htsNumberValue) ? htsNumberValue.ToString() : null;
            await _genericAttributeService.SaveAttributeAsync(product, EasyPostDefaults.ProductHtsNumberAttribute, htsNumber);
        }
        if (form.TryGetValue(EasyPostDefaults.ProductOriginCountryFormKey, out var originCountryValue))
        {
            var originCountry = !StringValues.IsNullOrEmpty(originCountryValue) ? originCountryValue.ToString() : null;
            await _genericAttributeService.SaveAttributeAsync(product, EasyPostDefaults.ProductOriginCountryAttribute, originCountry);
        }
    }

    /// <summary>
    /// Handle order placed event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        if (eventMessage.Order is null)
            return;

        if (!await _shippingPluginManager.IsPluginActiveAsync(EasyPostDefaults.SystemName))
            return;

        await _easyPostService.SaveShipmentAsync(eventMessage.Order);
    }

    /// <summary>
    /// Handle shipment created event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(ShipmentCreatedEvent eventMessage)
    {
        if (eventMessage.Shipment is null)
            return;

        if (!await _shippingPluginManager.IsPluginActiveAsync(EasyPostDefaults.SystemName))
            return;

        await _easyPostService.SaveShipmentAsync(eventMessage.Shipment);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    protected override string PluginSystemName => EasyPostDefaults.SystemName;

    /// <summary>
    /// Menu item insertion type
    /// </summary>
    protected override MenuItemInsertType InsertType => MenuItemInsertType.After;

    /// <summary>
    /// The system name of the menu item after with need to insert the current one
    /// </summary>
    protected override string AfterMenuSystemName => "Shipments";

    #endregion
}
