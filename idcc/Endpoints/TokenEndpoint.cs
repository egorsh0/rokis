using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class TokenEndpoint
{
    public static void RegisterTokenEndpoint(this IEndpointRouteBuilder routes)
    {
        var tokenRoute = routes.MapGroup("/api/v1/tokens");

        tokenRoute.MapGet("/", async (ITokenRepository tokenRepository) =>
        {
            var tokens = await tokenRepository.GetTokensAsync();
            return Results.Ok(tokens);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Получение токенов",
            Description = "Список токенов",
            Tags = new List<OpenApiTag> { new() { Name = "Tokens" } }
        });

        tokenRoute.MapPost("/assign", async () =>
        {
            return Results.Ok();
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Привязка токенов к сотрудникам",
            Description = "Токены привязаны",
            Tags = new List<OpenApiTag> { new() { Name = "Tokens" } }
        });
        
        tokenRoute.MapPost("/generate", async () =>
        {
            return Results.Ok();
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Генерация токенов",
            Description = "Токены сгенерированы",
            Tags = new List<OpenApiTag> { new() { Name = "Tokens" } }
        });
        
        tokenRoute.MapGet("/status", async () =>
        {
            return Results.Ok();
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Статус токенов",
            Description = "Статусы возвращены",
            Tags = new List<OpenApiTag> { new() { Name = "Tokens" } }
        });
        
        tokenRoute.MapPost("/validate", async (string payload, ITokenRepository tokenRepository) =>
        {
            var token = await tokenRepository.GetTokenByCodeAsync(payload);
            if (token == null)
            {
                return Results.NotFound("Token not found");
            }

            if (token.Status != "NotUsed")
            {
                return Results.BadRequest("Token already used");
            }

            token.Status = "InProgress";
            tokenRepository.UpdateToken(token);
            return Results.Ok(token);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Валидация токена перед тестом",
            Description = "Токен валиден",
            Tags = new List<OpenApiTag> { new() { Name = "Tokens" } }
        });
    }
}