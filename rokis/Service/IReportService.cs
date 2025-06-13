using rokis.Dtos;
using rokis.Infrastructures;
using rokis.Repository;
using rokis.Service;

namespace rokis.Application.Interfaces;

public interface IReportService
{
    Task<ReportDto?> GenerateAsync(SessionDto session);
}

public class ReportService : IReportService
{
    private readonly IConfigService _configService;
    private readonly IUserTopicService _userTopicService;
    private readonly IUserAnswerRepository _userAnswerRepository;
    private readonly IScoreCalculate _scoreCalculate;
    private readonly IMetricService _metricService;

    private ILogger<ReportService> _logger;

    public ReportService(
        IConfigService configService,
        IUserTopicService userTopicService,
        IUserAnswerRepository userAnswerRepository,
        IScoreCalculate scoreCalculate,
        IMetricService metricService,
        ILogger<ReportService> logger)
    {
        _configService = configService;
        _userTopicService = userTopicService;
        _userAnswerRepository = userAnswerRepository;
        _scoreCalculate = scoreCalculate;
        _metricService = metricService;
        _logger = logger;
    }
    
    public async Task<ReportDto?> GenerateAsync(SessionDto session)
    {
        _logger.LogInformation("Создание обьекта отчета");

        _logger.LogInformation("Генерация финального Score");
        var finalScoreDto = await CalculateFinalScoreAsync(session);
        _logger.LogInformation($"Финальный score: {finalScoreDto}");
        
        _logger.LogInformation("Генерация score для тем");
        var finalTopicData = await CalculateFinalTopicDataAsync(session);
        _logger.LogInformation($"Финальный score для тем сгенерирован");
        
        var questions = await _userAnswerRepository.GetQuestionResults(session);
        var cognitiveStabilityIndex = _metricService.CalculateCognitiveStability(questions);
        var thinkingPattern = _metricService.DetectThinkingPattern(questions, cognitiveStabilityIndex);
        
        var report = new ReportDto(session.Token.Id, session.StartTime, session.EndTime!.Value,
            session.EndTime.Value - session.StartTime, cognitiveStabilityIndex, thinkingPattern, finalScoreDto, finalTopicData);
        return report;
    }

    private async Task<FinalScoreDto?> CalculateFinalScoreAsync(SessionDto session)
    {
        var userTopics = await _userTopicService.GetFinishUserTopicsAsync(session.Id);
        if (userTopics.Count == 0)
        {
            return null;
        }

        var userAnswers = await _userAnswerRepository.GetAllUserAnswers(session.Id);

        var sum = 0.0;
        foreach (var userTopic in userTopics)
        {
            var scores = userAnswers.Where(userAnswer => userAnswer.Question.Topic.Id == userTopic.Topic.Id).Select(answer => answer.Score).ToList();
            var weight = userTopic.Weight;
            var topicScore = _scoreCalculate.GetTopicScore(scores, weight);
            sum += topicScore;
        }

        var (_, grade) = await _configService.GetGradeLevelAsync(sum);
        return grade != null ? new FinalScoreDto(sum, grade.Name) : new FinalScoreDto(sum, string.Empty);
    }
    
    private async Task<List<FinalTopicData>?> CalculateFinalTopicDataAsync(SessionDto session)
    {
        var userTopics = await _userTopicService.GetFinishUserTopicsAsync(session.Id);
        if (!userTopics.Any())
        {
            return null;
        }
        
        var userAnswers = await _userAnswerRepository.GetAllUserAnswers(session.Id);

        var topicDataAsync = new List<FinalTopicData>();
        foreach (var userTopic in userTopics)
        {
            var questionAnswers = userAnswers.Where(a => a.Question.Topic.Id == userTopic.Topic.Id).ToList();
            var scores = questionAnswers.Select(a => a.Score).ToList();
            var weight = userTopic.Weight;
            var topicScore = _scoreCalculate.GetTopicScore(scores, weight);
            var positive = questionAnswers.Count(answer => answer.Score > 0);
            var negative = questionAnswers.Count(answer => answer.Score == 0);

            var finalTopicData = new FinalTopicData(
                userTopic.Topic.Name,
                topicScore == 0 ? ValueConst.MinValue : topicScore,
                userTopic.Grade.Name,
                positive, 
                negative);
            topicDataAsync.Add(finalTopicData);
        }

        return topicDataAsync;
    }
}