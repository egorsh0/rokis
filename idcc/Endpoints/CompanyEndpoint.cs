using idcc.Models.Dto;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class CompanyEndpoint
{
    public static void RegisterCompanyEndpoints(this IEndpointRouteBuilder routes)
    {
        var companyRoute = routes.MapGroup("/api/v1/company");
      
        companyRoute.MapPost("/register", async ([FromBody] RegisterCompanyDto payload, ICompanyRepository companyRepository) =>
        {
            var company = await companyRepository.GetAsync(new SearchCompanyPayload(payload.Inn, payload.Email));
            if (company != null)
            {
                return Results.Conflict("Компания с таким ИНН или EMail уже существует.");
            }
            var entity = await companyRepository.CreateAsync(new RegisterCompanyDto(payload.Name, payload.Inn, payload.Email, payload.Password));
            return Results.Ok(entity);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Регистрация компании",
            Description = "Компания зарегистрирована.",
            Tags = new List<OpenApiTag> { new() { Name = "Company" } }
        });

        companyRoute.MapGet("/search", async ([FromBody] SearchCompanyPayload payload, ICompanyRepository companyRepository) =>
        {
            var company = await companyRepository.GetAsync(payload);
            if (company != null)
            {
                return Results.NotFound("Компания с таким ИНН или EMail не найдена.");
            }

            return Results.Ok(company);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Поиск компании",
            Description = "Компания найдена.",
            Tags = new List<OpenApiTag> { new() { Name = "Company" } }
        });

        companyRoute.MapPost("/login", async ([FromBody] LoginCompanyPayload payload, ICompanyRepository companyRepository) =>
        {
            var company = await companyRepository.GetAsync(payload);
            if (company != null)
            {
                return Results.BadRequest("Некорретный ИНН/Email или пароль.");
            }

            return Results.Ok(company);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Авторизация аккаунта компании.",
            Description = "Авторизация успешна.",
            Tags = new List<OpenApiTag> { new() { Name = "Company" } }
        });
    }
}