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

    public async Task<string?> CalculateScoreAsync(Session session, int userId, int interval, int questionId, IEnumerable<int> answerIds)
    {
        // Посчитать коэффицент времени K
        var actualTopic = await _userTopicRepository.GetActualTopicAsync(userId);
        if (actualTopic is null)
        {
            return ErrorMessage.ACTUAL_TOPIC_IS_NULL;
        }

        var times = await _dataRepository.GetGradeTimeInfoAsync(actualTopic.Current.Id);
        if (times is null)
        {
            return ErrorMessage.GRADE_TIMES_IS_NULL(actualTopic.Current.Name);
        }

        var k = _timeCalculate.K(interval, times.Value.average, times.Value.min, times.Value.max);
    
        // Посчитать и сохранить Score за ответ
        var question = await _questionRepository.GetQuestionAsync(questionId);
        if (question is null)
        {
            return ErrorMessage.QUESTION_IS_NULL;
        }
        var answeredCount = (from userAnswerId in answerIds let answers = question.Answers where answers.Any(_ => _.Id == userAnswerId && _.IsCorrect) select userAnswerId).Count();
        var totalCount = question.Answers.Count(_ => _.IsCorrect);
        var weight = question.Question.Weight;

        var score = _scoreCalculate.GetScore(weight, k, answeredCount, totalCount);
        _isCorrectAnswer = score > 0;
        await _userAnswerRepository.CreateUserAnswerAsync(session, question.Question, question.Answers,
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
        
        var gradeWeight = await _dataRepository.GetGradeWeightInfoAsync(actualTopic.Current.Id);
        if (gradeWeight is null)
        {
            return ErrorMessage.GRADE_WEIGHT_IS_NULL(actualTopic.Current.Name);
        }

        var gainWeightPersent = await _dataRepository.GetPercentOrDefaultAsync("GainWeight", 0.2);
        var lessWeightPersent = await _dataRepository.GetPercentOrDefaultAsync("LessWeight", 0.1);
        var newWeight = _weightCalculate.GetNewWeight(actualTopic.Weight, gradeWeight.Value.max, gainWeightPersent, lessWeightPersent, _isCorrectAnswer);
        // Сравнить прошлый вес и текущий

        var raisePercent = await _dataRepository.GetPercentOrDefaultAsync("RaiseData", 0.2);
        var raiseValue = (gradeWeight.Value.max - gradeWeight.Value.min) * raisePercent;
        var canRaise = await _userAnswerRepository.CanRaiseAsync(session.Id, gradeWeight.Value.max - raiseValue, 3);
    
        // Junior -> Middle => конец топика, фикс веса

        // Senior -> Middle => конец топика, фикс веса
        var (prev, next) = await _dataRepository.GetRelationAsync(actualTopic.Current);
        var grade = _gradeCalculate.Calculate(actualTopic.Current, gradeWeight.Value.min, prev, next, newWeight, canRaise);
        if (grade is null)
        {
            await _userTopicRepository.CloseTopicAsync(actualTopic.Id);
            return null;
        }

        if (grade == actualTopic.Current)
        {
            await _userTopicRepository.UpdateTopicInfoAsync(actualTopic.Id, false, true, actualTopic.Current, newWeight);
        }

        var weight = await _dataRepository.GetGradeWeightInfoAsync(grade.Id);
        if (weight is null)
        {
            return ErrorMessage.GRADE_WEIGHT_IS_NULL(grade.Name);
        }
        await _userTopicRepository.UpdateTopicInfoAsync(actualTopic.Id, false, true, grade, weight.Value.min);

        return null;
    }
}