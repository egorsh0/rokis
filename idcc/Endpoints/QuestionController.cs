using idcc.Application.Interfaces;
using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

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

    // -------- GET /api/v1/question ---------------------------------
    [HttpGet]
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

    // -------- POST /api/v1/question/answers -------------------------
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
