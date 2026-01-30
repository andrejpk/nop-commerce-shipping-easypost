#!/usr/bin/env dotnet-script

using System.IO.Compression;

if (Args.Count < 2)
{
    Console.WriteLine("Usage: CreateZip.csx <source-directory> <output-zip-file>");
    return 1;
}

var sourceDir = Args[0];
var outputZip = Args[1];

try
{
    // Delete existing ZIP if it exists
    if (File.Exists(outputZip))
    {
        File.Delete(outputZip);
    }

    // Create the ZIP file
    Console.WriteLine($"Creating ZIP: {outputZip}");
    Console.WriteLine($"From: {sourceDir}");

    ZipFile.CreateFromDirectory(
        sourceDir,
        outputZip,
        CompressionLevel.Optimal,
        includeBaseDirectory: false
    );

    var fileInfo = new FileInfo(outputZip);
    Console.WriteLine($"✅ ZIP created successfully: {fileInfo.Name} ({fileInfo.Length:N0} bytes)");

    // List contents
    using (var archive = ZipFile.OpenRead(outputZip))
    {
        Console.WriteLine($"📦 Package contains {archive.Entries.Count} files:");
        foreach (var entry in archive.Entries.Take(10))
        {
            Console.WriteLine($"   - {entry.FullName}");
        }
        if (archive.Entries.Count > 10)
        {
            Console.WriteLine($"   ... and {archive.Entries.Count - 10} more files");
        }
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"❌ Error creating ZIP: {ex.Message}");
    return 1;
}
