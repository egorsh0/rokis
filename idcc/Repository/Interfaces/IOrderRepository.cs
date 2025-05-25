using idcc.Dtos;

namespace idcc.Repository.Interfaces;

public interface IOrderRepository
{
    Task<bool> MarkOrderPaidAsync(int orderId, string paymentId);
    
    Task<OrderDto> CreateOrderAsync(string userId, string role, List<PurchaseTokensDto> items);
    
    Task<IEnumerable<OrderListItemDto>> GetOrdersAsync(string userId);
}