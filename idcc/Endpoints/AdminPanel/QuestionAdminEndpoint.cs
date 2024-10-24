using idcc.Infrastructures;
using idcc.Models.AdminDto;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints.AdminPanel;

public static class QuestionAdminEndpoint
{
    public static void RegisterQuestionAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var adminQuestions = routes.MapGroup("/api/v2/question");

        adminQuestions.MapPost("/create", async (List<QuestionAdminDto> questions, IQuestionRepository questionRepository
            ) =>
        {
            if (!questions.Any())
            {
                return Results.BadRequest(new ErrorMessage
                {
                    Message = ErrorMessages.QUESTIONS_IS_EMPTY
                });
            }
            var notAddedQuestions = await questionRepository.CreateAsync(questions);

            return Results.Ok(notAddedQuestions);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Create questions",
            Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
        });
    }
}