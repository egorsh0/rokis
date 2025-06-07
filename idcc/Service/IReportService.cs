using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Repository.Interfaces;
using idcc.Service;

namespace idcc.Application.Interfaces;

public interface IReportService
{
    Task<ReportDto?> GenerateAsync(SessionDto session);
    Task<List<List<FinalTopicData>>> GetAllTopicDataAsync(SessionDto session);
}

public class ReportService : IReportService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IDataRepository _dataRepository;
    private readonly IUserTopicRepository _userTopicRepository;
    private readonly IUserAnswerRepository _userAnswerRepository;
    private readonly IScoreCalculate _scoreCalculate;
    private readonly IMetricService _metricService;

    private ILogger<ReportService> _logger;

    public ReportService(
        ISessionRepository sessionRepository,
        IDataRepository dataRepository,
        IUserTopicRepository userTopicRepository,
        IUserAnswerRepository userAnswerRepository,
        IScoreCalculate scoreCalculate,
        IMetricService metricService,
        ILogger<ReportService> logger)
    {
        _sessionRepository = sessionRepository;
        _dataRepository = dataRepository;
        _userTopicRepository = userTopicRepository;
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
        var finalTopicDatas = await CalculateFinalTopicDataAsync(session);
        _logger.LogInformation($"Финальный score для тем сгенерирован");
        
        var questions = await _userAnswerRepository.GetQuestionResults(session);
        var cognitiveStabilityIndex = _metricService.CalculateCognitiveStability(questions);
        var thinkingPattern = _metricService.DetectThinkingPattern(questions, cognitiveStabilityIndex);
        
        var report = new ReportDto(session.Token.Id, session.StartTime, session.EndTime!.Value,
            session.EndTime.Value - session.StartTime, cognitiveStabilityIndex, thinkingPattern, finalScoreDto, finalTopicDatas);
        return report;
    }
    
    public async Task<List<List<FinalTopicData>>> GetAllTopicDataAsync(SessionDto session)
    {
        var list = new List<List<FinalTopicData>>();
        
        var allCloseSessions = await _sessionRepository.GetCloseSessionsAsync(session.Token.DirectionId);
        foreach (var closeSession in allCloseSessions)
        {
            var userTopics = await _userTopicRepository.GetAllTopicsAsync(closeSession.Id);
            var userAnswers = await _userAnswerRepository.GetAllUserAnswers(session.Id);

            var finalTopicDatas = new List<FinalTopicData>();
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
                finalTopicDatas.Add(finalTopicData);
            }
            list.Add(finalTopicDatas);
        }
        
        return list;
    }

    private async Task<FinalScoreDto?> CalculateFinalScoreAsync(SessionDto session)
    {
        var userTopics = await _userTopicRepository.GetAllTopicsAsync(session.Id);
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

        var (_, grade) = await _dataRepository.GetGradeLevelAsync(sum);

        return new FinalScoreDto(sum, grade.Name);
    }
    
    private async Task<List<FinalTopicData>?> CalculateFinalTopicDataAsync(SessionDto session)
    {
        var userTopics = await _userTopicRepository.GetAllTopicsAsync(session.Id);
        if (!userTopics.Any())
        {
            return null;
        }
        
        var userAnswers = await _userAnswerRepository.GetAllUserAnswers(session.Id);

        var finalTopicDatas = new List<FinalTopicData>();
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
            finalTopicDatas.Add(finalTopicData);
        }

        return finalTopicDatas;
    }
}