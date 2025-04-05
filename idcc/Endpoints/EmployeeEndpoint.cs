using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class EmployeeEndpoint
{
    public static void RegisterEmployeeEndpoints(this IEndpointRouteBuilder routes)
    {
        var employeeRoute = routes.MapGroup("/api/v1/employee");
      
        employeeRoute.MapPost("", async (EmployeeDto employeeDto, IEmployeeRepository employeeRepository) =>
        {
            var empl = await employeeRepository.GetEmployeeByNameAsync(employeeDto.UserName);

            if (empl is not null)
            {
                return Results.BadRequest(new ErrorMessage()
                {
                    Message = "Пользователь уже зарегистрирован"
                });
            }
            
            var employee = new Employee()
            {
                Name = employeeDto.UserName,
                Token = employeeDto.PasswordHash,
                RegistrationDate = DateTime.Now
            };
    
            var resultEmployee = await employeeRepository.CreateAsync(employee);
            return Results.Ok(resultEmployee);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Сreate a employee account",
            Description = "Returns information about created employee.",
            Tags = new List<OpenApiTag> { new() { Name = "Employee" } }
        });
        
        employeeRoute.MapGet("", async (string name, IEmployeeRepository employeeRepository) =>
        {
            var empl = await employeeRepository.GetEmployeeByNameAsync(name);

            if (empl is null)
            {
                return Results.BadRequest(new ErrorMessage()
                {
                    Message = "Пользователь не зарегистрирован"
                });
            }
            
            return Results.Ok();
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get a employee account",
            Description = "Returns information about created employee.",
            Tags = new List<OpenApiTag> { new() { Name = "Employee" } }
        });
    }
}