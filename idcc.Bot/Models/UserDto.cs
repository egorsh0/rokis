namespace idcc.Bot.Models;

public class UserDto
{
    public string UserName { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public RoleDto Role { get; set; } = default!;
}

public record RoleDto
{
    public string Code { get; set; } = default!;
}

public class UserFullDto : UserDto
{
    public int Id { get; set; }
    public virtual DateTime RegistrationDate { get; set; }
}