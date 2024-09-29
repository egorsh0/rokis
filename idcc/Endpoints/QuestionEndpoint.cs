using idcc.Application.Interfaces;
using idcc.Infrastructures;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class QuestionEndpoint
{
    public static void RegisterQuestionEndpoints(this IEndpointRouteBuilder routes)
    {
        var questions = routes.MapGroup("/api/v1/question");
      
        questions.MapGet("/{sessionId}", async (int sessionId, IUserTopicRepository userTopicRepository, IQuestionRepository questionRepository, ISessionRepository sessionRepository) =>
        {
            // Проверка на открытую сессию
            var session = await sessionRepository.GetSessionAsync(sessionId);
            if (session is null)
            {
                return Results.BadRequest(ErrorMessage.SESSION_IS_NOT_EXIST);
            }
            
            if (session.EndTime is not null)
            {
                return Results.BadRequest(ErrorMessage.SESSION_IS_FINISHED);
            }
            
            // Проверка на открытые темы
            var hasOpenTopics = await userTopicRepository.HasOpenTopic(session);
            if (!hasOpenTopics)
            {
                return Results.NoContent();
            }
            
            // Получение рандомной темы
            var userTopic = await userTopicRepository.GetRandomTopicAsync(session);
            if (userTopic is null)
            {
                return Results.BadRequest();
            }
                
            var question = await questionRepository.GetQuestionAsync(userTopic);
            if (question is null)
            {
                return Results.BadRequest();
            }
                
            await userTopicRepository.RefreshActualTopicInfoAsync(userTopic.Id, session);
            return Results.Ok(question);

        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get random question",
            Description = "Returns random question with list answers.",
            Tags = new List<OpenApiTag> { new() { Name = "Question" } }
        });
        
        questions.MapPost("/sendAnswers", async (int sessionId, TimeSpan dateInterval, QuestionShortDto question,
            IUserRepository userRepository,
            ISessionRepository sessionRepository,
            IIdccApplication idccApplication
        ) =>
        {
            var session = await sessionRepository.GetSessionAsync(sessionId);
            if (session is null)
            {
                return Results.BadRequest();
            }
            // Посчитать и сохранить Score за ответ
            var result = await idccApplication.CalculateScoreAsync(session, Convert.ToInt32(dateInterval.TotalSeconds), question.Id,
                question.Answers.Select(_ => _.Id).ToList());
            if (result is not null)
            {
                return Results.BadRequest(result);
            }
            // Пересчитать вес текущего топика
            result = await idccApplication.CalculateTopicWeightAsync(session);
            return result is not null ? Results.BadRequest(result) : Results.Ok();
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Send answers",
            Description = "Returns fact about sending answers.",
            Tags = new List<OpenApiTag> { new() { Name = "Question" } }
        });
    }
}