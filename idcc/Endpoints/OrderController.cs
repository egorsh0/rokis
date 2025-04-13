using idcc.Context;
using idcc.Dtos;
using idcc.Models;
using Microsoft.EntityFrameworkCore;

namespace idcc.Endpoints;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/v1/order")]
public class OrdersController : ControllerBase
{
    private readonly IdccContext _idccContext;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IdccContext idccContext, ILogger<OrdersController> logger)
    {
        _idccContext = idccContext;
        _logger = logger;
    }

    /// <summary>
    /// Создать заказ (только для ролей Company и Person)
    /// </summary>
    [HttpPost("create")]
    [Authorize(Roles = "Company,Person")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            // Получаем userId текущего пользователя из токена/контекста
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Не удалось определить пользователя (ClaimTypes.NameIdentifier отсутствует).");
                return Unauthorized("Не удалось определить пользователя.");
            }

            // Создаём новый заказ
            var order = new Order
            {
                Quantity = dto.Quantity,
                UserId = userId
            };

            _idccContext.Orders.Add(order);
            await _idccContext.SaveChangesAsync();

            _logger.LogInformation("Создан заказ Id={OrderId}, UserId={UserId}", order.Id, userId);

            return Ok(new
            {
                message = "Заказ успешно создан",
                orderId = order.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании заказа");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Получить все заказы текущего пользователя (компании или физ. лица)
    /// </summary>
    [HttpGet("my-orders")]
    [Authorize(Roles = "Company,Person")]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }

            var orders = await _idccContext.Orders
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении заказов пользователя");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}
