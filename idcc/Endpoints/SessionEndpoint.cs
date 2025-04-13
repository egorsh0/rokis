using idcc.Infrastructures;
using idcc.Models.Profile;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class SessionEndpoint
{
    public static void RegisterSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var sessionsRoute = routes.MapGroup("/api/v1/session");
      
        sessionsRoute.MapPost("/start", async (int? userId, string username, string roleCode, ISessionRepository sessionRepository, IUserRepository userRepository) =>
        {
            var role = await userRepository.GetRoleAsync(roleCode);
            if (role is null)
            {
                return Results.BadRequest($"Role with code {roleCode} not found");
            }

            PersonProfile? user = null;
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
                return Results.NotFound(ErrorMessages.USER_IS_NULL);
            }

            var sessions = await sessionRepository.GetSessionsAsync(user);
            foreach (var ses in sessions)
            {
                await sessionRepository.EndSessionAsync(ses.Id, true);
            }
            
            var session = await sessionRepository.StartSessionAsync(user, role);
            return Results.Ok(session);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Start new test session",
            Description = "Returns fact about start session.",
            Tags = new List<OpenApiTag> { new() { Name = "Session" } }
        });

        sessionsRoute.MapPost("/stop", async (int sessionId, bool faster, ISessionRepository sessionRepository) =>
        {
            var session = await sessionRepository.GetSessionAsync(sessionId);
            if (session is null)
            {
                return Results.BadRequest(ErrorMessages.SESSION_IS_NOT_EXIST);
            }
            
            if (session.EndTime is not null)
            {
                return Results.BadRequest(ErrorMessages.SESSION_IS_FINISHED);
            }
            var isFinished = await sessionRepository.EndSessionAsync(sessionId, faster);
            return isFinished ? Results.Ok() : Results.BadRequest(ErrorMessages.SESSION_IS_NOT_FINISHED);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Stop test session",
            Description = "Returns fact about stop session.",
            Tags = new List<OpenApiTag> { new() { Name = "Session" } }
        });
        
        sessionsRoute.MapPost("/actualStop", async (string username, ISessionRepository sessionRepository) =>
        {
            var session = await sessionRepository.GetActualSessionAsync(username);
            if (session is null)
            {
                return Results.Ok();
            }
            
            if (session.EndTime is not null)
            {
                return Results.Ok();
            }
            var isFinished = await sessionRepository.EndSessionAsync(session.Id, false);
            return isFinished ? Results.Ok() : Results.BadRequest(ErrorMessages.SESSION_IS_NOT_FINISHED);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Stop test session",
            Description = "Returns fact about stop session.",
            Tags = new List<OpenApiTag> { new() { Name = "Session" } }
        });
    }
}