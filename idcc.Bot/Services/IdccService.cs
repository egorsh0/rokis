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
    public async Task<(SessionDto?, string?)> StartSessionAsync(int userId)
    {
        using var response = await _httpClient.PostAsync($"http://{_settings.IdccApi}/api/v1/session/start?userId={userId}", new StringContent(""));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (null, error);
        }

        var session = await response.Content.ReadFromJsonAsync<SessionDto>();
        return (session, null);
    }

    public async Task<(QuestionDto?, string?)> GetQuestionAsync(int sessionId)
    {
        using var response = await _httpClient.GetAsync($"http://{_settings.IdccApi}/api/v1/question/{sessionId}");
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (null, error);
        }
        
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return (null, "Тестирование завершено!");
        }

        var question = await response.Content.ReadFromJsonAsync<QuestionDto>();
        return (question, null);
    }

    public async Task<(UserFullDto? userFull, string? message)> CreateUserAsync(string username, string role)
    {
        var newUser = new UserDto
        {
            UserName = username, PasswordHash = $"PasswordHash{username}", Role = new RoleDto()
            {
                Code = role
            }
        };
        using var response = await _httpClient.PostAsJsonAsync($"http://{_settings.IdccApi}/api/v1/user", newUser);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (null, error);
        }

        var userFullDto = await response.Content.ReadFromJsonAsync<UserFullDto>();
        return (userFullDto, null);
    }

    public async Task<string?> SendAnswerAsync(int sessionId, int questionId, int answerId, DateTime questionTime)
    {
        var answer = new QuestionShortDto
        {
            Id = questionId,
            Answers = new List<AnswerShortDto>()
            {
                new()
                {
                    Id = answerId
                }
            }
        };

        var dateInterval = questionTime - DateTime.Now;
        using var response = await _httpClient.PostAsJsonAsync($"http://{_settings.IdccApi}/api/v1/question/sendAnswers?sessionId={sessionId}&dateInterval={dateInterval}", answer);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadAsStringAsync();
            
            return error;
        }

        return null;
    }
}