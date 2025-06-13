using rokis.Dtos;
using rokis.Extensions;
using rokis.Infrastructures;
using rokis.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace rokis.Endpoints;

/// <summary>
/// Работа с вопросами и ответами во время прохождения теста.
/// </summary>
[ApiController]
[Route("api/question")]
[Authorize(Roles = "Employee,Person")]
public class QuestionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IUserTopicService _userTopicService;
    private readonly IQuestionService _questionService;
    private readonly IScoreService _scoreService;

    public QuestionController(ISessionService sessionService,
        IUserTopicService userTopicService,
        IQuestionService questionService,
        IScoreService scoreService)
    {
        _sessionService = sessionService;
        _userTopicService = userTopicService;
        _questionService = questionService;
        _scoreService = scoreService;
    }

    // ════════════════════════════════════════════════════════════════
    //          GET /api/v1/question
    // ════════════════════════════════════════════════════════════════
    /// <summary>Получает случайный вопрос рандомной (открытой) темы.</summary>
    /// <remarks>
    /// <para><b>Назначение:</b>Клиент вызывает метод, чтобы показать
    /// пользователю следующий вопрос.<br/>
    /// Если открытых тем больше нет — сессия закрывается и возвращается 204.</para>
    /// Возможные бизнес-ошибки:<br/>
    /// • <c>Cессия не найдена</c><br/>
    /// • <c>Сессия не завершена</c>
    /// </remarks>
    /// <param name="tokenId">
    /// GUID токена.</param>
    /// <returns>Вопрос с вариантами ответов или код 204/400.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(QuestionDto),  StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string),      StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status423Locked)]
    public async Task<IActionResult> GetRandom([FromQuery] Guid tokenId)
    {
        var session = await _sessionService.GetActualSessionAsync(tokenId);

        if (session == null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_NOT_EXIST, MessageCode.SESSION_IS_NOT_EXIST.GetDescription()));
        }

        if (session.EndTime != null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_FINISHED, MessageCode.SESSION_IS_FINISHED.GetDescription()));
        }

        QuestionDto? questionDto = null;
        UserTopicDto? userTopicDto = null;
        var cancelTokenSource = new CancellationTokenSource();
        while (!cancelTokenSource.Token.IsCancellationRequested)
        {
            if (!await _userTopicService.HasOpenTopic(session.Id))
            {
                await _sessionService.EndSessionAsync(tokenId);
                return NoContent();
            }

            userTopicDto = await _userTopicService.GetRandomUserTopicAsync(session.Id);
            if (userTopicDto == null)
            {
                return BadRequest(new ResponseDto(MessageCode.GET_RANDOM_TOPIC, MessageCode.GET_RANDOM_TOPIC.GetDescription()));
            }

            questionDto = await _questionService.GetQuestionAsync(session.Id, userTopicDto.Id, userTopicDto.Grade.Id);
            if (questionDto != null)
            {
                await cancelTokenSource.CancelAsync();
                continue;
            }
            await _userTopicService.CloseUserTopicAsync(userTopicDto.Id);
            return Accepted();
        }

        if (userTopicDto == null)
        {
            return BadRequest(new ResponseDto(MessageCode.TOPIC_IS_NULL, MessageCode.TOPIC_IS_NULL.GetDescription()));
        }
        await _userTopicService.RefreshActualTopicInfoAsync(userTopicDto.Id, session.Id);
        return Ok(questionDto);
    }

    // ════════════════════════════════════════════════════════════════
    //           POST /api/v1/question/answers
    // ════════════════════════════════════════════════════════════════
    /// <summary>Отправляет ответы на вопрос и пересчитывает баллы.</summary>
    /// <remarks>
    /// <b>Сценарий:</b>Клиент отправляет выбранные ответы и время,
    /// сервис возвращает 200 при успехе или 400 с сообщением об ошибке.
    /// Возможные бизнес-ошибки:<br/>
    /// • <c>Cессия не найдена</c><br/>
    /// • <c>Сессия не завершена</c>
    /// </remarks>
    /// <param name="tokenId">GUID токена.</param>
    /// <param name="dateInterval">Время ответа; <c>hh:mm:ss</c>.</param>
    /// <param name="question">ID вопроса и список выбранных answerId.</param>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [HttpPost("answers")]
    public async Task<IActionResult> SendAnswers(
        [FromQuery] Guid tokenId,
        [FromQuery] TimeSpan dateInterval,
        [FromBody]  QuestionShortDto question)
    {
        var session = await _sessionService.GetActualSessionAsync(tokenId);

        if (session == null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_NOT_EXIST, MessageCode.SESSION_IS_NOT_EXIST.GetDescription()));
        }

        if (session.EndTime != null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_FINISHED, MessageCode.SESSION_IS_FINISHED.GetDescription()));
        }

        var res = await _scoreService.CalculateScoreAsync(
                      session,
                      (int)dateInterval.TotalSeconds,
                      question.Id,
                      question.Answers.Select(a => a.Id).ToList());

        if (res.error != null)
        {
            return BadRequest(new ResponseDto(res.code, res.error));
        }

        res = await _scoreService.CalculateTopicWeightAsync(session);
        return res.error != null ? BadRequest(new ResponseDto(res.code, res.error)) : Ok(new ResponseDto(res.code,"The answer is sent"));
    }
}
