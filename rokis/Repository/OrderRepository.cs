using rokis.Context;
using rokis.Dtos;
using rokis.Infrastructures;
using rokis.Models;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface IOrderRepository
{
    Task<MessageCode> MarkOrderPaidAsync(int orderId, string paymentId);
    
    Task<OrderDto> CreateOrderAsync(string userId, string role, List<PurchaseTokensDto> items);
    
    Task<IEnumerable<OrderWithItemsDto>> GetOrdersAsync(string userId);
}

public class OrderRepository : IOrderRepository
{
    private readonly RokisContext _rokisContext;

    public OrderRepository(RokisContext rokisContext)
    {
        _rokisContext = rokisContext;
    }

    public async Task<MessageCode> MarkOrderPaidAsync(int orderId, string paymentId)
    {
        var order = await _rokisContext.Orders
            .Include(o => o.Tokens)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        // ── 1.  заказ не существует
        if (order is null)
        {
            return MessageCode.ORDER_NOT_FOUND;
        }

        switch (order.Status)
        {
            // ── 2.  заказ уже оплачен этим же paymentId  → OK (идемпотентность)
            case OrderStatus.Paid when order.PaymentId == paymentId:
            {
                return MessageCode.ORDER_PAID;
            }
            // ── 3.  заказ был оплачен другим paymentId  → считаем конфликт
            case OrderStatus.Paid when order.PaymentId != paymentId:
            {
                return MessageCode.ORDER_PAID;
            }
        }

        // ── 4.  отмечаем как оплаченный
        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.PaymentId = paymentId;

        // токены становятся доступными
        foreach (var tok in order.Tokens.Where(t => t.Status == TokenStatus.Pending))
        {
            tok.Status = order.Role == "Person"
                ? TokenStatus.Bound   // физ. лицо: сразу привязан
                : TokenStatus.Unused; // компания: ждёт привязки
        }

        await _rokisContext.SaveChangesAsync();
        return MessageCode.ORDER_IS_MARKED;
    }
    
    public async Task<OrderDto> CreateOrderAsync(string userId,
        string role,
        List<PurchaseTokensDto> items)
    {
        // 1. Подсчитываем общее количество токенов
        var totalQty = items.Sum(i => i.Quantity);

        // 2. Ищем скидку
        var rule = await _rokisContext.DiscountRules
            .OrderBy(r => r.MinQuantity)
            .FirstOrDefaultAsync(r =>
                totalQty >= r.MinQuantity &&
                (r.MaxQuantity == null || totalQty <= r.MaxQuantity));

        var discountRate = rule?.DiscountRate ?? 0m;

        // 3. Создаём заказ
        var order = new Order
        {
            UserId = userId,
            Role = role,
            Quantity = totalQty,
            DiscountRate = discountRate,
            UnitPrice = 0,      // посчитаем ниже
            TotalPrice = 0,
            DiscountedTotal = 0,
            Status = OrderStatus.Unpaid
        };
        _rokisContext.Orders.Add(order);

        decimal grandTotal = 0;

        // 4. Для каждого направления генерируем токены
        foreach (var itm in items)
        {
            var dir = await _rokisContext.Directions.FindAsync(itm.DirectionId)
                      ?? throw new Exception("Direction not found");

            var price = dir.BasePrice;
            for (var i = 0; i < itm.Quantity; i++)
            {
                _rokisContext.Tokens.Add(new Token
                {
                    DirectionId = dir.Id,
                    UnitPrice = price,
                    Status = TokenStatus.Pending,
                    PurchaseDate = DateTime.UtcNow,
                    PersonUserId = role == "Person" ? userId : null,
                    Order = order
                });
            }

            grandTotal += price * itm.Quantity;
        }

        order.UnitPrice = grandTotal / totalQty;
        order.TotalPrice = grandTotal;
        order.DiscountedTotal = grandTotal * (1 - discountRate);

        await _rokisContext.SaveChangesAsync();
        return new OrderDto(
            order.Id,
            order.Quantity,
            order.UnitPrice,
            order.TotalPrice,
            order.DiscountRate,
            order.DiscountedTotal,
            order.Tokens.Select(t => new TokenDto(
                t.Id,
                t.DirectionId,
                t.Direction.Name,
                t.UnitPrice,
                t.Status,
                t.PurchaseDate,
                t.Score,
                null, null, null, null, null)));
    }
    
    public async Task<IEnumerable<OrderWithItemsDto>> GetOrdersAsync(string userId)
    {
        var orders = await _rokisContext.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Id)
            .Include(o => o.Tokens)
            .ThenInclude(t => t.Direction)
            .AsNoTracking()
            .ToListAsync();

        var result = orders.Select(o => new OrderWithItemsDto(
            o.Id,
            o.Tokens.Any()
                ? o.Tokens.Min(t => t.PurchaseDate)
                : DateTime.MinValue,
            o.Quantity,
            o.DiscountedTotal,
            o.Status,
            o.Tokens
                .GroupBy(t => new { id = t.DirectionId, name = t.Direction.Name })
                .Select(g => new OrderDirectionQtyDto(
                    g.Key.id,
                    g.Key.name,
                    g.Count()))
                .ToList())).ToList();
        return result;
    }
}