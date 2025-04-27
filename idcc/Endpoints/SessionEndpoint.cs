using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class SessionEndpoint
{
    public static void RegisterSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var sessionsRoute = routes.MapGroup("/api/session");

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
    }
}