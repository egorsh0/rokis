using idcc.Bot.Models;

namespace idcc.Bot.Services;

public interface IIdccService
{
        
    Task<(UserFullDto? userFull, ErrorMessage? message)> CreateUserAsync(string username);
    
    Task<(SessionDto? session, ErrorMessage? message)> StartSessionAsync(string username, string role);
    Task<ErrorMessage?> StopSessionAsync(string username);
    
    Task<(QuestionDto? question, ErrorMessage? message, bool next)> GetQuestionAsync(string username);
    
    Task<ErrorMessage?> SendAnswerAsync(string username, int questionId, int answerId, DateTime questionTime);
    
    Task<(ReportDto? report, ErrorMessage? message)> GetReportAsync(string username);
}