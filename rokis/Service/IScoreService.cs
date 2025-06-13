using rokis.Dtos;
using rokis.Extensions;
using rokis.Infrastructures;
using rokis.Models;
using rokis.Repository;

namespace rokis.Service;

public interface IScoreService
{
    Task<(MessageCode code, string? error)> CalculateScoreAsync(SessionDto session, int interval, int questionId, List<int> answerIds);
    
    Task<(MessageCode code, string? error)> CalculateTopicWeightAsync(SessionDto session);
}

public class ScoreService : IScoreService
{
    private ILogger<ScoreService> _logger;
    
    private IUserTopicService _userTopicService;
    private IConfigService _configService;
    private IQuestionService _questionService;
    private IUserAnswerRepository _userAnswerRepository;
    private IUserAnswerService _userAnswerService;
    private IScoreCalculate _scoreCalculate;
    private ITimeCalculate _timeCalculate;
    private IGradeCalculate _gradeCalculate;

    private bool _isCorrectAnswer;
    private double _questionWeight;

    public ScoreService(
        ILogger<ScoreService> logger,
        IUserTopicService userTopicService,
        IConfigService configService,
        IQuestionService questionService,
        IUserAnswerRepository userAnswerRepository,
        IUserAnswerService userAnswerService,
        IScoreCalculate scoreCalculate,
        ITimeCalculate timeCalculate,
        IGradeCalculate gradeCalculate)
    {
        _logger = logger;
        _userTopicService = userTopicService;
        _configService = configService;
        _questionService = questionService;
        _userAnswerRepository = userAnswerRepository;
        _userAnswerService = userAnswerService;
        _scoreCalculate = scoreCalculate;
        _timeCalculate = timeCalculate;
        _gradeCalculate = gradeCalculate;
    }

    public async Task<(MessageCode code, string? error)> CalculateScoreAsync(SessionDto session, int interval, int questionId, List<int> answerIds)
    {
        // ───────────────── 1.  Получаем вопрос ─────────────────
        var question = await _questionService.GetQuestionAsync(questionId);
        if (question is null)
        {
            return (MessageCode.QUESTION_IS_NOT_EXIST, MessageCode.QUESTION_IS_NOT_EXIST.GetDescription());
        }

        _questionWeight = question.Weight;
        
        // ───────────────── 2.  Пустой список ответов ────────────
        if (!answerIds.Any())
        {
            _logger.LogInformation($"Список ответов для сессии {session.Id} и вопроса {questionId} пуст.");
            _isCorrectAnswer = false;
            await _userAnswerRepository.CreateUserAnswerAsync(session, question, interval, 0, DateTime.Now);
            return (MessageCode.ANSWER_IS_SEND, null);
        }
        
        // ───────────────── 3.  Одиночный VS множественный ──────
        if (!question.IsMultipleChoice && answerIds.Count > 1)
        {
            return (MessageCode.QUESTION_IS_MULTIPLY, MessageCode.QUESTION_IS_MULTIPLY.GetDescription());
        }
        
        // ───────────────── 4.  Подтягиваем варианты ответа ─────
        var answers = await _questionService.GetAnswersAsync(question.QuestionId);
        if (!answers.Any())
        {
            return (MessageCode.ANSWER_IS_NOT_EXIST, MessageCode.ANSWER_IS_NOT_EXIST.GetDescription());
        }

        // ---------- все ли id присутствуют ----------
        var validIds = answers.Select(a => a.Id).ToHashSet();
        var invalid  = answerIds.Where(id => !validIds.Contains(id)).ToList();

        if (invalid.Any())
        {
            _logger.LogWarning($"Вопрос {questionId}: некорректные id ответов [{string.Join(",", invalid)}]");
            return (MessageCode.ANSWER_ID_NOT_FOUND, MessageCode.ANSWER_ID_NOT_FOUND.GetDescription());   // создайте константу / функцию
        }

        // ───────────────── 5.  Коэффициент времени K ────────────
        var actualTopic = await _userTopicService.GetActualTopicAsync(session.Id);
        if (actualTopic is null)
        {
            return (MessageCode.TOPIC_IS_NULL, MessageCode.TOPIC_IS_NULL.GetDescription());
        }

        var times = await _configService.GetGradeTimeInfoAsync(actualTopic.Grade.Id);
        if (times is null)
        {
            return (MessageCode.GRADE_TIMES_IS_NULL, ErrorMessages.GRADE_TIMES_IS_NULL(actualTopic.Grade.Name));
        }

        var k = _timeCalculate.K(interval, times.Value.average, times.Value.min, times.Value.max);

        // ───────────────── 6.  Подсчёт балла ────────────────────
        var answeredCorrect = answers
            .Count(a => a.IsCorrect && answerIds.Contains(a.Id));

        var totalCorrect = answers.Count(a => a.IsCorrect);

        var score = _scoreCalculate.GetScore(_questionWeight, k, answeredCorrect, totalCorrect);
        _isCorrectAnswer = score > 0;

        _logger.LogInformation($"Сессия:{session.Id}; Вопрос:{questionId}; K:{k}; Score:{score}");

        await _userAnswerRepository.CreateUserAnswerAsync(session, question, interval, score, DateTime.Now);
        return (MessageCode.CALCULATE_IS_FINISHED, null);
    }

