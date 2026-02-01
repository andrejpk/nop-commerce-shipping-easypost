# EasyPost Batch API Usage Guide

This guide explains how to programmatically work with EasyPost batches in the nopCommerce plugin.

## Prerequisites

Inject the `EasyPostService` into your class:

```csharp
private readonly EasyPostService _easyPostService;

public YourClass(EasyPostService easyPostService)
{
    _easyPostService = easyPostService;
}
```

## 1. Check for an Open Batch

An "open" batch is one that hasn't been purchased yet and can still accept new shipments. These are batches with status: `Unknown`, `Creating`, `Created`, or `CreationFailed`.

```csharp
using Nop.Plugin.Shipping.EasyPost.Domain.Batch;

// Get all batches that can accept new shipments
var openBatches = await _easyPostService.GetAllBatchesAsync();

// Filter for batches that haven't been purchased
var availableBatch = openBatches
    .Where(b => b.StatusId == (int)BatchStatus.Unknown ||
                b.StatusId == (int)BatchStatus.Creating ||
                b.StatusId == (int)BatchStatus.Created ||
                b.StatusId == (int)BatchStatus.CreationFailed)
    .OrderByDescending(b => b.CreatedOnUtc)
    .FirstOrDefault();

if (availableBatch != null)
{
    // Found an open batch
    Console.WriteLine($"Open batch found: ID {availableBatch.Id}");
}
```

Or filter by specific status:

```csharp
// Get only "Created" batches
var createdBatches = await _easyPostService.GetAllBatchesAsync(
    status: BatchStatus.Created
);

var batch = createdBatches.FirstOrDefault();
```

## 2. Create a New Batch

```csharp
using Nop.Plugin.Shipping.EasyPost.Domain.Batch;

// Create a new batch entity
var newBatch = new EasyPostBatch
{
    StatusId = (int)BatchStatus.Unknown,
    ShipmentIds = string.Empty,  // Start with no shipments
    CreatedOnUtc = DateTime.UtcNow,
    UpdatedOnUtc = DateTime.UtcNow,
    BatchGuid = Guid.NewGuid()
};

// Insert the batch into the database
await _easyPostService.InsertBatchAsync(newBatch);

Console.WriteLine($"Created new batch with ID: {newBatch.Id}");
```

## 3. Add a Shipment to a Batch

Shipments are stored as a comma-separated list of nopCommerce shipment IDs in the `ShipmentIds` property.

```csharp
// Get the batch
var batch = await _easyPostService.GetBatchByIdAsync(batchId);
if (batch == null)
{
    throw new Exception("Batch not found");
}

// Parse existing shipment IDs
var shipmentIds = batch.ShipmentIds
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(id => int.Parse(id))
    .ToList();

// Add new shipment ID (nopCommerce shipment ID, not EasyPost shipment_id)
int nopCommerceShipmentId = 123; // Your shipment ID
if (!shipmentIds.Contains(nopCommerceShipmentId))
{
    shipmentIds.Add(nopCommerceShipmentId);
}

// Update the batch with new shipment list
batch.ShipmentIds = string.Join(',', shipmentIds.Distinct().OrderBy(id => id));
batch.UpdatedOnUtc = DateTime.UtcNow;

await _easyPostService.UpdateBatchAsync(batch);

Console.WriteLine($"Added shipment {nopCommerceShipmentId} to batch {batch.Id}");
```

## 4. Complete Workflow Example

Here's a complete example that finds or creates an open batch and adds a shipment:

```csharp
using Nop.Plugin.Shipping.EasyPost.Domain.Batch;

public async Task<int> AddShipmentToBatchAsync(int nopCommerceShipmentId)
{
    // Step 1: Look for an open batch
    var openBatches = await _easyPostService.GetAllBatchesAsync();
    var batch = openBatches
        .Where(b => b.StatusId == (int)BatchStatus.Unknown ||
                    b.StatusId == (int)BatchStatus.Created)
        .OrderByDescending(b => b.CreatedOnUtc)
        .FirstOrDefault();

    // Step 2: Create a new batch if none exists
    if (batch == null)
    {
        batch = new EasyPostBatch
        {
            StatusId = (int)BatchStatus.Unknown,
            ShipmentIds = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow,
            BatchGuid = Guid.NewGuid()
        };

        await _easyPostService.InsertBatchAsync(batch);
        Console.WriteLine($"Created new batch: {batch.Id}");
    }

    // Step 3: Add shipment to batch
    var shipmentIds = batch.ShipmentIds
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(id => int.Parse(id))
        .ToList();

    if (!shipmentIds.Contains(nopCommerceShipmentId))
    {
        shipmentIds.Add(nopCommerceShipmentId);
        batch.ShipmentIds = string.Join(',', shipmentIds.Distinct().OrderBy(id => id));
        batch.UpdatedOnUtc = DateTime.UtcNow;

        await _easyPostService.UpdateBatchAsync(batch);
        Console.WriteLine($"Added shipment {nopCommerceShipmentId} to batch {batch.Id}");
    }

    return batch.Id;
}
```

## 5. Sync Batch with EasyPost

After adding shipments, you need to create or update the batch with EasyPost's API:

```csharp
// This creates/updates the batch with EasyPost and creates shipments for any that don't exist
var (success, error) = await _easyPostService.CreateOrUpdateBatchAsync(batch);

if (!string.IsNullOrEmpty(error))
{
    Console.WriteLine($"Error syncing batch: {error}");
}
else
{
    Console.WriteLine("Batch synced successfully with EasyPost");
}
```

## Important Notes

### Shipment Requirements

Before adding a shipment to a batch, ensure:
1. The shipment exists in nopCommerce
2. The shipment has the EasyPost `shipment_id` stored as a generic attribute (see main README)
   - Attribute key: `"EasyPost.Shipment.Id"` (defined in `EasyPostDefaults.ShipmentIdAttribute`)

### Batch Status Workflow

- **Unknown**: Batch created locally, not yet synced with EasyPost
- **Creating**: Currently being created on EasyPost
- **Created**: Successfully created on EasyPost, can add more shipments
- **Purchasing**: Labels are being purchased
- **Purchased**: Labels purchased, batch is closed
- **LabelGenerating/LabelGenerated**: Batch labels are being/have been generated

Once a batch is **Purchased** or beyond, you cannot add more shipments to it.

### Additional Batch Operations

```csharp
// Generate labels for all shipments in batch
var (success, error) = await _easyPostService.GenerateBatchLabelAsync(batch, "PDF");

// Download batch label
var ((url, contentType), error) = await _easyPostService.DownloadBatchLabelAsync(batch);

// Generate manifest (scan form)
var (success, error) = await _easyPostService.GenerateBatchManifestAsync(batch);

// Get all shipments in batch with details
var (shipments, error) = await _easyPostService.GetBatchShipmentsAsync(batch);
```

## Reference

- Batch entity: `src/Plugins/Nop.Plugin.Shipping.EasyPost/Domain/Batch/EasyPostBatch.cs`
- Batch status enum: `src/Plugins/Nop.Plugin.Shipping.EasyPost/Domain/Batch/BatchStatus.cs`
- Service methods: `src/Plugins/Nop.Plugin.Shipping.EasyPost/Services/EasyPostService.cs`
- Controller example: `src/Plugins/Nop.Plugin.Shipping.EasyPost/Controllers/EasyPostController.cs` (see `SelectShipments` method at line 824)
