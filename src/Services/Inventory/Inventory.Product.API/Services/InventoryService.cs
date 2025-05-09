using AutoMapper;
using Infrastructure.Common;
using Infrastructure.Common.Models;
using Infrastructure.Extensions;
using Inventory.Product.API.Entities;
using Inventory.Product.API.Extentions;
using Inventory.Product.API.Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Configurations;
using Shared.DTOs.Inventory;

namespace Inventory.Product.API.Services;

public class InventoryService : MongoDbRepository<InventoryEntry>, IInventoryService
{
    private readonly IMapper _mapper;
    
    public InventoryService(IMongoClient client, MongoDbSettings settingses, IMapper mapper) : base(client, settingses)
    {
        _mapper = mapper;
    }

    public async Task<IEnumerable<InventoryEntryDto>> GetAllByItemNoAsync(string itemNo)
    {
        var entities = await FindAll()
            .Find(x => x.ItemNo.Equals(itemNo))
            .ToListAsync();
        var result = _mapper.Map<IEnumerable<InventoryEntryDto>>(entities);
        
        return result;
    }

    public async Task<PagedList<InventoryEntryDto>> GetAllByItemNoPageingAsync(GetInventoryPagingQuery query)
    {
        var filterSearchTerm = Builders<InventoryEntry>.Filter.Empty;
        var filterItemNo = Builders<InventoryEntry>.Filter.Eq(x => x.ItemNo, query.ItemNo());
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            filterSearchTerm = Builders<InventoryEntry>.Filter.Eq(x => x.DocumentNo, query.SearchTerm);
        }
        
        var andFilter = filterItemNo & filterSearchTerm;
        var pagedList = await Collection.PagingtedListAsync(andFilter, query.PageIndex, query.PageSize);
        var items = _mapper.Map<IEnumerable<InventoryEntryDto>>(pagedList);
        var result = new PagedList<InventoryEntryDto>(items, pagedList.GetMetaData().TotalItems, query.PageIndex, query.PageSize);
        
        return result;
    }

    public async Task<InventoryEntryDto> GetByIdAsync(string id)
    {
        FilterDefinition<InventoryEntry> filter = Builders<InventoryEntry>.Filter.Eq(x => x.Id, id);
        var entity = await FindAll().Find(filter).FirstOrDefaultAsync();
        var result = _mapper.Map<InventoryEntryDto>(entity);
        
        return result;
    }

    public async Task<InventoryEntryDto> PurchaseItemAsync(string itemNo, PurchaseProductDto model)
    {
        var entity = new InventoryEntry(ObjectId.GenerateNewId().ToString())
        {
            ItemNo = itemNo,
            Quantity = model.Quantity,
            DocumentType = model.DocumentType,
        };
        await CreateAsync(entity);
        var result = _mapper.Map<InventoryEntryDto>(entity);
        
        return result;
    }

    public async Task<InventoryEntryDto> SalesItemAsync(string itemNo, SalesProductDto model)
    {
        var itemToAdd = new InventoryEntry(ObjectId.GenerateNewId().ToString())
        {
            ItemNo = itemNo,
            Quantity = model.Quantity * -1,
            DocumentType = model.DocumentType,
            ExternalDocumentNo = model.ExternalDocumentNo
        };
        
        await CreateAsync(itemToAdd);
        var result = _mapper.Map<InventoryEntryDto>(itemToAdd);
        
        return result;
    }

    public async Task<bool> DeleteByDocumentNoAsync(string documentNo)
    {
        FilterDefinition<InventoryEntry> filter = Builders<InventoryEntry>.Filter.Eq(x => x.DocumentNo, documentNo);
       var result = await Collection.DeleteOneAsync(filter);
       return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<string> SalesOrderAsync(SalesOrderDto model)
    {
        var documentNo = Guid.NewGuid().ToString();
        foreach (var saleItem in model.SaleItems)
        {
            var itemToAdd = new InventoryEntry(ObjectId.GenerateNewId().ToString())
            {
                DocumentNo = documentNo,
                ItemNo = saleItem.ItemNo,
                Quantity = saleItem.Quantity * -1,
                DocumentType = saleItem.DocumentType,
                ExternalDocumentNo = model.OrderNo
            };
            await CreateAsync(itemToAdd);
        }
        
        return documentNo;
    }
}