namespace idcc.Models.Dto;

public record UserDto
{
    public string FullName { get; set; }
    public RoleDto Role { get; set; }
}