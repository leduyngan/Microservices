using Saga.Orchestrator.HttpRepository.Interfaces;
using Shared.DTOs.Inventory;

namespace Saga.Orchestrator.HttpRepository;

public class InventoryHttpRepository : IInventoryHttpRepository
{
    private readonly HttpClient _client;

    public InventoryHttpRepository(HttpClient client)
    {
        _client = client;
    }


    public Task<string> CreateSalesOrder(SalesProductDto model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteOrderByDocumentNo(string documentNo)
    {
        throw new NotImplementedException();
    }
}