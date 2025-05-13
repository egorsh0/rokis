using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class SessionEndpoint
{
    public static void RegisterSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var sessionsRoute = routes.MapGroup("/api/session");

        sessionsRoute.MapPost("/stop", 
            /// <summary>Досрочно (или штатно) завершает сессию тестирования.</summary>
            /// <remarks>
            /// <para>
            /// • <paramref name="sessionId"/> — идентификатор сессии.<br/>
            /// • <paramref name="faster"/> = <see langword="true"/> означает принудительное
            /// завершение, даже если вопросы не исчерпаны.
            /// </para>
            /// </remarks>
            /// <response code="200">Сессия успешно завершена.</response>
            /// <response code="400">
            /// <list type="bullet">
            ///   <item><description><c>Сессии не существует.</c></description></item>
            ///   <item><description><c>Сессия завершена.</c></description></item>
            ///   <item><description><c>Сессия не завершена.</c></description></item>
            /// </list>
            /// </response>
            async (int sessionId, bool faster, ISessionRepository sessionRepository) =>
        {
            if (sessionId <= 0)
            {
                return Results.BadRequest("sessionId must be > 0");
            }
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
        }).Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithOpenApi(op =>
            {
                op.Summary     = "Stop test session";
                op.Description = "Завершает указанную сессию; возвращает 200 OK при успехе.";
                op.OperationId = "StopSession";

                op.Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name        = "sessionId",
                        In          = ParameterLocation.Query,
                        Required    = true,
                        Schema      = new OpenApiSchema { Type = "integer", Format = "int32" },
                        Description = "Идентификатор сессии."
                    },
                    new()
                    {
                        Name        = "faster",
                        In          = ParameterLocation.Query,
                        Required    = false,
                        Schema      = new OpenApiSchema
                        {
                            Type = "boolean",
                            Default = new OpenApiBoolean(false)
                        },
                        Description = "Принудительное завершение (даже если ещё есть вопросы)."
                    }
                };
                return op;
            });
    }
}