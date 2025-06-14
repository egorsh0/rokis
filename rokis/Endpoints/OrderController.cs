using System.Security.Claims;
using rokis.Dtos;
using rokis.Extensions;
using rokis.Infrastructures;
using rokis.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace rokis.Endpoints;

[ApiController]
[Route("order")]
[Authorize(Roles = "Company,Person")]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    public OrderController(IOrderRepository orderRepository) => _orderRepository = orderRepository;

    // ═══════════════════════════════════════════════════════
    // POST /purchase
    // ═══════════════════════════════════════════════════════
    /// <summary>Создаёт заказ на токены.</summary>
    /// <response code="200">
    /// Объект заказа в статусе <c>Unpaid</c>.  
    /// <example>
    /// {
    ///   "id": 42,
    ///   "status": "Unpaid",
    ///   "quantity": 10,
    ///   "tokens": [ { "id":"…", "status":"Pending" } ]
    /// }
    /// </example>
    /// </response>
    /// <response code="400">Список позиций пуст.</response>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),   StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Purchase([FromBody] CreateOrderDto dto)
    {
        if (dto.Items.Count == 0)
        {
            return BadRequest(new ResponseDto(MessageCode.ORDER_SHOULD_HAS_ITEMS, MessageCode.ORDER_SHOULD_HAS_ITEMS.GetDescription()));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role   = User.IsInRole("Company") ? "Company" : "Person";

        var order = await _orderRepository.CreateOrderAsync(userId, role, dto.Items);
        return Ok(order);
    }
    
    /// <summary>Отметить заказ как оплаченный.</summary>
    /// <remarks>Меняет статус заказа на <c>Paid</c>, а токены — на <c>Unused</c>.</remarks>
    /// <response code="200">Заказ помечен оплаченным.</response>
    /// <response code="404">Заказ не найден или уже оплачен другим PaymentId.</response>
    [HttpPost("paid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkPaid([FromBody] PayOrderDto dto)
    {
        var code = await _orderRepository.MarkOrderPaidAsync(dto.OrderId, dto.PaymentId);
        return code == MessageCode.ORDER_IS_MARKED ? Ok(new ResponseDto(code, code.GetDescription())) : NotFound(new ResponseDto(code, code.GetDescription()));
    }
    
    /// <summary>Список оформленных заказов текущего пользователя.</summary>
    /// <remarks>
    /// Доступно ролям <c>Company</c> и <c>Person</c>.  
    /// Сортировка — от новых к старым.
    /// </remarks>
    /// <response code="200">Массив заказов.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderWithItemsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var list = await _orderRepository.GetOrdersAsync(userId);
        return Ok(list);
    }
}