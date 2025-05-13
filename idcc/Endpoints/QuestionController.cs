using idcc.Application.Interfaces;
using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>
/// Работа с вопросами и ответами во время прохождения теста.
/// </summary>
[ApiController]
[Route("api/question")]
[Authorize(Roles = "Employee,Person")]
public class QuestionController : ControllerBase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserTopicRepository _topicRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IIdccApplication _idccApplication;

    public QuestionController(ISessionRepository sessionRepository,
                              IUserTopicRepository topicRepository,
                              IQuestionRepository questionRepository,
                              IIdccApplication idccApplication)
    {
        _sessionRepository = sessionRepository;
        _topicRepository = topicRepository;
        _questionRepository = questionRepository;
        _idccApplication = idccApplication;
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
    /// <param name="sessionId">
    /// ID активной сессии. Если отсутствует — берётся фактическая сессия по <paramref name="tokenId"/>.
    /// </param>
    /// <param name="tokenId">
    /// GUID токена. Используется, когда <paramref name="sessionId"/> не указан.
    /// </param>
    /// <returns>Вопрос с вариантами ответов или код 204/400.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(QuestionDto),  StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string),      StatusCodes.Status400BadRequest)]
    
    public async Task<IActionResult> GetRandom([FromQuery] int? sessionId,
                                               [FromQuery] Guid tokenId)
    {
        var session = sessionId.HasValue
            ? await _sessionRepository.GetSessionAsync(sessionId.Value)
            : await _sessionRepository.GetActualSessionAsync(tokenId);

        if (session == null)
        {
            return BadRequest(ErrorMessages.SESSION_IS_NOT_EXIST);
        }

        if (session.EndTime != null)
        {
            return BadRequest(ErrorMessages.SESSION_IS_FINISHED);
        }

        if (!await _topicRepository.HasOpenTopic(session))
        {
            await _sessionRepository.EndSessionAsync(session.Id, false);
            return NoContent();
        }

        var userTopic = await _topicRepository.GetRandomTopicAsync(session);
        if (userTopic == null)
        {
            return BadRequest(ErrorMessages.GET_RANDOM_TOPIC);
        }

        var question = await _questionRepository.GetQuestionAsync(userTopic);
        if (question == null)
        {
            await _topicRepository.CloseTopicAsync(userTopic.Id);
            return Accepted();
        }

        await _topicRepository.RefreshActualTopicInfoAsync(userTopic.Id, session);
        return Ok(question);
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
    /// <param name="sessionId">ID сессии (опц.).</param>
    /// <param name="tokenId">GUID токена (обязателен, если нет sessionId).</param>
    /// <param name="dateInterval">Время ответа; <c>hh:mm:ss</c>.</param>
    /// <param name="question">ID вопроса и список выбранных answerId.</param>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [HttpPost("answers")]
    public async Task<IActionResult> SendAnswers(
        [FromQuery] int? sessionId,
        [FromQuery] Guid tokenId,
        [FromQuery] TimeSpan dateInterval,
        [FromBody]  QuestionShortDto question)
    {
        var session = sessionId.HasValue
            ? await _sessionRepository.GetSessionAsync(sessionId.Value)
            : await _sessionRepository.GetActualSessionAsync(tokenId);

        if (session == null)
        {
            return BadRequest(ErrorMessages.SESSION_IS_NOT_EXIST);
        }

        if (session.EndTime != null)
        {
            return BadRequest(ErrorMessages.SESSION_IS_FINISHED);
        }

        var res = await _idccApplication.CalculateScoreAsync(
                      session,
                      (int)dateInterval.TotalSeconds,
                      question.Id,
                      question.Answers.Select(a => a.Id).ToList());

        if (res != null)
        {
            return BadRequest(res);
        }

        res = await _idccApplication.CalculateTopicWeightAsync(session);
        return res != null ? BadRequest(res) : Ok();
    }
}
