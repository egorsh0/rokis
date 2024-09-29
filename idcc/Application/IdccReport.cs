using idcc.Application.Interfaces;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;

namespace idcc.Application;

public class IdccReport : IIdccReport
{
    private IDataRepository _dataRepository;
    private IUserTopicRepository _userTopicRepository;
    private IUserAnswerRepository _userAnswerRepository;
    private IScoreCalculate _scoreCalculate;

    public IdccReport(
        IDataRepository dataRepository,
        IUserTopicRepository userTopicRepository,
        IUserAnswerRepository userAnswerRepository,
        IScoreCalculate scoreCalculate)
    {
        _dataRepository = dataRepository;
        _userTopicRepository = userTopicRepository;
        _userAnswerRepository = userAnswerRepository;
        _scoreCalculate = scoreCalculate;
    }
    
    public async Task<ReportDto?> GenerateAsync(Session session)
    {
        var report = new ReportDto
        {
            Name = session.User.UserName,
            StartSession = session.StartTime,
            EndSession = session.EndTime!.Value,
            TestingTime = (session.StartTime - session.EndTime.Value)
        };

        var finalScoreDto = await CalculateFinalScoreAsync(session);

        report.FinalScoreDto = finalScoreDto;
        
        var finalTopicDatas = await CalculateFinalTopicDataAsync(session);

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
            var scores = userAnswers.Where(_ => _.Question.Topic == userTopic.Topic).Select(_ => _.Score).ToList();
            var weight = userTopic.Weight;
            var topicScore = _scoreCalculate.GetTopicScore(scores, weight);
            var positive = userAnswers.Count(_ => _.Score > 0);
            var negative = userAnswers.Count(_ => _.Score == 0);

            var finalTopicData = new FinalTopicData()
            {
                Topic = userTopic.Topic.Name,
                Score = topicScore,
                Positive = positive,
                Negative = negative
            };
            finalTopicDatas.Add(finalTopicData);
        }

        return finalTopicDatas;
    }
}