using rokis.Dtos;
using rokis.Repository;

namespace rokis.Service;

public interface IUserTopicService
{
    /// <summary>
    /// Создать пользовательскую тему.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="directionId">Идентификатор направления.</param>
    /// <returns></returns>
    Task CreateUserTopicAsync(int sessionId, int directionId);
    
    /// <summary>
    /// Получить пользовательские темы.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns></returns>
    Task<List<UserTopicDto>> GetUserTopicsAsync(int sessionId);
    
    /// <summary>
    /// Получить пользовательские темы.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns></returns>
    Task<List<UserTopicDto>> GetFinishUserTopicsAsync(int sessionId);
    
    /// <summary>
    /// Получить пользовательскую тему.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="userTopicId"></param>
    /// <returns></returns>
    Task<UserTopicDto?> GetUserTopicAsync(int sessionId, int userTopicId);
    
    /// <summary>
    /// Имеются ли открытые темы?
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns></returns>
    Task<bool> HasOpenTopic(int sessionId);

    /// <summary>
    /// Получить рандомную тему.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns></returns>
    Task<UserTopicDto?> GetRandomUserTopicAsync(int sessionId);

    /// <summary>
    /// Получить актуальную пользовательскую тему.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns></returns>
    Task<UserTopicDto?> GetActualTopicAsync(int sessionId);
    
    /// <summary>
    /// Закрыть пользовательскую тему.
    /// </summary>
    /// <param name="userTopicId">Идентификатор темы.</param>
    /// <returns></returns>
    Task CloseUserTopicAsync(int userTopicId);
    
    /// <summary>
    /// Изменение актуальной пользовательской темы.
    /// </summary>
    /// <param name="userTopicId">Идентификатор темы.</param>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns></returns>
    Task RefreshActualTopicInfoAsync(int userTopicId, int sessionId);

    /// <summary>
    /// Обновление информации пользовательской темы.
    /// </summary>
    /// <param name="userTopicId">Идентификатор темы.</param>
    /// <param name="actual">Тема актуальна?</param>
    /// <param name="previous">Тема предыдущая?</param>
    /// <param name="count">Количество вопросов в теме.</param>
    /// <param name="grade">Грейд темы.</param>
    /// <param name="weight">Вес темы.</param>
    /// <returns></returns>
    Task UpdateUserTopicInfoAsync(int userTopicId, bool actual, bool previous, int? count, GradeDto? grade, double? weight = null);

    /// <summary>
    /// Уменьшение количества вопросов в теме.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="userTopicId">Идентификатор темы.</param>
    /// <returns></returns>
    Task ReduceUserTopicQuestionCountAsync(int sessionId, int userTopicId);

    /// <summary>
    /// В теме есть еще вопросы?
    /// </summary>
    /// <param name="userTopicId">Идентификатор темы.</param>
    /// <param name="gradeMax">Максимальный вес темы.</param>
    /// <returns></returns>
    Task<bool> HaveQuestionAsync(int userTopicId, double gradeMax);
}

public class UserTopicService : IUserTopicService
{
    private const int questionCount = 10;
    private readonly ILogger<UserTopicService> _logger;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserTopicRepository _userTopicRepository;
    private readonly IConfigRepository _configRepository;

    public UserTopicService(
        ILogger<UserTopicService> logger,
        ISessionRepository sessionRepository,
        IUserTopicRepository userTopicRepository,
        IConfigRepository configRepository)
    {
        _logger = logger;
        _sessionRepository = sessionRepository;
        _userTopicRepository = userTopicRepository;
        _configRepository = configRepository;
    }
    
    public async Task CreateUserTopicAsync(int sessionId, int directionId)
    {
        var session = await _sessionRepository.GetSessionAsync(sessionId);
        if (session == null)
        {
            _logger.LogError("Session not found");
            return;
        }
        
        // Получить грейд "Middle"
        var grades = await _configRepository.GetGradesAsync();
        var middleGrade = grades.SingleOrDefault(g => g.Code == "Middle");
        if (middleGrade == null)
        {
            _logger.LogError("Middle grade not found");
            return;
        }
        
        // Получить веса для грейда
        var weights = await _configRepository.GetWeightsAsync();
        var weight = weights.SingleOrDefault(w => w.Grade == middleGrade);
        if (weight == null)
        {
            _logger.LogError($"Weight for \"{middleGrade.Code}\" grade not found");
            return;
        }
        
        // Получить все темы для направления
        var allTopics = await _configRepository.GetTopicsAsync();
        var topics = allTopics.Where(t => t.Direction.Id == directionId).ToList();
        
        // Получить количество вопросов для темы.
        var counts = await _configRepository.GetCountsAsync();
        var questionCountSetting = counts.FirstOrDefault(c => c.Code == "Question");
        
        var firstActual = true;
        foreach (var topic in topics)
        {
            await _userTopicRepository.CreateUserTopicAsync(session, topic, weight.Min, middleGrade, false, false, firstActual, questionCountSetting?.Value ?? questionCount);
            firstActual = false;
        }
    }

