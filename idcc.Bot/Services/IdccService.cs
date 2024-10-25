using System.Net;
using System.Net.Http.Json;
using idcc.Bot.Models;

namespace idcc.Bot.Services;

public class IdccService : IIdccService
{
    private readonly BotConfiguration _settings;
    private readonly HttpClient _httpClient;

    public IdccService(BotConfiguration settings, HttpClient httpClient)
    {
        _settings = settings;
        _httpClient = httpClient;
    }
    public async Task<(SessionDto?, ErrorMessage?)> StartSessionAsync(string id, string role)
    {
        using var response = await _httpClient.PostAsync($"http://{_settings.IdccApi}/api/v1/session/start?username={id}&roleCode={role}", new StringContent(""));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>();
            return (null, error);
        }

        var session = await response.Content.ReadFromJsonAsync<SessionDto>();
        return (session, null);
    }

    public async Task<ErrorMessage?> StopSessionAsync(string id)
    {
        using var response = await _httpClient.PostAsync($"http://{_settings.IdccApi}/api/v1/session/actualStop?username={id}", new StringContent(""));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>();
            return error;
        }
        return null;
    }

    public async Task<(QuestionDto?, ErrorMessage?, bool)> GetQuestionAsync(string id)
    {
        using var response = await _httpClient.GetAsync($"http://{_settings.IdccApi}/api/v1/question?username={id}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>();
            return (null, error, false);
        }
        
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return (null, null, false);
        }
        
        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            return (null, null, true);
        }

        var question = await response.Content.ReadFromJsonAsync<QuestionDto>();
        return (question, null, false);
    }
    
    public async Task<(ReportDto?, ErrorMessage?)> GetReportAsync(string id)
    {
        using var response = await _httpClient.GetAsync($"http://{_settings.IdccApi}/api/v1/report/generate?username={id}");
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>();
            return (null, error);
        }

        var report = await response.Content.ReadFromJsonAsync<ReportDto>();
        return (report, null);
    }

    public async Task<ErrorMessage?> GetUserAsync(string id)
    {
        using var response = await _httpClient.GetAsync($"http://{_settings.IdccApi}/api/v1/user?username={id}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>();
            return error;
        }
        return null;
    }

    public async Task<(UserFullDto? userFull, ErrorMessage? message)> CreateUserAsync(string id, string username)
    {
        var newUser = new UserDto
        {
            UserName = id, PasswordHash = $"{username}"
        };
        using var response = await _httpClient.PostAsJsonAsync($"http://{_settings.IdccApi}/api/v1/user", newUser);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>();
            return (null, error);
        }

        var userFullDto = await response.Content.ReadFromJsonAsync<UserFullDto>();
        return (userFullDto, null);
    }

    public async Task<ErrorMessage?> SendAnswerAsync(string id, int questionId, int answerId, DateTime questionTime)
    {
        var answer = new QuestionShortDto
        {
            Id = questionId,
            Answers =
            [
                new()
                {
                    Id = answerId
                }
            ]
        };

        var dateInterval =  DateTime.Now - questionTime;
        using var response = await _httpClient.PostAsJsonAsync($"http://{_settings.IdccApi}/api/v1/question/answers?username={id}&dateInterval={dateInterval}", answer);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>();
            
            return error;
        }

        return null;
    }
}