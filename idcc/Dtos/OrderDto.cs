using idcc.Infrastructures;

namespace idcc.Dtos;

public record OrderDto(
    int           Id,
    int           Quantity,
    decimal       UnitPrice,
    decimal       TotalPrice,
    decimal       DiscountRate,
    decimal       DiscountedTotal,
    IEnumerable<TokenDto> Tokens);
public record PurchaseTokensDto(int DirectionId, int Quantity);
public record CreateOrderDto(List<PurchaseTokensDto> Items); // "Company" или "Person"

public record PayOrderDto(int OrderId, string PaymentId);

/// <summary>
/// Короткая запись о заказе токенов.<br/>
/// </summary>
/// <remarks>
/// * Заказ из нескольких позиций может включать токены разных направлений,
///   поэтому поле <see cref="Directions"/> — коллекция строк.<br/>
/// * <see cref="Status"/> — бизнес-статус заказа  
///   (<c>Unpaid</c> — оформлен, но не оплачен; <c>Paid</c> — оплачен).<br/>
/// * После оплаты токены с этого заказа переходят из <c>Pending</c>
///   в <c>Unused</c> и становятся доступны пользователю.
/// </remarks>
/// <param name="OrderId">Уникальный идентификатор заказа.</param>
/// <param name="PurchaseDate">
/// Дата/время оформления заказа (UTC).  
/// Если платёж уже прошёл — обычно совпадает с <c>PaidAt</c>.
/// </param>
/// <param name="Directions">
/// Список направлений, содержащихся в заказе  
/// (например <c>["QA","Dev"]</c>).  
/// Порядок не гарантирован, дубликаты исключены.
/// </param>
/// <param name="Quantity">Общее количество купленных токенов.</param>
/// <param name="Amount">
/// Итоговая сумма с учётом скидки, в валюте системы (например, RUB).  
/// Соответствует полю <c>DiscountedTotal</c> в сущности <c>Order</c>.
/// </param>
/// <param name="Status">
/// Текущий статус заказа:  
/// • <c>Unpaid</c> — заказ создан, ожидает оплаты;  
/// • <c>Paid</c>   — оплачен, токены активны.
/// </param>
public record OrderListItemDto(
    int              OrderId,
    DateTime         PurchaseDate,
    List<string>     Directions,
    int              Quantity,
    decimal          Amount,
    OrderStatus      Status);