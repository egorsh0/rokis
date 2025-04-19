namespace idcc.Dtos;

public record PurchaseTokensDto(int DirectionId, int Quantity);
public record CreateOrderDto(string Role, List<PurchaseTokensDto> Items); // "Company" или "Person"