#!/usr/bin/env dotnet-script
#r "nuget: System.IO.Compression, 4.3.0"

using System.IO.Compression;

// Configuration
var pluginName = "Shipping.EasyPost";
var pluginProjectPath = Path.Combine("src", "Plugins", "Nop.Plugin.Shipping.EasyPost");
var nopCommerceRoot = Environment.GetEnvironmentVariable("NopCommerceRoot")
    ?? Path.Combine("..", "nopCommerce", "src");
var pluginOutputPath = Path.Combine(nopCommerceRoot, "Presentation", "Nop.Web", "Plugins", pluginName);
var packageOutputPath = Path.Combine("package", pluginName);
var zipFileName = "Nop.Plugin.Shipping.EasyPost.zip";

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
if (Directory.Exists(packageOutputPath))
{
    Directory.Delete(packageOutputPath, true);
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

// Copy metadata files
Console.WriteLine("   Copying metadata...");
File.Copy(Path.Combine(pluginProjectPath, "plugin.json"), Path.Combine(packageOutputPath, "plugin.json"), true);
CopyIfExists(Path.Combine(pluginProjectPath, "logo.png"), packageOutputPath);

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

// Display version from plugin.json
try
{
    var pluginJson = File.ReadAllText(Path.Combine(pluginProjectPath, "plugin.json"));
    var versionMatch = System.Text.RegularExpressions.Regex.Match(pluginJson, @"""Version""\s*:\s*""([^""]+)""");
    if (versionMatch.Success)
    {
        Console.WriteLine($"   Version: {versionMatch.Groups[1].Value}");
    }
}
catch { }

Console.WriteLine("\nTo install:");
Console.WriteLine("  1. Go to Admin → Configuration → Local plugins");
Console.WriteLine("  2. Click 'Upload plugin or theme'");
Console.WriteLine("  3. Select Nop.Plugin.Shipping.EasyPost.zip");
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