    public async Task<List<UserTopicDto>> GetUserTopicsAsync(int sessionId)
    {
        var userTopics = await _userTopicRepository.GetUserTopicsAsync(sessionId);
        return userTopics.Select(userTopic => new UserTopicDto(userTopic.Id, userTopic.Session.Id,
                new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
                new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
                userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count))
            .ToList();
    }

    public async Task<List<UserTopicDto>> GetFinishUserTopicsAsync(int sessionId)
    {
        var userTopics = await _userTopicRepository.GetUserTopicsAsync(sessionId, true);
        return userTopics.Select(userTopic => new UserTopicDto(userTopic.Id, userTopic.Session.Id,
                new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
                new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
                userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count))
            .ToList();
    }

    public async Task<UserTopicDto?> GetUserTopicAsync(int sessionId, int userTopicId)
    {
        var userTopics = await _userTopicRepository.GetUserTopicsAsync(sessionId);
        var userTopic = userTopics.SingleOrDefault(t => t.Id == userTopicId);
        return userTopic == null ? null : new UserTopicDto(userTopic.Id, userTopic.Session.Id,
            new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
            new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
            userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count);
    }

    public async Task<bool> HasOpenTopic(int sessionId)
    {
        var userTopics = await _userTopicRepository.GetUserTopicsAsync(sessionId);
        return userTopics.Any(t => t.IsFinished == false);
    }

    public async Task<UserTopicDto?> GetRandomUserTopicAsync(int sessionId)
    {
        var userTopics = await _userTopicRepository.GetUserTopicsAsync(sessionId);
        if (!userTopics.Any())
        {
            return null;
        }
        
        var userTopic = userTopics.Count == 1 ? userTopics.Single() : userTopics.Where(t => t.WasPrevious == false).MinBy(_ => Guid.NewGuid());
        return userTopic == null ? null : new UserTopicDto(userTopic.Id, userTopic.Session.Id,
            new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
            new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
            userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count);
    }

    public async Task<UserTopicDto?> GetActualTopicAsync(int sessionId)
    {
        var userTopics = await _userTopicRepository.GetUserTopicsAsync(sessionId);
        if (!userTopics.Any())
        {
            return null;
        }

        var userTopic = userTopics.FirstOrDefault(t => t is { IsFinished: false, Actual: true });
        return userTopic == null ? null : new UserTopicDto(userTopic.Id, userTopic.Session.Id,
            new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
            new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
            userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count);
    }

    public async Task CloseUserTopicAsync(int userTopicId)
    {
        await _userTopicRepository.CloseTopicAsync(userTopicId);
    }

    public async Task RefreshActualTopicInfoAsync(int userTopicId, int sessionId)
    {
        await _userTopicRepository.RefreshActualTopicInfoAsync(userTopicId, sessionId);
    }

    public async Task UpdateUserTopicInfoAsync(int userTopicId, bool actual, bool previous, int? count, GradeDto? grade, double? weight = null)
    {
        var userTopic = await _userTopicRepository.GetUserTopicAsync(userTopicId);
        if (userTopic is not null)
        {
            userTopic.Actual = actual;
            userTopic.WasPrevious = previous;
            if (weight is not null)
            {
                userTopic.Weight = weight.Value;
            }

            if (grade is not null)
            {
                var grades = await _configRepository.GetGradesAsync();

                var prev = grades.FirstOrDefault(g => g.Code == grade.Code);
                if (prev is not null)
                {
                    userTopic.Grade = prev;
                }
            }

            if (count is not null)
            {
                userTopic.Count = count.Value;
            }

            await _userTopicRepository.UpdateTopicInfoAsync(userTopic);
        }
    }

    public async Task ReduceUserTopicQuestionCountAsync(int sessionId, int userTopicId)
    {
        var userTopics = await _userTopicRepository.GetUserTopicsAsync(sessionId);
        var userTopic = userTopics.SingleOrDefault(t => t.Id == userTopicId);
        if (userTopic is not null)
        {
            var userTopicCount = userTopic.Count;
            var count = userTopicCount - 1;
            await UpdateUserTopicInfoAsync(userTopicId, userTopic.Actual, userTopic.WasPrevious, count, null);
        }
    }

    public async Task<bool> HaveQuestionAsync(int userTopicId, double gradeMax)
    {
        return await _userTopicRepository.HaveQuestionAsync(userTopicId, gradeMax);
    }
}
