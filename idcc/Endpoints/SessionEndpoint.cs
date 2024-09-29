using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class SessionEndpoint
{
    public static void RegisterSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var sessions = routes.MapGroup("/api/v1/session");
      
        sessions.MapPost("/start", async (int userId, ISessionRepository sessionRepository, IUserRepository userRepository) =>
        {
            var user = await userRepository.GetUserAsync(userId);
            if (user is null)
            {
                return Results.NotFound();
            }
            
            var session = await sessionRepository.StartSessionAsync(user);
            return Results.Ok(session);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Start new test session",
            Description = "Returns fact about start session.",
            Tags = new List<OpenApiTag> { new() { Name = "Session" } }
        });

        sessions.MapPost("/stop", async (int sessioId, ISessionRepository sessionRepository, IUserRepository userRepository) =>
        {
            var session = await sessionRepository.GetSessionAsync(sessioId);
            if (session is null)
            {
                return Results.BadRequest(ErrorMessage.SESSION_IS_NOT_EXIST);
            }
            
            if (session.EndTime is not null)
            {
                return Results.BadRequest(ErrorMessage.SESSION_IS_FINISHED);
            }
            var isFinished = await sessionRepository.EndSessionAsync(sessioId);
            return isFinished ? Results.Ok() : Results.BadRequest(ErrorMessage.SESSION_IS_NOT_FINISHED);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Stop test session",
            Description = "Returns fact about stop session.",
            Tags = new List<OpenApiTag> { new() { Name = "Session" } }
        });
    }
}