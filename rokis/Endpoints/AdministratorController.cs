using rokis.Dtos.AdminDto;
using rokis.Service;
using Microsoft.AspNetCore.Mvc;
using rokis.Filters;

namespace rokis.Endpoints;

[ApiController]
[Route("administator")]
[ServiceFilter(typeof(AdminSecretFilter))]
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