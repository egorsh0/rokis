using idcc.Application.Interfaces;
using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Repository.Interfaces;

namespace idcc.Application;

public class IdccReport : IIdccReport
{
    private IDataRepository _dataRepository;
    private IUserTopicRepository _userTopicRepository;
    private IUserAnswerRepository _userAnswerRepository;
    private IScoreCalculate _scoreCalculate;

    private ILogger<IdccReport> _logger;

    public IdccReport(
        IDataRepository dataRepository,
        IUserTopicRepository userTopicRepository,
        IUserAnswerRepository userAnswerRepository,
        IScoreCalculate scoreCalculate,
        ILogger<IdccReport> logger)
    {
        _dataRepository = dataRepository;
        _userTopicRepository = userTopicRepository;
        _userAnswerRepository = userAnswerRepository;
        _scoreCalculate = scoreCalculate;
        _logger = logger;
    }
    
    public async Task<ReportDto?> GenerateAsync(Session session)
    {
        _logger.LogInformation("Создание обьекта отчета");
        var report = new ReportDto
        {
            Name = session.PersonProfile.FullName,
            StartSession = session.StartTime,
            EndSession = session.EndTime!.Value,
            TestingTime = (session.StartTime - session.EndTime.Value)
        };

        _logger.LogInformation("Генерация финального Score");
        var finalScoreDto = await CalculateFinalScoreAsync(session);

        _logger.LogInformation($"Финальный score: {finalScoreDto}");
        
        report.FinalScoreDto = finalScoreDto;
        
        _logger.LogInformation("Генерация score для тем");
        var finalTopicDatas = await CalculateFinalTopicDataAsync(session);

        _logger.LogInformation($"Финальный score для тем сгенерирован");
        report.FinalTopicDatas = finalTopicDatas;
        
        return report;
    }

    private async Task<FinalScoreDto?> CalculateFinalScoreAsync(Session session)
    {
        var userTopics = await _userTopicRepository.GetAllTopicsAsync(session);
        if (userTopics.Count == 0)
        {
            return null;
        }

        var userAnswers = await _userAnswerRepository.GetAllUserAnswers(session);

        var sum = 0.0;
        foreach (var userTopic in userTopics)
        {
            var scores = userAnswers.Where(_ => _.Question.Topic == userTopic.Topic).Select(_ => _.Score).ToList();
            var weight = userTopic.Weight;
            var topicScore = _scoreCalculate.GetTopicScore(scores, weight);
            sum += topicScore;
        }

        var (_, grade) = await _dataRepository.GetGradeLevelAsync(sum);

        return new FinalScoreDto()
        {
            Score = sum,
            Grade = grade.Name
        };
    }
    
    private async Task<List<FinalTopicData>?> CalculateFinalTopicDataAsync(Session session)
    {
        var userTopics = await _userTopicRepository.GetAllTopicsAsync(session);
        if (!userTopics.Any())
        {
            return null;
        }
        
        var userAnswers = await _userAnswerRepository.GetAllUserAnswers(session);

        var finalTopicDatas = new List<FinalTopicData>();
        foreach (var userTopic in userTopics)
        {
            var questionAnswers = userAnswers.Where(a => a.Question.Topic == userTopic.Topic).ToList();
            var scores = questionAnswers.Select(a => a.Score).ToList();
            var weight = userTopic.Weight;
            var topicScore = _scoreCalculate.GetTopicScore(scores, weight);
            var positive = questionAnswers.Count(_ => _.Score > 0);
            var negative = questionAnswers.Count(_ => _.Score == 0);

            var finalTopicData = new FinalTopicData()
            {
                Topic = userTopic.Topic.Name,
                Score = topicScore == 0 ? ValueConst.MinValue : topicScore,
                Positive = positive,
                Negative = negative
            };
            finalTopicDatas.Add(finalTopicData);
        }

        return finalTopicDatas;
    }
}