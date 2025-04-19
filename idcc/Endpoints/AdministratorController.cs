using idcc.Dtos.AdminDto;
using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/v1/administator")]
public class AdministatorController : ControllerBase
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ILogger<OrdersController> _logger;

    public AdministatorController(IQuestionRepository questionRepository, ILogger<OrdersController> logger)
    {
        _questionRepository = questionRepository;
        _logger = logger;
    }

    [HttpPost]
    [Route("question/create")]
    [Authorize("Administrator")]
    public async Task<IResult> Create([FromBody] List<QuestionAdminDto> questions)
    {
        if (!questions.Any())
        {
            return Results.BadRequest(ErrorMessages.QUESTIONS_IS_EMPTY);
        }
        var notAddedQuestions = await _questionRepository.CreateAsync(questions);

        return Results.Ok(notAddedQuestions);
    }
}