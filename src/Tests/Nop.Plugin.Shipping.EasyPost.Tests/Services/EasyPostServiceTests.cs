using FluentAssertions;
using Moq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Shipping.EasyPost.Domain.Batch;
using Nop.Plugin.Shipping.EasyPost.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Nop.Plugin.Shipping.EasyPost.Tests.Services;

[TestFixture]
public class EasyPostServiceTests
{
    private Mock<IActionContextAccessor> _actionContextAccessor;
    private Mock<IAddressService> _addressService;
    private Mock<ICountryService> _countryService;
    private Mock<ICurrencyService> _currencyService;
    private Mock<ICustomerService> _customerService;
    private Mock<IGenericAttributeService> _genericAttributeService;
    private Mock<ILocalizationService> _localizationService;
    private Mock<ILogger> _logger;
    private Mock<IMeasureService> _measureService;
    private Mock<INotificationService> _notificationService;
    private Mock<IOrderService> _orderService;
    private Mock<IProductService> _productService;
    private Mock<IRepository<EasyPostBatch>> _batchRepository;
    private Mock<IShipmentService> _shipmentService;
    private Mock<IShippingService> _shippingService;
    private Mock<IStateProvinceService> _stateProvinceService;
    private Mock<IStoreContext> _storeContext;
    private Mock<IStoreService> _storeService;
    private Mock<IUrlHelperFactory> _urlHelperFactory;
    private Mock<IWarehouseService> _warehouseService;
    private Mock<IWorkContext> _workContext;
    private CurrencySettings _currencySettings;
    private EasyPostSettings _easyPostSettings;
    private MeasureSettings _measureSettings;
    private ShippingSettings _shippingSettings;
    private EasyPostService _easyPostService;

    [SetUp]
    public void Setup()
    {
        // Initialize mocks
        _actionContextAccessor = new Mock<IActionContextAccessor>();
        _addressService = new Mock<IAddressService>();
        _countryService = new Mock<ICountryService>();
        _currencyService = new Mock<ICurrencyService>();
        _customerService = new Mock<ICustomerService>();
        _genericAttributeService = new Mock<IGenericAttributeService>();
        _localizationService = new Mock<ILocalizationService>();
        _logger = new Mock<ILogger>();
        _measureService = new Mock<IMeasureService>();
        _notificationService = new Mock<INotificationService>();
        _orderService = new Mock<IOrderService>();
        _productService = new Mock<IProductService>();
        _batchRepository = new Mock<IRepository<EasyPostBatch>>();
        _shipmentService = new Mock<IShipmentService>();
        _shippingService = new Mock<IShippingService>();
        _stateProvinceService = new Mock<IStateProvinceService>();
        _storeContext = new Mock<IStoreContext>();
        _storeService = new Mock<IStoreService>();
        _urlHelperFactory = new Mock<IUrlHelperFactory>();
        _warehouseService = new Mock<IWarehouseService>();
        _workContext = new Mock<IWorkContext>();

        // Initialize settings
        _currencySettings = new CurrencySettings();
        _easyPostSettings = new EasyPostSettings();
        _measureSettings = new MeasureSettings();
        _shippingSettings = new ShippingSettings();

        // Create service instance
        _easyPostService = new EasyPostService(
            _currencySettings,
            _easyPostSettings,
            _actionContextAccessor.Object,
            _addressService.Object,
            _countryService.Object,
            _currencyService.Object,
            _customerService.Object,
            _genericAttributeService.Object,
            _localizationService.Object,
            _logger.Object,
            _measureService.Object,
            _notificationService.Object,
            _orderService.Object,
            _productService.Object,
            _batchRepository.Object,
            _shipmentService.Object,
            _shippingService.Object,
            _stateProvinceService.Object,
            _storeContext.Object,
            _storeService.Object,
            _urlHelperFactory.Object,
            _warehouseService.Object,
            _workContext.Object,
            _measureSettings,
            _shippingSettings
        );
    }

    [Test]
    public void Constructor_ShouldInitializeService()
    {
        // Assert - Service should be created without errors
        _easyPostService.Should().NotBeNull();
    }

    [Test]
    public async Task GetAllBatchesAsync_ShouldReturnEmptyList_WhenNoBatchesExist()
    {
        // Arrange
        _batchRepository
            .Setup(x => x.GetAllAsync(
                It.IsAny<Func<IQueryable<EasyPostBatch>, IQueryable<EasyPostBatch>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Func<IQueryable<EasyPostBatch>, IQueryable<EasyPostBatch>> func,
                Func<ICacheKeyService, CacheKey> cacheKey,
                bool includeDeleted) => new List<EasyPostBatch>());

        // Act
        var result = await _easyPostService.GetAllBatchesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
