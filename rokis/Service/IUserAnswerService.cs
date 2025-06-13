using rokis.Repository;

namespace rokis.Service;

public interface IUserAnswerService
{
    /// <summary>
    /// Количество пользовательских ответов.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="topicId">Идентификатор темы.</param>
    /// <returns></returns>
    Task<int> CountUserAnswersAsync(int sessionId, int topicId);
    
    /// <summary>
    /// Можно ли закрыть тему?
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="topicId">Идентификатор темы.</param>
    /// <param name="limit">Лимит закрытия.</param>
    /// <returns></returns>
    Task<bool> CanCloseTopicAsync(int sessionId, int topicId, int limit);
    
    /// <summary>
    /// Можно ли повысить ранг темы?
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="topicId">Идентификатор темы.</param>
    /// <param name="limit">Лимит закрытия.</param>
    /// <returns></returns>
    Task<bool> CanRaiseTopicAsync(int sessionId, int topicId, int limit);
}

public class UserAnswerService : IUserAnswerService
{
    private readonly IUserAnswerRepository _userAnswers;

    public UserAnswerService(IUserAnswerRepository userAnswers)
    {
        _userAnswers = userAnswers;
    }

    public async Task<int> CountUserAnswersAsync(int sessionId, int topicId)
    {
        var list = await _userAnswers.GetUserAnswersAsync(sessionId, topicId);
        return list.Count;
    }

    public async Task<bool> CanCloseTopicAsync(int sessionId, int topicId, int limit)
    {
        var list = await _userAnswers.GetUserAnswersAsync(sessionId, topicId);
        var streak = 0;
        foreach (var ua in list)
        {
            if (ua.Score == 0)
            {
                streak++;
                // N подряд ошибок
                if (streak >= limit)
                {
                    return true;
                }
            }
            else
            {
                // сброс серии
                streak = 0;
            }
        }
        return false;
    }

    public async Task<bool> CanRaiseTopicAsync(int sessionId, int topicId, int limit)
    {
        var list = await _userAnswers.GetUserAnswersAsync(sessionId, topicId);
        var correctCount = limit;
        foreach (var userAnswer in list)
        {
            bool hasCorrect;
            if (userAnswer.Score > 0)
            {
                correctCount--;
                hasCorrect = true;
            }
            else
            {
                return false;
            }

            switch (hasCorrect)
            {
                case false when correctCount > 0:
                    return false;
                case true when correctCount == 0:
                    return true;
            }
        }

        return false;
    }
}