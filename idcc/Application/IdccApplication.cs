using idcc.Application.Interfaces;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Repository.Interfaces;

namespace idcc.Application;

public class IdccApplication : IIdccApplication
{
    private ILogger<IdccApplication> _logger;
    
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

    public IdccApplication(
        ILogger<IdccApplication> logger,
        IUserTopicRepository userTopicRepository,
        IDataRepository dataRepository,
        IQuestionRepository questionRepository,
        IUserAnswerRepository userAnswerRepository,
        IScoreCalculate scoreCalculate,
        ITimeCalculate timeCalculate,
        IWeightCalculate weightCalculate,
        IGradeCalculate gradeCalculate)
    {
        _logger = logger;
        _userTopicRepository = userTopicRepository;
        _dataRepository = dataRepository;
        _questionRepository = questionRepository;
        _userAnswerRepository = userAnswerRepository;
        _scoreCalculate = scoreCalculate;
        _timeCalculate = timeCalculate;
        _weightCalculate = weightCalculate;
        _gradeCalculate = gradeCalculate;
    }

    public async Task<string?> CalculateScoreAsync(Session session, int interval, int questionId, List<int> answerIds)
    {
        var question = await _questionRepository.GetQuestionAsync(questionId);
        if (question is null)
        {
            return ErrorMessages.QUESTION_IS_NULL;
        }
        
        _weight = question.Weight;
        
        if (!answerIds.Any())
        {
            _logger.LogInformation($"Список ответов для сессии {session.Id} и вопроса {questionId} пустой.");
            
            _isCorrectAnswer = false;
            await _userAnswerRepository.CreateUserAnswerAsync(session, question,
                interval, 0, DateTime.Now);
        
            return null;
        }
        
        if (!question.IsMultipleChoice && answerIds.Count > 1)
        {
            return ErrorMessages.QUESTION_IS_NOT_MULTIPLY;
        }
        
        // Посчитать коэффицент времени K
        
        var actualTopic = await _userTopicRepository.GetActualTopicAsync(session);
        if (actualTopic is null)
        {
            return ErrorMessages.ACTUAL_TOPIC_IS_NULL;
        }

        var times = await _dataRepository.GetGradeTimeInfoAsync(actualTopic.Grade.Id);
        if (times is null)
        {
            return ErrorMessages.GRADE_TIMES_IS_NULL(actualTopic.Grade.Name);
        }

        var k = _timeCalculate.K(interval, times.Value.average, times.Value.min, times.Value.max);
    
        // Посчитать и сохранить Score за ответ

        var answers = await _questionRepository.GetAnswersAsync(question);
        var answeredCount = (from userAnswerId in answerIds let ans = answers where ans.Any(a => a.Id == userAnswerId && a.IsCorrect) select userAnswerId).Count();

        var totalCount = answers.Count(a => a.IsCorrect);
        
        var score = _scoreCalculate.GetScore(_weight, k, answeredCount, totalCount);
        _isCorrectAnswer = score > 0;

        _logger.LogInformation($"Сессия: {session.Id}; Вопрос: {questionId}; Коэффицент времени K: {k}; Score: {score}.");
        await _userAnswerRepository.CreateUserAnswerAsync(session, question,
            interval, score, DateTime.Now);
        
        return null;
    }

    public async Task<string?> CalculateTopicWeightAsync(Session session)
    {
        var actualTopic = await _userTopicRepository.GetActualTopicAsync(session);
        if (actualTopic is null)
        {
            return ErrorMessages.ACTUAL_TOPIC_IS_NULL;
        }
        
        var gradeWeights = await _dataRepository.GetGradeWeightInfoAsync(actualTopic.Grade.Id);
        if (gradeWeights is null)
        {
            return ErrorMessages.GRADE_WEIGHT_IS_NULL(actualTopic.Grade.Name);
        }

        var gainWeightPersent = await _dataRepository.GetPercentOrDefaultAsync("GainWeight", 0.2);
        var lessWeightPersent = await _dataRepository.GetPercentOrDefaultAsync("LessWeight", 0.1);
        var newWeight = _weightCalculate.GetNewWeight(actualTopic.Weight, _weight, gradeWeights.Value.max, gainWeightPersent, lessWeightPersent, _isCorrectAnswer);
        // Сравнить прошлый вес и текущий

        var raisePercent = await _dataRepository.GetPercentOrDefaultAsync("RaiseData", 0.2);
        var raiseValue = (gradeWeights.Value.max - gradeWeights.Value.min) * raisePercent;
        
        var raiseCount = await _dataRepository.GetCountOrDefaultAsync("Raise", 3);
        var canRaise = false;
        if (newWeight >= (gradeWeights.Value.max - raiseValue))
        {
            canRaise =
                await _userAnswerRepository.CanRaiseAsync(session, raiseCount);
        }
        // Junior -> Middle => конец топика, фикс веса

        // Senior -> Middle => конец топика, фикс веса
        var (prev, next) = await _dataRepository.GetRelationAsync(actualTopic.Grade);
        var grade = _gradeCalculate.Calculate(actualTopic.Grade, gradeWeights.Value.min, prev, next, newWeight, canRaise);
        if (grade is null)
        {
            _logger.LogInformation($"Сессия: {session.Id}; Топик: {actualTopic.Id}; Статус: Закрыт.");
            await _userTopicRepository.CloseTopicAsync(actualTopic.Id);
            return null;
        }

        if (grade == actualTopic.Grade)
        {
            _logger.LogInformation($"Сессия: {session.Id}; Топик: {actualTopic.Id}; Статус: Сохранение грейда {grade.Name}.");
            await _userTopicRepository.UpdateTopicInfoAsync(actualTopic.Id, false, true, actualTopic.Grade, newWeight);
            
            await ReduceTopicQuestionCountAndCloseTopic(session, actualTopic.Id);
            var count = await _userTopicRepository.CountQuestionAsync(actualTopic.Id, gradeWeights.Value.max);
            if (count == 0)
            {
                await _userTopicRepository.CloseTopicAsync(actualTopic.Id);
            }
            return null;
        }

        var weight = await _dataRepository.GetGradeWeightInfoAsync(grade.Id);
        if (weight is null)
        {
            return ErrorMessages.GRADE_WEIGHT_IS_NULL(grade.Name);
        }
        var raiseLevel = await _dataRepository.GetPercentOrDefaultAsync("RaiseLevel", 0.3);

        var gradeWeight = (weight.Value.min * raiseLevel) + weight.Value.min;
        
        _logger.LogInformation($"Сессия: {session.Id}; Топик: {actualTopic.Id}; Статус: Смена грейда {actualTopic.Grade.Name} на {grade.Name} с весом {gradeWeight}.");

        await _userTopicRepository.UpdateTopicInfoAsync(actualTopic.Id, false, true, grade, gradeWeight);
        
        await ReduceTopicQuestionCountAndCloseTopic(session, actualTopic.Id);

        return null;
    }

    private async Task ReduceTopicQuestionCountAndCloseTopic(Session session, int id)
    {
        await _userTopicRepository.ReduceTopicQuestionCountAsync(id);
        var topic = await _userTopicRepository.GetTopicAsync(id);
        if (topic is not null && topic.Count == 0)
        {
            _logger.LogInformation($"Сессия: {session.Id}; Топик: {id}; Статус: Закрытие по количеству вопросов.");
            await _userTopicRepository.CloseTopicAsync(id);
        }
    }
}