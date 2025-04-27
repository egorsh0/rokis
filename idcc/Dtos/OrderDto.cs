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