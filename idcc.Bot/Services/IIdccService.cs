using idcc.Bot.Models;

namespace idcc.Bot.Services;

public interface IIdccService
{
    Task<ErrorMessage?> GetUserAsync(string id);
    Task<(UserFullDto? userFull, ErrorMessage? message)> CreateUserAsync(string id, string username);
    
    Task<(SessionDto? session, ErrorMessage? message)> StartSessionAsync(string id, string role);
    Task<ErrorMessage?> StopSessionAsync(string id);
    
    Task<(QuestionDto? question, ErrorMessage? message, bool next)> GetQuestionAsync(string id);
    
    Task<ErrorMessage?> SendAnswerAsync(string id, int questionId, int answerId, DateTime questionTime);
    
    Task<(ReportDto? report, ErrorMessage? message)> GetReportAsync(string id);
}