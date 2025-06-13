using idcc.Infrastructures;

namespace idcc.Dtos;

public record OrderDto(
    int Id,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    decimal DiscountRate,
    decimal DiscountedTotal,
    IEnumerable<TokenDto> Tokens);
public record PurchaseTokensDto(int DirectionId, int Quantity);
public record CreateOrderDto(List<PurchaseTokensDto> Items); // "Company" или "Person"

public record PayOrderDto(int OrderId, string PaymentId);

/// <summary>
/// Короткая запись о заказе токенов.<br/>
/// </summary>
/// <remarks>
/// * Заказ из нескольких позиций может включать токены разных направлений,
///   поэтому поле <see cref="Items"/> — коллекция направлений.<br/>
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
/// <param name="Items">
/// Список направлений, содержащихся в заказе  
/// (например <c>["QA","Dev"]</c>) с информацией по токенм.  
/// Порядок не гарантирован, дубликаты исключены.
/// </param>
/// <param name="TotalQuantity">Общее количество купленных токенов.</param>
/// <param name="TotalAmount">
/// Итоговая сумма с учётом скидки, в валюте системы (например, RUB).  
/// Соответствует полю <c>DiscountedTotal</c> в сущности <c>Order</c>.
/// </param>
/// <param name="Status">
/// Текущий статус заказа:  
/// • <c>Unpaid</c> — заказ создан, ожидает оплаты;  
/// • <c>Paid</c>   — оплачен, токены активны.
/// </param>
public record OrderWithItemsDto(
    int OrderId,
    DateTime PurchaseDate,
    int TotalQuantity,
    decimal TotalAmount,
    OrderStatus Status,
    List<OrderDirectionQtyDto> Items);
    
/// <summary>Количество токенов конкретного направления в заказе.</summary>
/// <param name="DirectionId">Уникальный идентификатор специальности.</param>
/// <param name="DirectionName">Имя специальности.</param>
/// <param name="Quantity">Количество токенов.</param>
public record OrderDirectionQtyDto(
    int DirectionId,
    string DirectionName,
    int Quantity);