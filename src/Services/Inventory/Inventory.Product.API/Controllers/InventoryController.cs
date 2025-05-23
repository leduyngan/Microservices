using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Infrastructure.Common.Models;
using Inventory.Product.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Inventory;

namespace Inventory.Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// api/inventory/items/{itemNo}
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("items/{itemNo}", Name = "GetAllByItemNo")]
    [ProducesResponseType(typeof(IEnumerable<InventoryEntryDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<InventoryEntryDto>>> GetAllByItemNo([Required] string itemNo)
    {
        var result = await _inventoryService.GetAllByItemNoAsync(itemNo);
        return Ok(result);
    }

    /// <summary>
    /// api/inventory/items/{itemNo}/paging
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("items/{itemNo}/paging", Name = "GetAllByItemNoPaging")]
    [ProducesResponseType(typeof(PagedList<InventoryEntryDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PagedList<InventoryEntryDto>>> GetAllByItemNoPaging([Required] string itemNo,
        [FromQuery] GetInventoryPagingQuery query)
    {
        query.SetItemNo(itemNo);
        var result = await _inventoryService.GetAllByItemNoPageingAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// api/inventory/{id}
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{id}", Name = "GetInventoryById")]
    [ProducesResponseType(typeof(InventoryEntryDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<InventoryEntryDto>> GetInventoryById([Required] string id)
    {
        var result = await _inventoryService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// api/inventory/purchase/{itemNo}
    /// </summary>
    /// <returns></returns>
    [HttpPost("purchase/{itemNo}", Name = "PurchaseOrder")]
    [ProducesResponseType(typeof(InventoryEntryDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<InventoryEntryDto>> PurchaseOrder([Required] string itemNo,
        [FromBody] PurchaseProductDto model)
    {
        var result = await _inventoryService.PurchaseItemAsync(itemNo, model);
        return Ok(result);
    }

    [HttpPost("sales/{itemNo}", Name = "SalesItem")]
    [ProducesResponseType(typeof(InventoryEntryDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<InventoryEntryDto>> SalesItem([Required] string itemNo,
        [FromBody] SalesProductDto model)
    {
        var result = await _inventoryService.SalesItemAsync(itemNo, model);
        return Ok(result);
    }
    
    [HttpPost("sales/order-no/{orderNo}", Name = "SalesOrder")]
    [ProducesResponseType(typeof(InventoryEntryDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CreatedSalesOrderSuccessDto>> SalesOrder([Required] string orderNo,
        [FromBody] SalesOrderDto model)
    {
        model.OrderNo = orderNo;
        var documentNo = await _inventoryService.SalesOrderAsync(model);
        var result = new CreatedSalesOrderSuccessDto(documentNo);
        return Ok(result);
    }

    /// <summary>
    /// api/inventory/{id}
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}", Name = "DeleteById")]
    [ProducesResponseType(typeof(InventoryEntryDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<InventoryEntryDto>> DeleteById([Required] string id)
    {
        var entity = await _inventoryService.GetByIdAsync(id);
        if (entity == null) return NotFound();
        await _inventoryService.DeleteAsync(id);
        return NoContent();
    }

    [HttpDelete("document-no/{documentNo}", Name = "DeleteByDocumentNo")]
    [ProducesResponseType(typeof(InventoryEntryDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> DeleteByDocumentNo([Required] string documentNo)
    {
        await _inventoryService.DeleteByDocumentNoAsync(documentNo);
        return NoContent();
    }
}