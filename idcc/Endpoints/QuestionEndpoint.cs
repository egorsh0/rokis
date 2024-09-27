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
      
        questions.MapGet("/{userId}", async (int userId, IUserTopicRepository userTopicRepository, IQuestionRepository questionRepository, ISessionRepository sessionRepository) =>
        {
            // Проверка на открытые темы
        
            var hasOpenTopics = await userTopicRepository.HasOpenTopic(userId);
            if (!hasOpenTopics)
            {
                return Results.BadRequest(ErrorMessage.TOPIC_IS_NOT_EXIST);
            }
            var session = await sessionRepository.GetSessionAsync(userId);
            if (session is null)
            {
                return Results.BadRequest(ErrorMessage.SESSION_IS_NOT_EXIST);
            }

            if (session.EndTime is not null)
            {
                return Results.BadRequest(ErrorMessage.SESSION_IS_FINISHED);
            }
            
            // Получение рандомной темы
            var userTopic = await userTopicRepository.GetRandomTopicAsync(userId);
            if (userTopic is null)
            {
                return Results.NoContent();
            }
                
            var question = await questionRepository.GetQuestionAsync(userTopic);
            if (question is null)
            {
                return Results.BadRequest();
            }
                
            await userTopicRepository.RefreshActualTopicInfoAsync(userTopic.Id, userId);
            return Results.Ok(question);

        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get random question",
            Description = "Returns random question with list answers.",
            Tags = new List<OpenApiTag> { new() { Name = "Question" } }
        });
        
        questions.MapPost("/sendAnswers", async (int userId, TimeSpan dateInterval, QuestionDto question,
            IUserRepository userRepository,
            ISessionRepository sessionRepository,
            IIdccApplication idccApplication
        ) =>
        {
            var session = await sessionRepository.GetSessionAsync(userId);
            if (session is null)
            {
                return Results.NotFound();
            }
            // Посчитать и сохранить Score за ответ
            var result = await idccApplication.CalculateScoreAsync(session, userId, Convert.ToInt32(dateInterval.TotalSeconds), question.Id,
                question.Answers.Select(_ => _.Id));
            if (result is not null)
            {
                return Results.NotFound(result);
            }
            // Пересчитать вес текущего топика
            result = await idccApplication.CalculateTopicWeightAsync(session, userId);
            return result is not null ? Results.NotFound(result) : Results.Ok();
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Send answers",
            Description = "Returns fact about sending answers.",
            Tags = new List<OpenApiTag> { new() { Name = "Question" } }
        });
    }
}