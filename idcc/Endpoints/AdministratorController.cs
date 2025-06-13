using idcc.Dtos.AdminDto;
using idcc.Service;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/administator")]
public class AdministatorController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly ILogger<AdministatorController> _logger;

    public AdministatorController(IQuestionService questionService, ILogger<AdministatorController> logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    [HttpPost]
    [Route("question/create")]
    public async Task<IResult> Create([FromBody] List<QuestionAdminDto> questions)
    {
        var notAddedQuestions = await _questionService.CreateAsync(questions);

        return Results.Ok(notAddedQuestions);
    }
}