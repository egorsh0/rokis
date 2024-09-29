using idcc.Bot.Models;

namespace idcc.Bot.Services;

public interface IIdccService
{
    Task<(SessionDto? session, string? message)> StartSessionAsync(int userId);
    
    Task<(QuestionDto? question, string? message)> GetQuestionAsync(int sessionId);
    
    Task<(UserFullDto? userFull, string? message)> CreateUserAsync(string username, string role);
    
    Task<string?> SendAnswerAsync(int sessionId, int questionId, int answerId, DateTime questionTime);
}