namespace idcc.Models.Dto;

public record UserDto
{
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public RoleDto Role { get; set; }
}