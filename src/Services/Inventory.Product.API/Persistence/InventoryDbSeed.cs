using Inventory.Product.API.Entities;
using Inventory.Product.API.Extentions;
using MongoDB.Driver;
using Shared.Enums.Inventory;

namespace Inventory.Product.API.Persistence;

public class InventoryDbSeed
{
   public async Task SeedDataAsync(IMongoClient mongoClient, DatabaseSettings settings)
   {
      var databaseName = settings.DatabaseName;
      var database = mongoClient.GetDatabase(databaseName);
      var inventoryCollection = database.GetCollection<InventoryEntry>("inventoryEntries");
      if (await inventoryCollection.EstimatedDocumentCountAsync() == 0)
      {
         await inventoryCollection.InsertManyAsync(GetPreconfiguredInventoryEntries());
      }
   }

   private IEnumerable<InventoryEntry> GetPreconfiguredInventoryEntries()
   {
      return new List<InventoryEntry>
      {
         new()
         {
            Quantity = 10,
            DocumentNo = Guid.NewGuid().ToString(),
            ItemNo = "Lotus",
            ExternalDocumentNo = Guid.NewGuid().ToString(),
            DocumentType = EDocumentType.Purchase
         },
         new()
         {
            Quantity = 10,
            DocumentNo = Guid.NewGuid().ToString(),
            ItemNo = "Cadillac",
            ExternalDocumentNo = Guid.NewGuid().ToString(),
            DocumentType = EDocumentType.Purchase
         }
      };
   }
}