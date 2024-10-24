using idcc.Infrastructures;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;
using ErrorMessage = idcc.Models.Dto.ErrorMessage;

namespace idcc.Endpoints;

public static class SessionEndpoint
{
    public static void RegisterSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var sessions = routes.MapGroup("/api/v1/session");
      
        sessions.MapPost("/start", async (int? userId, string username, string roleCode, ISessionRepository sessionRepository, IUserRepository userRepository) =>
        {
            var role = await userRepository.GetRoleAsync(roleCode);
            if (role is null)
            {
                return Results.BadRequest($"Role with code {roleCode} not found");
            }

            User? user = null;
            if (userId.HasValue)
            {
                user = await userRepository.GetUserAsync(userId.Value);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(username))
                {
                    user = await userRepository.GetUserByNameAsync(username);
                }
            }
            if (user is null)
            {
                return Results.NotFound(new ErrorMessage()
                {
                    Message = ErrorMessages.USER_IS_NULL
                });
            }
            
            var session = await sessionRepository.StartSessionAsync(user, role);
            return Results.Ok(session);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Start new test session",
            Description = "Returns fact about start session.",
            Tags = new List<OpenApiTag> { new() { Name = "Session" } }
        });

        sessions.MapPost("/stop", async (int sessioId, bool faster, ISessionRepository sessionRepository) =>
        {
            var session = await sessionRepository.GetSessionAsync(sessioId);
            if (session is null)
            {
                return Results.BadRequest(new ErrorMessage()
                {
                    Message = ErrorMessages.SESSION_IS_NOT_EXIST
                });
            }
            
            if (session.EndTime is not null)
            {
                return Results.BadRequest(new ErrorMessage()
                {
                    Message = ErrorMessages.SESSION_IS_FINISHED
                });
            }
            var isFinished = await sessionRepository.EndSessionAsync(sessioId, faster);
            return isFinished ? Results.Ok() : Results.BadRequest(new ErrorMessage()
            {
                Message = ErrorMessages.SESSION_IS_NOT_FINISHED
            });
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Stop test session",
            Description = "Returns fact about stop session.",
            Tags = new List<OpenApiTag> { new() { Name = "Session" } }
        });
    }
}