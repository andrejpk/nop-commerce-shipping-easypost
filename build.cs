#!/usr/bin/env dotnet-script
#r "nuget: System.IO.Compression, 4.3.0"

using System.IO.Compression;

// Configuration
var pluginName = "Shipping.EasyPost";
var pluginSystemName = "Nop.Plugin.Shipping.EasyPost";
var pluginProjectPath = Path.Combine("src", "Plugins", pluginSystemName);
var nopCommerceRoot = Environment.GetEnvironmentVariable("NopCommerceRoot")
    ?? Path.Combine("..", "nopCommerce", "src");
var pluginOutputPath = Path.Combine(nopCommerceRoot, "Presentation", "Nop.Web", "Plugins", pluginName);
var version = Environment.GetEnvironmentVariable("VERSION") ?? "0.0.0-dev";
var packageRootPath = "package";
var packageOutputPath = Path.Combine(packageRootPath, pluginSystemName);
var zipFileName = $"{pluginSystemName}_v{version}.zip";

Console.WriteLine("📦 EasyPost Plugin Packager");
Console.WriteLine("===========================\n");

// Check if nopCommerce build output exists
if (!Directory.Exists(pluginOutputPath))
{
    Console.Error.WriteLine($"❌ Error: Plugin build output not found at {pluginOutputPath}");
    Console.Error.WriteLine("Please build the plugin first with: dotnet build");
    Environment.Exit(1);
}

// Clean and create package directory
Console.WriteLine("📦 Creating package...");
if (Directory.Exists(packageRootPath))
{
    Directory.Delete(packageRootPath, true);
}
Directory.CreateDirectory(packageOutputPath);

// Copy plugin DLL and PDB
Console.WriteLine("   Copying binaries...");
CopyIfExists(Path.Combine(pluginOutputPath, "Nop.Plugin.Shipping.EasyPost.dll"), packageOutputPath);
CopyIfExists(Path.Combine(pluginOutputPath, "Nop.Plugin.Shipping.EasyPost.pdb"), packageOutputPath);

// Copy EasyPost dependencies
foreach (var file in Directory.GetFiles(pluginOutputPath, "EasyPost*.dll"))
{
    File.Copy(file, Path.Combine(packageOutputPath, Path.GetFileName(file)), true);
}

// Copy views
Console.WriteLine("   Copying views...");
var viewsSource = Path.Combine(pluginProjectPath, "Views");
var viewsTarget = Path.Combine(packageOutputPath, "Views");
if (Directory.Exists(viewsSource))
{
    CopyDirectory(viewsSource, viewsTarget);
}

// Copy wwwroot (JavaScript, CSS, images, etc.)
Console.WriteLine("   Copying wwwroot...");
var wwwrootSource = Path.Combine(pluginProjectPath, "wwwroot");
var wwwrootTarget = Path.Combine(packageOutputPath, "wwwroot");
if (Directory.Exists(wwwrootSource))
{
    CopyDirectory(wwwrootSource, wwwrootTarget);
}

// Copy metadata files
Console.WriteLine("   Copying metadata...");
var pluginJsonSource = Path.Combine(pluginProjectPath, "plugin.json");
var pluginJsonTarget = Path.Combine(packageOutputPath, "plugin.json");
var pluginJsonContent = File.ReadAllText(pluginJsonSource);
pluginJsonContent = System.Text.RegularExpressions.Regex.Replace(
    pluginJsonContent,
    @"""Version""\s*:\s*""0\.0\.0""",
    $@"""Version"": ""{version}""");
File.WriteAllText(pluginJsonTarget, pluginJsonContent);
CopyIfExists(Path.Combine(pluginProjectPath, "logo.png"), packageOutputPath);

// Create uploadedItems.json
Console.WriteLine("   Creating uploadedItems.json...");
var uploadedItemsPath = Path.Combine(packageRootPath, "uploadedItems.json");
var uploadedItems = $$"""
[
  {
    "Type": "Plugin",
    "SupportedVersion": "4.90",
    "DirectoryPath": "{{pluginSystemName}}/",
    "SystemName": "{{pluginName}}",
    "SourceDirectoryPath": "{{pluginSystemName}}/"
  }
]
""";
File.WriteAllText(uploadedItemsPath, uploadedItems);

// Create ZIP
Console.WriteLine("\n📦 Creating ZIP file...");
if (File.Exists(zipFileName))
{
    File.Delete(zipFileName);
}

ZipFile.CreateFromDirectory("package", zipFileName, CompressionLevel.Optimal, includeBaseDirectory: false);

var zipInfo = new FileInfo(zipFileName);
Console.WriteLine($"✅ Package created successfully!");
Console.WriteLine($"   File: {zipFileName}");
Console.WriteLine($"   Size: {zipInfo.Length:N0} bytes");
Console.WriteLine($"   Version: {version}");

Console.WriteLine("\nTo install:");
Console.WriteLine("  1. Go to Admin → Configuration → Local plugins");
Console.WriteLine("  2. Click 'Upload plugin or theme'");
Console.WriteLine($"  3. Select {zipFileName}");
Console.WriteLine("  4. Upload and restart the application");

// Helper functions
void CopyIfExists(string source, string destination)
{
    if (File.Exists(source))
    {
        var fileName = Path.GetFileName(source);
        var destPath = Directory.Exists(destination) ? Path.Combine(destination, fileName) : destination;
        File.Copy(source, destPath, true);
    }
}

void CopyDirectory(string sourceDir, string destDir)
{
    Directory.CreateDirectory(destDir);

    foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
    {
        var relativePath = Path.GetRelativePath(sourceDir, file);
        var destFile = Path.Combine(destDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
        File.Copy(file, destFile, true);
    }
}
