using idcc.Dtos;
using idcc.Infrastructures;

namespace idcc.Repository.Interfaces;

public interface IOrderRepository
{
    Task<MessageCode> MarkOrderPaidAsync(int orderId, string paymentId);
    
    Task<OrderDto> CreateOrderAsync(string userId, string role, List<PurchaseTokensDto> items);
    
    Task<IEnumerable<OrderWithItemsDto>> GetOrdersAsync(string userId);
}