    public async Task<(MessageCode code, string? error)> CalculateTopicWeightAsync(SessionDto session)
    {
        var actualTopic = await _userTopicService.GetActualTopicAsync(session.Id);
        if (actualTopic is null)
        {
            return (MessageCode.TOPIC_IS_NULL, MessageCode.TOPIC_IS_NULL.GetDescription());
        }
        
        var gradeWeights = await _configService.GetGradeWeightInfoAsync(actualTopic.Grade.Id);
        if (gradeWeights is null)
        {
            return (MessageCode.GRADE_WEIGHT_IS_NULL, ErrorMessages.GRADE_WEIGHT_IS_NULL(actualTopic.Grade.Name));
        }

        var gradeRelations = await _configService.GetGradeRelationsAsync();
        var gradeRelation = gradeRelations.FirstOrDefault(gr => gr.End == actualTopic.Grade.Code);
        if (gradeRelation == null)
        {
            return (MessageCode.GRADE_RELATIONS_IS_NULL, ErrorMessages.GRADE_RELATIONS_IS_NULL(actualTopic.Grade.Name));
        }

        var isLeftGrade = gradeRelation.Start == null;
        
        // Подсчет нового веса вопроса
        var newWeight = await ChangeGradeAsync(actualTopic.Grade, session.Id, actualTopic.Topic.Id, actualTopic.Weight, _questionWeight, _isCorrectAnswer, isLeftGrade);
        
        // Можно ли быстро закрыть?
        var canClose = await CanCloseAsync(session.Id, actualTopic.Topic.Id);
        
        if (canClose)
        {
            _logger.LogInformation($"Сессия: {session.Id}; Топик {actualTopic.Id}; Тема;{actualTopic.Topic.Id}; Статус: Закрыт. Причина: Много подрят ошибок.");
            
            // Уменьшаем вопросы в теме
            await ReduceTopicQuestionCountAndCloseTopic(session.Id, actualTopic.Id);
            // Обновляем вес темы
            await _userTopicService.UpdateUserTopicInfoAsync(actualTopic.Id, false, true, null, actualTopic.Grade, newWeight);
            // Закрываем тему
            await _userTopicService.CloseUserTopicAsync(actualTopic.Id);
            return (MessageCode.TOPIC_IS_CLOSED,null);
        }
        
        // Можно ли быстро повысить?
        var canRaise = await CanRaiseAsync(session.Id, actualTopic.Topic.Id, gradeWeights.Value.max, gradeWeights.Value.min, newWeight);
        
        // Считаем новый грейд.
        var (prev, next) = await _configService.GetRelationAsync(actualTopic.Grade);
        var grade = _gradeCalculate.Calculate(actualTopic.Grade, gradeWeights.Value.min, prev, next, newWeight, canRaise);
        
        _logger.LogDebug($"Сессия: {session.Id}; Топик: {actualTopic.Id}; Статус грейда: {actualTopic.Grade.Name} -> {grade.Name}.");
        // Обновляем вес темы
        await _userTopicService.UpdateUserTopicInfoAsync(actualTopic.Id, false, true, null, grade, newWeight);
        // Уменьшаем количество вопросов в теме
        await ReduceTopicQuestionCountAndCloseTopic(session.Id, actualTopic.Id);
        // Проверяем количество вопросов в теме
        var count = await _userTopicService.HaveQuestionAsync(actualTopic.Id, gradeWeights.Value.max);
        if (!count)
        {
            await _userTopicService.CloseUserTopicAsync(actualTopic.Id);
        }
        return (MessageCode.CALCULATE_IS_FINISHED, null);
    }

