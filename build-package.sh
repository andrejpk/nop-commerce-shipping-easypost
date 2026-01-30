#!/bin/bash
set -e

echo "EasyPost Plugin Package Builder"
echo "================================"

# Check if nopCommerce is available
if [ ! -d "../nopCommerce/src" ]; then
    echo "Error: nopCommerce not found at ../nopCommerce/src"
    echo "Please clone nopCommerce to the parent directory:"
    echo "  cd .. && git clone https://github.com/nopSolutions/nopCommerce.git"
    exit 1
fi

# Clean previous build
echo "Cleaning previous build..."
rm -rf package
rm -f Nop.Plugin.Shipping.EasyPost.zip

# Build the plugin
echo "Building plugin..."
dotnet build src/Plugins/Nop.Plugin.Shipping.EasyPost/Nop.Plugin.Shipping.EasyPost.csproj --configuration Release

# Run tests
echo "Running tests..."
dotnet test src/Tests/Nop.Plugin.Shipping.EasyPost.Tests/Nop.Plugin.Shipping.EasyPost.Tests.csproj --configuration Release

# Create package directory
echo "Creating package..."
mkdir -p package/Shipping.EasyPost

# Copy plugin files from nopCommerce output
echo "Copying plugin files..."
cp ../nopCommerce/src/Presentation/Nop.Web/Plugins/Shipping.EasyPost/Nop.Plugin.Shipping.EasyPost.dll package/Shipping.EasyPost/
[ -f ../nopCommerce/src/Presentation/Nop.Web/Plugins/Shipping.EasyPost/Nop.Plugin.Shipping.EasyPost.pdb ] && \
    cp ../nopCommerce/src/Presentation/Nop.Web/Plugins/Shipping.EasyPost/Nop.Plugin.Shipping.EasyPost.pdb package/Shipping.EasyPost/

# Copy EasyPost dependency
echo "Copying dependencies..."
cp ../nopCommerce/src/Presentation/Nop.Web/Plugins/Shipping.EasyPost/EasyPost*.dll package/Shipping.EasyPost/ 2>/dev/null || true

# Copy views, plugin.json, and logo
echo "Copying views and resources..."
cp -r src/Plugins/Nop.Plugin.Shipping.EasyPost/Views package/Shipping.EasyPost/
cp src/Plugins/Nop.Plugin.Shipping.EasyPost/plugin.json package/Shipping.EasyPost/
[ -f src/Plugins/Nop.Plugin.Shipping.EasyPost/logo.png ] && \
    cp src/Plugins/Nop.Plugin.Shipping.EasyPost/logo.png package/Shipping.EasyPost/

# Create ZIP
echo "Creating ZIP file..."
cd package
zip -r ../Nop.Plugin.Shipping.EasyPost.zip Shipping.EasyPost/
cd ..

# Get version (compatible with both GNU and BSD grep)
VERSION=$(grep -o '"Version"[[:space:]]*:[[:space:]]*"[^"]*"' src/Plugins/Nop.Plugin.Shipping.EasyPost/plugin.json | sed 's/.*"\([^"]*\)".*/\1/')

echo ""
echo "✅ Package created successfully!"
echo "   File: Nop.Plugin.Shipping.EasyPost.zip"
echo "   Version: $VERSION"
echo ""
echo "To install:"
echo "  1. Go to Admin → Configuration → Local plugins"
echo "  2. Click 'Upload plugin or theme'"
echo "  3. Select Nop.Plugin.Shipping.EasyPost.zip"
echo "  4. Upload and restart the application"
