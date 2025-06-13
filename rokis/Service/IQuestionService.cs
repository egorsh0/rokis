using rokis.Dtos;
using rokis.Dtos.AdminDto;
using rokis.Repository;
using Microsoft.Extensions.Caching.Hybrid;

namespace rokis.Service;

public interface IQuestionService
{
    public Task<QuestionSmartDto?> GetQuestionAsync(int questionId);
    Task<List<AnswerDto>> GetAnswersAsync(int questionId);
    public Task<QuestionDto?> GetQuestionAsync(int sessionId, int userTopicId, int gradeId);
    public Task<List<string>> CreateAsync(List<QuestionAdminDto> questions);
}

public class QuestionService : IQuestionService
{
    private readonly ILogger<QuestionService> _logger;
    private readonly HybridCache _hybridCache;
    private readonly IQuestionRepository _questionRepository;
    private readonly IUserAnswerRepository _userAnswerRepository;
    private readonly IUserTopicRepository _userTopicRepository;
    private readonly IConfigService _configService;

    public QuestionService(
        ILogger<QuestionService> logger,
        HybridCache hybridCache,
        IQuestionRepository questionRepository,
        IUserAnswerRepository userAnswerRepository,
        IUserTopicRepository userTopicRepository,
        IConfigService configService)
    {
        _logger = logger;
        _hybridCache = hybridCache;
        _questionRepository = questionRepository;
        _userAnswerRepository = userAnswerRepository;
        _userTopicRepository = userTopicRepository;
        _configService = configService;
    }


    public async Task<QuestionSmartDto?> GetQuestionAsync(int questionId)
    {
        _logger.LogInformation("GetQuestionAsync");
        var cachekey = $"{nameof(QuestionService)}.{nameof(GetQuestionAsync)}.{questionId}";
        return await _hybridCache.GetOrCreateAsync<QuestionSmartDto?>(cachekey, 
            async _ => await _questionRepository.GetQuestionAsync(questionId));
    }
    
    public async Task<List<AnswerDto>> GetAnswersAsync(int questionId)
    {
        _logger.LogInformation("GetAnswersAsync");
        var cachekey = $"{nameof(QuestionService)}.{nameof(GetAnswersAsync)}.{questionId}";
        return await _hybridCache.GetOrCreateAsync<List<AnswerDto>>(cachekey, 
            async _ => await _questionRepository.GetAnswersAsync(questionId));
    }

    public async Task<QuestionDto?> GetQuestionAsync(int sessionId, int userTopicId, int gradeId)
    {
        var userTopic = await _userTopicRepository.GetUserTopicAsync(userTopicId);
        if (userTopic == null)
        {
            return null;
        }
        
        var topic = await _configService.GetTopicAsync(userTopic.Topic.Id);
        if (topic == null)
        {
            return null;
        }
        
        var weight = await _configService.GetWeightsAsync(gradeId);
        if (weight is null)
        {
            return null;
        }
        
        var answeredQuestions = await _userAnswerRepository.GetUserAnswersAsync(sessionId, topic.Id);

        var answeredIds = answeredQuestions.Select(a => a.Question.Id).ToList();
        
        var question = await _questionRepository.GetQuestionAsync(answeredIds, topic.Id, userTopic.Weight, weight.Max);
        
        if (question is null)
        {
            return null;
        }

        var answers = await _questionRepository.GetAnswersAsync(question.Id);
        if (!answers.Any())
        {
            return null;
        }
        
        var questionAnswers = answers.Select(a => new AnswerDto()
        {
            Id = a.Id,
            IsCorrect = a.IsCorrect,
            Content = a.Content
        }).OrderBy(_ => Guid.NewGuid()).ToList();
        
        var dto = new QuestionDto()
        {
            Id = question.Id,
            Topic = topic.Name,
            Content = question.Content,
            Answers = questionAnswers
        };

        return dto;
    }

    public async Task<List<string>> CreateAsync(List<QuestionAdminDto> questions)
    {
        return await _questionRepository.CreateAsync(questions);
    }
}