    private async Task<double> ChangeGradeAsync(GradeDto grade, int sessionId, int topicId, double weight, double questionWeight, bool increase, bool isLeftGrade)
    {
        var weights = await _configService.GetGradeWeightInfoAsync(grade.Id);

        var userAnswers = await _userAnswerRepository.GetAllUserAnswers(sessionId, topicId);
        
        // Среднее время ответов на теме
        var avgTime = !userAnswers.Any() ? 0 : userAnswers.Select(ua => ua.TimeSpent).Average();
        
        // Количество правильных ответов на теме
        var correct = userAnswers.Count(ua => ua.Score > 0);
        
        // Границы времен текущего грейда
        var times = await _configService.GetGradeTimeInfoAsync(grade.Id);
        
        // Количество вопросов в теме
        var counts = userAnswers.Count;
        
        // Расчет сложности нового вопроса
        var difficulty = CalculateDifficulty(correct, counts, avgTime, times!.Value.max);

        // Расчет скользящей точности
        var answers = await _userAnswerRepository.GetAllUserAnswers(sessionId, topicId);
        var rollingAcc = await GetRollingAccuracy(answers);
        // Расчет нового веса темы
        var gradeWeight = UpdateWithAsymptoticGrowth(weight, questionWeight, weights!.Value.min, weights.Value.max, increase, difficulty, rollingAcc);
        if (isLeftGrade && gradeWeight < weights.Value.min)
        {
            return weights.Value.min;
        }
        return gradeWeight;
    }
    
    public double CalculateDifficulty(int correct, int total, double avgTimeSeconds, double maxTimeSeconds)
    {
        if (total < 3)
        {
            return 0.5;
        }

        var accuracy = (double)correct / total;
        var timeFactor = Math.Min(avgTimeSeconds / maxTimeSeconds, 1.0);

        return 0.5 * (1 - accuracy) + 0.5 * timeFactor;
    }
    
    double UpdateWithAsymptoticGrowth(
        double weight, 
        double questionWeight, 
        double min, double max, 
        bool increase, 
        double difficulty,
        double rollingAcc)
    {
        const double learningRate = 0.3;
        const double minDamping = 0.09; // минимальная адаптация
        
        // 0) если точность < 0.7, урежаем подъём вдвое
        var accuracyFactor = rollingAcc < 0.7 ? 0.5 : 1.0;
        
        // Асимптотическое затухание при приближении к границам
        var distanceToEdge = increase
            ? max - weight
            : weight - min;

        distanceToEdge = Math.Max(distanceToEdge, minDamping);
        
        var baseDelta = learningRate * distanceToEdge * accuracyFactor;
        double delta;
       
        if (increase)
        {
            // если правильный ответ на очень сложный вопрос → добавляем буст
            delta = ApplyDifficultyBonus(baseDelta, increase, difficulty);
        
            // дополнительный расчет от веса вопроса
            delta = ApplyQuestionWeightInfluence(delta, questionWeight, weight);
        }else
        {
            delta = -baseDelta * (1 - difficulty);
            
            // Дополнительное усиление штрафа, если вопрос был слишком простой
            var penaltyBoost = weight - questionWeight;
            delta *= 4 + penaltyBoost;
        }

        var temp = weight + delta;
        var clamp = Math.Clamp(temp, min, max);
        if (temp < min && Math.Abs(clamp - min) < 0.000001)
        {
            return temp;
        }

        return clamp;
    }
    
