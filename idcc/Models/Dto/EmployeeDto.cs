namespace idcc.Models.Dto;

public record EmployeeDto
{
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
}