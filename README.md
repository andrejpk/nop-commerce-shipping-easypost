# EasyPost Shipping Plugin for nopCommerce 4.90

A shipping rate computation plugin for nopCommerce that integrates with EasyPost's shipping API, supporting multiple carriers and advanced shipping features.

## Features

- Multi-carrier shipping rate calculation
- Real-time shipping quotes
- Label generation and printing
- Batch shipment processing
- Pickup scheduling
- Commercial invoice generation
- Tracking integration
- Support for international shipping with customs forms

## Requirements

- nopCommerce 4.90
- .NET 9.0
- EasyPost API account ([Sign up here](https://www.easypost.com/))

## Installation

### Option 1: Upload through nopCommerce Admin (Recommended)

1. Download the latest `Nop.Plugin.Shipping.EasyPost.zip` from the [Releases page](../../releases) or [Actions artifacts](../../actions)
2. Go to **Admin → Configuration → Local plugins**
3. Click **Upload plugin or theme**
4. Select the downloaded ZIP file
5. Click **Upload plugin or theme** button
6. **Restart the application** when prompted
7. Click **Install** next to the EasyPost Shipping plugin
8. Configure the plugin with your EasyPost API keys

### Option 2: Manual Installation

1. Extract the ZIP file
2. Copy the `Shipping.EasyPost` folder to `[nopCommerce root]/src/Presentation/Nop.Web/Plugins/`
3. Restart the application
4. Go to **Admin → Configuration → Local plugins**
5. Click **Install** next to the EasyPost Shipping plugin

## Configuration

After installation:

1. Go to **Admin → Configuration → Shipping → Shipping providers**
2. Find **EasyPost** and click **Configure**
3. Enter your EasyPost API credentials:
   - **API Key**: Your EasyPost production API key
   - **Test API Key**: Your EasyPost test/sandbox API key
   - **Use Sandbox**: Toggle for testing mode
4. Configure additional settings as needed
5. Click **Save**

## Development

### Building from Source

```bash
# Clone this repository
git clone https://github.com/your-username/stripe-nop-commerce.git
cd stripe-nop-commerce

# Clone nopCommerce (required for building)
cd ..
git clone https://github.com/nopSolutions/nopCommerce.git

# Build the plugin
cd stripe-nop-commerce
dotnet build src/Plugins/Nop.Plugin.Shipping.EasyPost/Nop.Plugin.Shipping.EasyPost.csproj

# Run tests
dotnet test src/Tests/Nop.Plugin.Shipping.EasyPost.Tests/Nop.Plugin.Shipping.EasyPost.Tests.csproj
```

### Project Structure

```
├── src/
│   ├── Plugins/
│   │   └── Nop.Plugin.Shipping.EasyPost/    # Plugin source code
│   │       ├── Components/                    # View components
│   │       ├── Controllers/                   # Admin controllers
│   │       ├── Domain/                        # Domain models
│   │       ├── Factories/                     # Model factories
│   │       ├── Models/                        # View models
│   │       ├── Services/                      # Business logic
│   │       ├── Views/                         # Razor views
│   │       └── plugin.json                    # Plugin metadata
│   └── Tests/
│       └── Nop.Plugin.Shipping.EasyPost.Tests/  # Unit tests
└── .github/
    └── workflows/
        └── build.yml                          # CI/CD pipeline
```

### CI/CD Pipeline

The project includes a GitHub Actions workflow that:

1. **Builds** the plugin against the latest nopCommerce master branch
2. **Runs tests** to ensure code quality
3. **Creates a ZIP package** ready for nopCommerce installation
4. **Uploads artifacts** for each build (available for 30 days)
5. **Creates releases** automatically when you push a version tag

#### Triggering a Build

- **Automatic**: Every push to `main` or pull request
- **Manual**: Go to Actions → Build and Package Plugin → Run workflow

#### Creating a Release

```bash
# Tag a version (will trigger release creation)
git tag v1.33.0
git push origin v1.33.0
```

The workflow will automatically create a GitHub release with the packaged plugin.

## Migration from v4.60

This plugin has been migrated from nopCommerce 4.60 to 4.90 with the following major changes:

- **EasyPost SDK**: Upgraded from v4.0.2 to v7.4.0
- **.NET Version**: Upgraded from .NET 7.0 to .NET 9.0
- **API Changes**: Updated for nopCommerce 4.90 API (permissions, menu system, etc.)
- **Breaking Changes**: CarbonNeutral option removed (all shipments are carbon neutral by default in EasyPost v7)

## Testing

The project includes unit tests with full mocking:

```bash
dotnet test src/Tests/Nop.Plugin.Shipping.EasyPost.Tests/Nop.Plugin.Shipping.EasyPost.Tests.csproj
```

Tests use:
- **NUnit** for test framework
- **Moq** for mocking dependencies
- **FluentAssertions** for readable assertions

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This plugin is provided as-is for use with nopCommerce. See nopCommerce license terms at https://www.nopcommerce.com/license

## Support

- **EasyPost API Documentation**: https://docs.easypost.com/
- **nopCommerce Documentation**: https://docs.nopcommerce.com/
- **Issues**: Report issues on the [GitHub Issues page](../../issues)

## Changelog

### v1.33 (2024)
- Migrated to nopCommerce 4.90
- Upgraded to EasyPost SDK v7.4.0
- Updated to .NET 9.0
- Added comprehensive test coverage
- Added CI/CD pipeline for automated builds and releases