    double ApplyDifficultyBonus(double delta, bool increase, double difficulty)
    {
        if (increase && difficulty > 0.7)
        {
            var bonusFactor = (difficulty - 0.7) / 0.3; // нормализация: 0.71 → 0.03, 1.0 → 1.0
            var bonus = delta * 0.5 * bonusFactor;     // до +50% от текущего delta

            delta += bonus;
        }

        return delta;
    }
    
    double ApplyQuestionWeightInfluence(double delta, double questionWeight, double topicWeight)
    {
        var boost = Math.Clamp(questionWeight - topicWeight, -0.1, 0.2);
        return delta * (1.0 + boost);
    }
    
    /// <summary>
    /// Скользящая точность.
    /// </summary>
    /// <param name="answers">Список ответов пользователя.</param>
    /// <returns></returns>
    async Task<double> GetRollingAccuracy(List<UserAnswer> answers)
    {
        var window = await _configService.GetCountOrDefaultAsync("RollingWindow", 5);
        if (answers.Count < window)
        {
            return 1.0;
        }
        var last = answers.TakeLast(window);
        return last.Count(a => a.Score > 0) / (double)window;
    }
    
    /// <summary>
    /// Можно ли быстро повысить уровень?
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="topicId">Идентификатор темы.</param>
    /// <param name="max">Максимальный вес вопроса текущего грейда.</param>
    /// <param name="min">Минимальный вес вопроса текущего грейда.</param>
    /// <param name="newWeight">Новый вес вопроса.</param>
    /// <returns></returns>
    private async Task<bool> CanRaiseAsync(int sessionId, int topicId, double max, double min, double newWeight)
    {
        // Разница в весах
        var raisePercent = await _configService.GetPercentOrDefaultAsync("RaiseData", 0.2);
        var raiseValue = (max - min) * raisePercent;
        
        // Разница в количестве подрят
        var increasePercent = await _configService.GetPercentOrDefaultAsync("IncreaseLevel", 0.3);
        var allCount = await _configService.GetCountOrDefaultAsync("Question", 10);
        var raiseCount = (int)Math.Floor(allCount * increasePercent);
        var canRaise = await _userAnswerService.CanRaiseTopicAsync(sessionId, topicId, raiseCount);
        
        if (newWeight >= (max - raiseValue) || canRaise)
        {
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Можно ли быстро закрыть топик?
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="topicId">Идентификатор темы.</param>
    /// <returns></returns>
    private async Task<bool> CanCloseAsync(int sessionId, int topicId)
    {
        var decreasePercent = await _configService.GetPercentOrDefaultAsync("DecreaseLevel", 0.3);
        var allCount = await _configService.GetCountOrDefaultAsync("Question", 10);
        var mandatoryQuestionPercent = await _configService.GetPercentOrDefaultAsync("MandatoryQuestions", 0.5);
        var closeCount = (int)Math.Floor(allCount * decreasePercent);
        var mandatoryCount = (int)Math.Floor(allCount * mandatoryQuestionPercent);
        var canClose = await _userAnswerService.CanCloseTopicAsync(sessionId, topicId, closeCount);
        var userAnswers = await _userAnswerService.CountUserAnswersAsync(sessionId, topicId);
        if (canClose && userAnswers >= mandatoryCount)
        {
            _logger.LogInformation($"Сессия: {sessionId}; Топик: {topicId}; Статус: можно быстро закрыть.");
            return true;
        }
        return false;
    }
    
    private async Task ReduceTopicQuestionCountAndCloseTopic(int sessionId, int userTopicId)
    {
        await _userTopicService.ReduceUserTopicQuestionCountAsync(sessionId, userTopicId);
        var userTopic = await _userTopicService.GetUserTopicAsync(sessionId, userTopicId);
        if (userTopic is not null && userTopic.Count == 0)
        {
            _logger.LogInformation($"Сессия: {sessionId}; Топик: {userTopicId}; Статус: Закрытие по количеству вопросов.");
            await _userTopicService.CloseUserTopicAsync(userTopicId);
        }
    }
}