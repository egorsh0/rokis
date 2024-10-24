using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class UserEndpoint
{
    public static void RegisterUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/user");
      
        users.MapPost("", async (UserDto userDto, IUserRepository userRepository) =>
        {
            var usr = await userRepository.GetUserByNameAsync(userDto.UserName);

            if (usr is not null)
            {
                return Results.BadRequest(new ErrorMessage()
                {
                    Message = "Пользователь уже зарегистрирован"
                });
            }
            
            var user = new User()
            {
                UserName = userDto.UserName,
                PasswordHash = userDto.PasswordHash,
                RegistrationDate = DateTime.Now
            };
    
            var resultUser = await userRepository.CreateAsync(user);
            return Results.Ok(resultUser);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Сreate a user account",
            Description = "Returns information about created user.",
            Tags = new List<OpenApiTag> { new() { Name = "User" } }
        });
    }
}