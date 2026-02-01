# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a nopCommerce 4.90 shipping plugin that integrates with EasyPost's multi-carrier shipping API. The plugin provides real-time shipping rate calculation, label generation, batch shipment processing, pickup scheduling, and international shipping support.

**Related Project**: This repository follows similar patterns to [api-for-nopcommerce](https://github.com/andrejpk/api-for-nopcommerce) for build system, CI/CD, and project structure. Refer to that project for additional examples of common patterns.

## Build System & Dependencies

### nopCommerce Dependency Architecture

This plugin has an unusual build setup that requires the nopCommerce source code to be present:

1. The plugin references nopCommerce projects directly (not NuGet packages)
2. Build output goes to: `$(NopCommerceRoot)/Presentation/Nop.Web/Plugins/Shipping.EasyPost/`
3. Default `NopCommerceRoot` is `../../../../nopCommerce/src` (4 levels up)
4. Can be overridden with environment variable: `export NopCommerceRoot=/path/to/nopCommerce/src`

### Building the Plugin

**Prerequisites:**
```bash
# Clone nopCommerce (required for building)
cd ..
git clone https://github.com/nopSolutions/nopCommerce.git
cd nop-commerce-shipping-easypost
```

**Build commands:**
```bash
# Build plugin
dotnet build src/Plugins/Nop.Plugin.Shipping.EasyPost/Nop.Plugin.Shipping.EasyPost.csproj

# Run tests
dotnet test src/Tests/Nop.Plugin.Shipping.EasyPost.Tests/Nop.Plugin.Shipping.EasyPost.Tests.csproj

# Package for distribution
VERSION=1.0.0 dotnet script build.cs
```

### Packaging System

The `build.cs` script (dotnet-script) creates a nopCommerce-compatible ZIP package:
- Copies built DLLs from nopCommerce output directory
- Includes Views, wwwroot, plugin.json, logo.png
- Injects version into plugin.json
- Creates uploadedItems.json for nopCommerce installer
- Output: `Nop.Plugin.Shipping.EasyPost_v{VERSION}.zip`

## Architecture

### Core Components

**EasyPostProvider** (`EasyPostProvider.cs`): Main plugin entry point
- Implements `IShippingRateComputationMethod` for rate calculation
- Implements `IWidgetPlugin` for admin UI customization
- Handles plugin lifecycle (install, uninstall, configuration)

**EasyPostService** (`Services/EasyPostService.cs`): Central business logic
- Wraps EasyPost SDK API calls
- Handles address validation, rate retrieval, label generation
- Manages batch shipment operations
- Converts between nopCommerce and EasyPost domain models

**EasyPostTracker** (`Services/EasyPostTracker.cs`): Shipment tracking integration
- Implements `IShipmentTracker` for nopCommerce tracking integration

**EventConsumer** (`Services/EventConsumer.cs`): Event-driven operations
- Listens to nopCommerce domain events
- Handles automatic address validation, tracking updates, etc.

### Domain Models

**Configuration:**
- `CarrierServiceConfig`: Defines which carrier services are available
- `ServiceDisplayRule`: Rules for showing/hiding shipping options based on criteria

**Batch Processing:**
- `EasyPostBatch`: Batch entity with status tracking
- `BatchShipment`: Links nopCommerce shipments to EasyPost batches
- `BatchStatus`: Enum for batch lifecycle states

**Shipment:**
- `ShippingRate`: Rate information from EasyPost
- `CustomsInfo`: International shipping customs data
- `Options`: EasyPost shipment options (signature, insurance, etc.)
- `CreatePickupRequest`: Pickup scheduling data

### Controllers

**EasyPostController** (admin): Configuration, batch management, label generation
**EasyPostPublicController** (storefront): Public-facing operations if needed

### View Components

Used to inject plugin UI into nopCommerce admin pages:
- `ShipmentDetailsViewComponent`: Adds EasyPost controls to shipment details
- `ProductDetailsViewComponent`: Adds EasyPost fields to product edit page
- `ShippingMethodsViewComponent`: Customizes shipping method selection

### Frontend JavaScript

Located in `wwwroot/js/`:
- `service-discovery.js`: Auto-discovers available carrier services from EasyPost API
- `rule-manager.js`: Manages service display rules in admin UI
- `pattern-matcher.js`: Pattern matching for service filtering
- `utils.js`: Common utilities

## Testing

Uses NUnit with Moq for mocking. Test project: `src/Tests/Nop.Plugin.Shipping.EasyPost.Tests/`

```bash
# Run all tests
dotnet test src/Tests/Nop.Plugin.Shipping.EasyPost.Tests/Nop.Plugin.Shipping.EasyPost.Tests.csproj

# Run with verbose output
dotnet test src/Tests/Nop.Plugin.Shipping.EasyPost.Tests/Nop.Plugin.Shipping.EasyPost.Tests.csproj --verbosity detailed
```

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/build.yml`) has a two-job design:

1. **Build job**: Builds plugin and runs tests, uploads artifact
2. **Release job**: Downloads artifact, runs semantic-release to create GitHub releases

**Semantic Release**: Configured in `.releaserc.json`
- Uses conventional commits to determine version bumps
- Runs `build.cs` script during prepare phase
- Attaches ZIP to GitHub release

**Triggering releases:**
- Automatic: Push to main with conventional commits (e.g., `feat:`, `fix:`)
- Manual: Actions → Build and Package Plugin → Run workflow

## nopCommerce Plugin Structure

**plugin.json**: Metadata for nopCommerce plugin system
- SystemName: `Shipping.EasyPost`
- Group: `Shipping rate computation`
- SupportedVersions: `["4.90"]`
- Version gets injected by build script

**Installation folder structure:**
```
Plugins/Shipping.EasyPost/
├── Nop.Plugin.Shipping.EasyPost.dll
├── EasyPost*.dll (dependencies)
├── plugin.json
├── logo.png
├── Views/ (Razor views)
└── wwwroot/ (JS, CSS, images)
```

## Key Development Patterns

1. **Dependency Injection**: Use constructor injection for services
2. **Localization**: Use `ILocalizationService` for all user-facing strings
3. **Database migrations**: FluentMigrator in `Data/` folder
4. **Settings**: Store configuration in `EasyPostSettings` with `ISettingService`
5. **Logging**: Use `ILogger` for debug/error logging

## Common Development Tasks

When adding new carrier service features, update:
1. Domain models in `Domain/`
2. Service methods in `Services/EasyPostService.cs`
3. Controller actions in `Controllers/`
4. View models in `Models/`
5. Razor views in `Views/`

When modifying batch processing logic, see `BATCH_API_USAGE.md` for API patterns.
