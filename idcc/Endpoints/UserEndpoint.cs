using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;

namespace idcc.Endpoints;

public static class UserEndpoint
{
    public static void RegisterUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/user");
      
        users.MapPost("", async (UserDto userDto, IUserRepository userRepository) =>
        {
            var role = await userRepository.GetRoleAsync(userDto.Role.Name);
            if (role is null)
            {
                return Results.BadRequest(string.Format("Role with name {name} not found", userDto.Role.Name));
            }

            var user = new User()
            {
                Role = role,
                UserName = userDto.UserName,
                PasswordHash = userDto.PasswordHash,
                RegistrationDate = DateTime.Now
            };
    
            var resultUser = await userRepository.CreateAsync(user);
            return Results.Ok(resultUser);
        });
    }
}