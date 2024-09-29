using idcc.Application.Interfaces;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Repository.Interfaces;

namespace idcc.Application;

public class IdccApplication : IIdccApplication
{
    private IUserTopicRepository _userTopicRepository;
    private IDataRepository _dataRepository;
    private IQuestionRepository _questionRepository;
    private IUserAnswerRepository _userAnswerRepository;
    private IScoreCalculate _scoreCalculate;
    private ITimeCalculate _timeCalculate;
    private IWeightCalculate _weightCalculate;
    private IGradeCalculate _gradeCalculate;

    private bool _isCorrectAnswer;
    private double _weight;

    public IdccApplication(IUserTopicRepository userTopicRepository,
        IDataRepository dataRepository,
        IQuestionRepository questionRepository,
        IUserAnswerRepository userAnswerRepository,
        IScoreCalculate scoreCalculate,
        ITimeCalculate timeCalculate,
        IWeightCalculate weightCalculate,
        IGradeCalculate gradeCalculate)
    {
        _userTopicRepository = userTopicRepository;
        _dataRepository = dataRepository;
        _questionRepository = questionRepository;
        _userAnswerRepository = userAnswerRepository;
        _scoreCalculate = scoreCalculate;
        _timeCalculate = timeCalculate;
        _weightCalculate = weightCalculate;
        _gradeCalculate = gradeCalculate;
    }

    public async Task<string?> CalculateScoreAsync(Session session, int userId, int interval, int questionId, List<int> answerIds)
    {
        var question = await _questionRepository.GetQuestionAsync(questionId);
        if (question is null)
        {
            return ErrorMessage.QUESTION_IS_NULL;
        }
        
        _weight = question.Question.Weight;
        
        if (!answerIds.Any())
        {
            _isCorrectAnswer = false;
            await _userAnswerRepository.CreateUserAnswerAsync(session, question.Question,
                interval, 0, DateTime.Now);
        
            return null;
        }
        
        if (!question.Question.IsMultipleChoice && answerIds.Count > 1)
        {
            return ErrorMessage.QUESTION_IS_NOT_MULTIPLY;
        }
        
        // Посчитать коэффицент времени K
        
        var actualTopic = await _userTopicRepository.GetActualTopicAsync(userId);
        if (actualTopic is null)
        {
            return ErrorMessage.ACTUAL_TOPIC_IS_NULL;
        }

        var times = await _dataRepository.GetGradeTimeInfoAsync(actualTopic.Grade.Id);
        if (times is null)
        {
            return ErrorMessage.GRADE_TIMES_IS_NULL(actualTopic.Grade.Name);
        }

        var k = _timeCalculate.K(interval, times.Value.average, times.Value.min, times.Value.max);
    
        // Посчитать и сохранить Score за ответ

        var answeredCount = (from userAnswerId in answerIds let answers = question.Answers where answers.Any(_ => _.Id == userAnswerId && _.IsCorrect) select userAnswerId).Count();

        var totalCount = question.Answers.Count(_ => _.IsCorrect);
        

        var score = _scoreCalculate.GetScore(_weight, k, answeredCount, totalCount);
        _isCorrectAnswer = score > 0;
        await _userAnswerRepository.CreateUserAnswerAsync(session, question.Question,
            interval, score, DateTime.Now);
        
        return null;
    }

    public async Task<string?> CalculateTopicWeightAsync(Session session, int userId)
    {
        var actualTopic = await _userTopicRepository.GetActualTopicAsync(userId);
        if (actualTopic is null)
        {
            return ErrorMessage.ACTUAL_TOPIC_IS_NULL;
        }
        
        var gradeWeight = await _dataRepository.GetGradeWeightInfoAsync(actualTopic.Grade.Id);
        if (gradeWeight is null)
        {
            return ErrorMessage.GRADE_WEIGHT_IS_NULL(actualTopic.Grade.Name);
        }

        var gainWeightPersent = await _dataRepository.GetPercentOrDefaultAsync("GainWeight", 0.2);
        var lessWeightPersent = await _dataRepository.GetPercentOrDefaultAsync("LessWeight", 0.1);
        var newWeight = _weightCalculate.GetNewWeight(actualTopic.Weight, _weight, gradeWeight.Value.max, gainWeightPersent, lessWeightPersent, _isCorrectAnswer);
        // Сравнить прошлый вес и текущий

        var raisePercent = await _dataRepository.GetPercentOrDefaultAsync("RaiseData", 0.2);
        var raiseValue = (gradeWeight.Value.max - gradeWeight.Value.min) * raisePercent;
        
        var raiseCount = await _dataRepository.GetCountOrDefaultAsync("Raise", 3);
        var canRaise = false;
        if (actualTopic.Weight >= (gradeWeight.Value.max - raiseValue))
        {
            canRaise =
                await _userAnswerRepository.CanRaiseAsync(session.Id, raiseCount);
        }
        // Junior -> Middle => конец топика, фикс веса

        // Senior -> Middle => конец топика, фикс веса
        var (prev, next) = await _dataRepository.GetRelationAsync(actualTopic.Grade);
        var grade = _gradeCalculate.Calculate(actualTopic.Grade, gradeWeight.Value.min, prev, next, newWeight, canRaise);
        if (grade is null)
        {
            await _userTopicRepository.CloseTopicAsync(actualTopic.Id);
            
            await ReduceTopicQuestionCountAndCloseTopic(actualTopic.Id);
            return null;
        }

        if (grade == actualTopic.Grade)
        {
            await _userTopicRepository.UpdateTopicInfoAsync(actualTopic.Id, false, true, actualTopic.Grade, newWeight);
            
            await ReduceTopicQuestionCountAndCloseTopic(actualTopic.Id);
            return null;
        }

        var weight = await _dataRepository.GetGradeWeightInfoAsync(grade.Id);
        if (weight is null)
        {
            return ErrorMessage.GRADE_WEIGHT_IS_NULL(grade.Name);
        }
        var raiseLevel = await _dataRepository.GetPercentOrDefaultAsync("RaiseLevel", 0.3);
        await _userTopicRepository.UpdateTopicInfoAsync(actualTopic.Id, false, true, grade, (weight.Value.min * raiseLevel) + weight.Value.min);
        
        await ReduceTopicQuestionCountAndCloseTopic(actualTopic.Id);
        
        return null;
    }

    private async Task ReduceTopicQuestionCountAndCloseTopic(int id)
    {
        await _userTopicRepository.ReduceTopicQuestionCountAsync(id);
        var topic = await _userTopicRepository.GetTopicAsync(id);
        if (topic is not null && topic.Count == 0)
        {
            await _userTopicRepository.CloseTopicAsync(id);
        }
    }
}