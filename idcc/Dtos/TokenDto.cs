namespace idcc.Dtos;

public record BindTokenDto(Guid TokenId, string EmployeeEmail);
public record BindUsedTokenDto(Guid TokenId, string UserEmail );