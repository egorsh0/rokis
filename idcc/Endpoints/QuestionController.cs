using idcc.Application.Interfaces;
using idcc.Dtos;
using idcc.Extensions;
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
        var session = await _sessionRepository.GetActualSessionAsync(tokenId);

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
            if (!await _topicRepository.HasOpenTopic(session))
            {
                await _sessionRepository.EndSessionAsync(tokenId);
                return NoContent();
            }

            userTopicDto = await _topicRepository.GetRandomTopicAsync(session);
            if (userTopicDto == null)
            {
                return BadRequest(new ResponseDto(MessageCode.GET_RANDOM_TOPIC, MessageCode.GET_RANDOM_TOPIC.GetDescription()));
            }

            questionDto = await _questionRepository.GetQuestionAsync(userTopicDto);
            if (questionDto != null)
            {
                await cancelTokenSource.CancelAsync();
                continue;
            }
            await _topicRepository.CloseTopicAsync(userTopicDto.Id);
            return Accepted();
        }

        if (userTopicDto == null)
        {
            return BadRequest(new ResponseDto(MessageCode.TOPIC_IS_NULL, MessageCode.TOPIC_IS_NULL.GetDescription()));
        }
        await _topicRepository.RefreshActualTopicInfoAsync(userTopicDto.Id, session);
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
        var session = await _sessionRepository.GetActualSessionAsync(tokenId);

        if (session == null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_NOT_EXIST, MessageCode.SESSION_IS_NOT_EXIST.GetDescription()));
        }

        if (session.EndTime != null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_FINISHED, MessageCode.SESSION_IS_FINISHED.GetDescription()));
        }

        var res = await _idccApplication.CalculateScoreAsync(
                      session,
                      (int)dateInterval.TotalSeconds,
                      question.Id,
                      question.Answers.Select(a => a.Id).ToList());

        if (res.error != null)
        {
            return BadRequest(new ResponseDto(res.code, res.error));
        }

        res = await _idccApplication.CalculateTopicWeightAsync(session);
        return res.error != null ? BadRequest(new ResponseDto(res.code, res.error)) : Ok(new ResponseDto(res.code,"The answer is sent"));
    }
}
