using idcc.Repository.Interfaces;

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

            var session = await sessionRepository.GetSessionAsync(userId);
            if (session is not null)
            {
                return Results.BadRequest();
            }
            await sessionRepository.StartSessionAsync(user);
            return Results.Ok();

        });

        sessions.MapPost("/stop", async (int userId, ISessionRepository sessionRepository, IUserRepository userRepository) =>
        {
            var user = await userRepository.GetUserAsync(userId);
            if (user is null)
            {
                return Results.NotFound();
            }

            var session = await sessionRepository.GetSessionAsync(userId);
            if (session is null)
            {
                return Results.BadRequest();
            }

            if (session.EndTime is not null)
            {
                return Results.BadRequest();
            }
            await sessionRepository.EndSessionAsync(user);
            return Results.Ok();
        });
    }
}